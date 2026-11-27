// Space.cs — how much room the encoded cache takes, and a way to free it and bring it back.
// data\songs\<id>\ holds song.wav (48 kHz PCM, the source for the wave editor and the preview),
// enc\ (what the EA encoder produced) and cover.png. All of it can be rebuilt.
//
// Freeing "all" used to clear the song's Music signature, which is the signal the window uses to import a song
// again — so the window re-imported everything at once and nothing was freed in practice. Now the song is only
// marked noAudio: the window leaves it alone, the build says it needs the audio back, and "restore" clears the
// mark and the signature, so the import happens when the user asks for it.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

static class Space
{
    // { enc, audio, cover, total, marked } — bytes, and how many songs are marked "no audio"
    public static Dictionary<string, object> Size()
    {
        long enc = 0, audio = 0, cover = 0;
        int marked = 0;
        foreach (var s in Settings.Get().songs) if (!s.draft && s.noAudio) marked++;
        foreach (string d in Dirs())
        {
            string id = Path.GetFileName(d);
            if (id == null) continue;
            string e = Path.Combine(d, "enc");
            if (Directory.Exists(e)) enc += Weigh(e);
            string w = Path.Combine(d, "song.wav");
            if (File.Exists(w)) audio += Length(w);
            string c = Path.Combine(d, "cover.png");
            if (File.Exists(c)) cover += Length(c);
        }
        return new Dictionary<string, object> {
            { "enc", enc }, { "audio", audio }, { "cover", cover }, { "total", enc + audio + cover }, { "marked", marked } };
    }

    // what = "enc" (only the encoder output) | "all" (the whole per-song folder) | "restore" (make them importable again)
    // only — ids to touch (empty/null: every song)
    public static Dictionary<string, object> Clean(string what, IEnumerable<string> only)
    {
        if (what != "enc" && what != "all" && what != "restore") throw new UserError("cleanWhat", what);
        var want = new HashSet<string>((only ?? new string[0]).Where(x => x != null), StringComparer.Ordinal);
        var picked = Settings.Get().songs.Where(s => !s.draft && (want.Count == 0 || want.Contains(s.id))).ToList();

        if (what == "restore")
        {
            var back = picked.Where(s => s.noAudio && s.src != null).ToList();
            if (back.Count > 0)
                Settings.Edit(x =>
                {
                    foreach (var s in back)
                    {
                        var t = x.songs.FirstOrDefault(y => y.id == s.id);
                        if (t == null) continue;
                        t.noAudio = false;
                        t.asig = "";             // the window imports a song again when its Music signature no longer matches
                    }
                });
            App.Log("restore: " + back.Count + " songs are imported again");
            return new Dictionary<string, object> { { "ok", true }, { "what", what }, { "restored", back.Count } };
        }

        var songs = picked.Where(s => s.src != null && !s.noAudio).ToList();   // no src -> nowhere to rebuild from; never touch it
        int skipped = picked.Count(s => s.src == null);
        long freed = 0;
        int done = 0, failed = 0;
        var dropped = new List<string>();
        foreach (var s in songs)
        {
            string d = Path.Combine(App.SongsDir, s.id);
            if (!Directory.Exists(d)) continue;
            string target = what == "enc" ? Path.Combine(d, "enc") : d;
            if (!Directory.Exists(target)) continue;
            long before = Weigh(target);
            try
            {
                Directory.Delete(target, true);
                freed += before; done++;
                if (what == "all") dropped.Add(s.id);
            }
            catch (Exception e)
            {
                failed++;
                App.Log("clean " + target + ": " + e.Message);   // still open in the preview, or a locked file
            }
        }
        if (dropped.Count > 0)
            Settings.Edit(x =>
            {
                foreach (var id in dropped)
                {
                    var s = x.songs.FirstOrDefault(y => y.id == id);
                    if (s != null) s.noAudio = true;                // stays freed until "restore"
                }
            });
        App.Log("clean " + what + ": freed " + freed + " bytes, songs " + done + ", failed " + failed + ", skipped " + skipped);
        return new Dictionary<string, object> {
            { "ok", true }, { "what", what }, { "freed", freed }, { "songs", done }, { "failed", failed }, { "skipped", skipped },
            { "marked", dropped.Count } };
    }

    static string[] Dirs()
    {
        try { return Directory.Exists(App.SongsDir) ? Directory.GetDirectories(App.SongsDir) : new string[0]; }
        catch (Exception e) { App.Log("songs dir: " + e.Message); return new string[0]; }
    }
    static long Length(string f)
    {
        try { return new FileInfo(f).Length; } catch { return 0; }
    }
    static long Weigh(string dir)
    {
        long sum = 0;
        try
        {
            foreach (string f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                sum += Length(f);
        }
        catch (Exception e) { App.Log("weigh " + dir + ": " + e.Message); }
        return sum;
    }
}
