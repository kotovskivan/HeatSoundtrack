// Space.cs — how much room the encoded cache takes, and a way to free it.
// data\songs\<id>\ holds song.wav (48 kHz PCM, the source for the wave editor and the preview),
// enc\ (what the EA encoder produced) and cover.png. Everything here can be rebuilt:
// enc\ on the next build, song.wav and cover.png by importing the song again (the window decodes it).
// A song without src (never came from the Music folder) has nowhere to be rebuilt from, so it is skipped.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

static class Space
{
    // { enc, audio, cover, total } in bytes
    public static Dictionary<string, object> Size()
    {
        long enc = 0, audio = 0, cover = 0;
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
            { "enc", enc }, { "audio", audio }, { "cover", cover }, { "total", enc + audio + cover } };
    }

    // what = "enc" (only the encoder output) | "all" (the whole per-song folder, songs are re-imported later).
    // only — ids to touch (empty/null: every song). Returns { freed, songs, skipped, failed }.
    public static Dictionary<string, object> Clean(string what, IEnumerable<string> only)
    {
        if (what != "enc" && what != "all") throw new UserError("cleanWhat", what);
        var st = Settings.Get();
        var want = new HashSet<string>((only ?? new string[0]).Where(x => x != null), StringComparer.Ordinal);
        var songs = st.songs.Where(s => !s.draft && s.src != null && (want.Count == 0 || want.Contains(s.id))).ToList();
        int skipped = want.Count == 0 ? st.songs.Count(s => !s.draft && s.src == null) : 0;   // nowhere to rebuild from
        long freed = 0;
        int done = 0, failed = 0;
        var reimport = new List<string>();
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
                if (what == "all") reimport.Add(s.id);
            }
            catch (Exception e)
            {
                failed++;
                App.Log("clean " + target + ": " + e.Message);   // still open in the preview, or a locked file
            }
        }
        // the window imports a song again when its Music signature no longer matches: clearing it is the signal
        if (reimport.Count > 0)
            Settings.Edit(x => { foreach (var id in reimport) { var s = x.songs.FirstOrDefault(y => y.id == id); if (s != null) s.asig = ""; } });
        App.Log("clean " + what + ": freed " + freed + " bytes, songs " + done + ", failed " + failed + ", skipped " + skipped);
        return new Dictionary<string, object> {
            { "ok", true }, { "what", what }, { "freed", freed }, { "songs", done }, { "failed", failed }, { "skipped", skipped },
            { "reimport", reimport.Count } };
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
