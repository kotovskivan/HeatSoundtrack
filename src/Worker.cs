// Рабочий процесс: "Heat Soundtrack.exe --worker job.json". Пишет в stdout строки для окна:
//   STEP <ключ> [текст]   — шаг (ключ переводится в окне)
//   PCT <0..100>          — прогресс шага
//   ERR <ключ> [текст]    — ошибка, понятная человеку
//   OK <json>             — готово
// Задачи: extract — список треков игры по контекстам и фоны из игры игрока (в zip нет файлов EA); build — собрать мод.
//
// Музыка NFS Heat (разбор 13.11.2026, igry/nfs-heat/README.md; тест в игре — tools/heat-reader testmod, «работает»):
//   песня = NewWaveAsset (EBX, 1 чанк звука EA Layer3) + NewWaveResource (RES — SBle как в Unbound, ключ = djb2 от GUID корня);
//   контексты = списки Playables во фразах (MusicPhraseData) музыкальных ассетов: гараж Play_Garage (фразы P_<песня> -> сегмент),
//   город Day / Night и гонка PlayGenericRace (сегменты MusicSegmentData прямо в списке). Своя песня = НОВЫЙ ассет звука + новые
//   сегмент (+ фраза в гараже) и MusicMetadata — дописываются в список: треки игры остаются (не замена). Выключить трек игры — убрать из списка.
//   Старт гонки (PlayRaceIntro: вступление из 6 кусков) — не трогаем (v2).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using FrostySdk.Resources;

class WLog : ILogger
{
    int lastPct = -1;
    public void Log(string t, params object[] v)
    {
        string s = Fmt(t, v);
        if (s.StartsWith("progress:"))
        {
            double d; if (double.TryParse(s.Substring(9).Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
            { int p = (int)d; if (p != lastPct) { lastPct = p; Worker.Say("PCT " + p); } }
            return;
        }
        App.Log("frosty: " + s);
    }
    public void LogWarning(string t, params object[] v) { App.Log("frosty WARN " + Fmt(t, v)); }
    public void LogError(string t, params object[] v) { App.Log("frosty ERR " + Fmt(t, v)); }
    static string Fmt(string t, object[] v) { try { return v == null || v.Length == 0 ? t : string.Format(t, v); } catch { return t; } }
}

static class Worker
{
    const string GARAGE = "audio/music/roboto/roboto_licensedmusic_garage";
    const string FREEDRIVE = "audio/music/roboto/roboto_music_freedrive";
    const string RACEDAY = "audio/music/roboto/roboto_licensedmusic_raceday";
    const string RACENIGHT = "audio/music/roboto/roboto_licensedmusic_racenight";
    // образец ассета звука — песня города: ее бандлы (gamelogic/music + robotoprogression) покрывают все контексты (у гаража / гонки — только gamelogic/music)
    const string WAVE_TPL = "audio/music/roboto/playlist/dc_inna_tumanera";
    const string GA_PHR = "P_Tycho_Easy";   // образец фразы гаража (сегмент + 2 MusicMetadata + FollowOnPlayable -> петля гаража) — проверено в игре 13.11
    const uint SEG_LEN = 0x6CFCCE5B, CHK_SIZE = 0xDC19107B, CHK_GUID = 0xF4369173;   // поля SBle (как в Unbound, Sble.cs)

    // контексты своей песни: ключ = Song.Ctx, ассет музыки, фраза-список; Phrase — в списке фразы (гараж), иначе сегменты
    class CtxDef { public string Key, Asset, List; public bool Phrase; }
    static readonly CtxDef[] CTX = {
        new CtxDef { Key = "garage", Asset = GARAGE, List = "Play_Garage", Phrase = true },
        new CtxDef { Key = "cityDay", Asset = FREEDRIVE, List = "Day" },
        new CtxDef { Key = "cityNight", Asset = FREEDRIVE, List = "Night" },
        new CtxDef { Key = "raceDay", Asset = RACEDAY, List = "PlayGenericRace" },
        new CtxDef { Key = "raceNight", Asset = RACENIGHT, List = "PlayGenericRace" } };

    // фоны окна: имя в игре -> файл (не BC7 / нет в игре — пропускается, окно живет без него)
    static readonly string[][] BG = {
        new[] { "ui/textures/bootflow/loadingscreen4k", "photo.jpg", "1920" },
        new[] { "ui/textures/onboarding/onboard_background", "leak.jpg", "1920" },
        new[] { "ui/textures/common/nfs_simple_logo", "logo.png", "0" } };

    static AssetManager am;
    static Dictionary<string, object> job;

    public static void Say(string s) { Console.WriteLine(s); Console.Out.Flush(); App.Log("worker> " + s); }
    // прогресс по песням (счетчик + полоса в окне) — только окну, без log.txt (иначе +сотни строк на сборку)
    static void Tell(string step, int done, int all, bool log = false) { if (log) App.Log("worker> STEP " + step); Console.WriteLine("STEP " + step); Console.WriteLine("PCT " + (100 * done / Math.Max(1, all))); Console.Out.Flush(); }

    public static int Run(string jobPath)
    {
        try { Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true }); } catch { }
        try
        {
            job = App.Json.Deserialize<Dictionary<string, object>>(File.ReadAllText(jobPath, Encoding.UTF8));
            string task = (string)job["task"];
            var mx = new System.Threading.Mutex(false, MutexName);
            bool got; try { got = mx.WaitOne(0); } catch (System.Threading.AbandonedMutexException) { got = true; }   // прошлый упал — мьютекс наш
            if (!got) { mx.Dispose(); throw new UserError("busyOld"); }
            busyMutex = mx;
            object par; if (job.TryGetValue("parent", out par) && par != null) WatchParent(Convert.ToInt32(par));
            App.Log("worker start: " + task);
            if (task == "extract") Extract();
            else if (task == "build") Build();
            else throw new Exception("unknown task " + task);
            return 0;
        }
        catch (UserError e) { App.Log("worker user error: " + e); Say("ERR " + e.Key + (e.Arg != null ? " " + e.Arg.Replace("\n", " ") : "")); return 2; }
        catch (Exception e)
        {
            App.Log("worker crash: " + e);
            // P2-01: нет Visual C++ (FrostyHash.dll / libzstd / zlibwapi не грузятся; Oodle игры не загрузился — Frosty падает NullReference
            // в DecompressOodle) — окно предложит установщик из app\vcredist
            for (var x = e; x != null; x = x.InnerException)
                if ((x is FileNotFoundException && x.Message.Contains("FrostyHash") && File.Exists(Path.Combine(App.FrostyDir, "FrostyHash.dll")))
                    || (x is DllNotFoundException && (x.Message.Contains("zstd") || x.Message.Contains("zlib")))
                    || (x is NullReferenceException && (x.StackTrace ?? "").Contains("Oodle")))
                { Say("ERR vcMissing " + x.GetType().Name); return 1; }
            Say("ERR crash " + e.GetType().Name + ": " + e.Message.Replace("\n", " ")); return 1;
        }
        finally { if (busyMutex != null) try { busyMutex.ReleaseMutex(); } catch { } }
    }

    // программа закрылась / ее убили — выходим и мы (и гасим кодировщик Tool.exe). Мод в Mods цел: новый подменяет старый в самом конце (.part)
    static void WatchParent(int pid)
    {
        System.Diagnostics.Process parent;
        try { parent = System.Diagnostics.Process.GetProcessById(pid); } catch { App.Log("worker: program already closed - stopping"); Environment.Exit(3); return; }
        new System.Threading.Thread(() =>
        {
            try { parent.WaitForExit(); } catch { return; }
            App.Log("worker: program closed - stopping");
            try { var c = Audio.Current; if (c != null && !c.HasExited) c.Kill(); } catch { }
            Environment.Exit(3);
        }) { IsBackground = true }.Start();
    }

    // рабочий процесс от прошлого запуска еще жив — новая программа не запускает второй поверх
    public static string MutexName { get { return "HeatSoundtrack-worker-" + (App.Root.ToLowerInvariant().GetHashCode() & 0x7fffffff); } }
    static System.Threading.Mutex busyMutex;

    // ---------- загрузка игры через Frosty ----------
    static void Load()
    {
        string game = (string)job["game"], fmm = (string)job["fmm"];
        if (!Find.IsGame(game)) throw new UserError("noGame");
        if (!Find.IsFmm(fmm)) throw new UserError("noFmm");
        string keyPath = Find.KeyPath(fmm);
        if (!File.Exists(keyPath)) throw new UserError("noKey", fmm);
        Directory.SetCurrentDirectory(App.FrostyDir);
        string caches = Path.Combine(App.FrostyDir, "Caches"); Directory.CreateDirectory(caches);
        string myCache = Path.Combine(caches, "NFSHEAT.cache"), fmmCache = Path.Combine(fmm, "Caches", "NFSHEAT.cache");
        bool fresh = false;
        if (!File.Exists(myCache) && File.Exists(fmmCache))
        {
            Say("STEP copyCache");
            try { File.Copy(fmmCache, myCache + ".tmp", true); File.Move(myCache + ".tmp", myCache); } catch (Exception e) { App.Log("copy cache: " + e.Message); }
        }
        if (!File.Exists(myCache)) fresh = true;
        Say(fresh ? "STEP firstLoad" : "STEP loadGame");

        File.WriteAllText(Path.Combine(App.FrostyDir, "config.json"),
            App.Json.Serialize(new Dictionary<string, object> {
                { "Games", new Dictionary<string, object> { { App.Profile, new Dictionary<string, object> { { "GamePath", game }, { "Options", new Dictionary<string, object>() }, { "Packs", new Dictionary<string, object>() } } } } },
                { "GlobalOptions", new Dictionary<string, object>() } }), new UTF8Encoding(false));

        var lg = new WLog();
        ProfilesLibrary.Initialize(new List<Profile>());
        if (!ProfilesLibrary.SelectProfile(App.Profile)) throw new Exception("profile " + App.Profile + " not selected");
        // ключ Heat составной (16 416 Б = Key1 16 + Key2 16 + Key3 16384, ключ с Nexus 74); 16 Б — один ключ на все три
        byte[] raw = File.ReadAllBytes(keyPath);
        if (raw.Length < 16) throw new UserError("noKey", fmm);
        byte[] key = raw.Take(16).ToArray();
        KeyManager.Instance.AddKey("Key1", key);
        if (raw.Length >= 32 + 0x4000) { KeyManager.Instance.AddKey("Key2", raw.Skip(16).Take(16).ToArray()); KeyManager.Instance.AddKey("Key3", raw.Skip(32).Take(0x4000).ToArray()); }
        else { KeyManager.Instance.AddKey("Key2", key); KeyManager.Instance.AddKey("Key3", key); }
        TypeLibrary.Initialize();
        var fs = new FileSystemManager(game.TrimEnd('\\') + "\\");
        foreach (FileSystemSource s in ProfilesLibrary.Sources) fs.AddSource(s.Path, s.SubDirs);
        fs.Initialize(key);
        var rm = new ResourceManager(fs); rm.SetLogger(lg); rm.Initialize();
        am = new AssetManager(fs, rm); am.SetLogger(lg);
        am.Initialize(false, null);
        Frosty.Core.App.FileSystemManager = fs; Frosty.Core.App.ResourceManager = rm; Frosty.Core.App.AssetManager = am;
        Frosty.Core.App.Logger = new Lg();
        App.Log("game loaded: ebx " + am.GetEbxCount());
        foreach (string a in new[] { GARAGE, FREEDRIVE, RACEDAY, RACENIGHT, WAVE_TPL })
            if (am.GetEbxEntry(a) == null) throw new UserError("gameChanged", "no " + a);
        if (am.GetResEntry(WAVE_TPL) == null) throw new UserError("gameChanged", "no res " + WAVE_TPL);
    }

    // ---------- extract: треки игры по контекстам + фоны окна ----------
    static void Extract()
    {
        Load();
        Say("STEP readTracks");
        var tracks = new List<object>();
        var assets = new Dictionary<string, EbxAsset>();
        foreach (var c in CTX)
        {
            EbxAsset a; if (!assets.TryGetValue(c.Asset, out a)) assets[c.Asset] = a = am.GetEbx(am.GetEbxEntry(c.Asset));
            int i = 0;
            foreach (object it in Items(Phrase(a, c.List)))
            {
                string name = Str(it, "Name");
                object seg = c.Phrase ? FirstSegment(it) : it;
                var nm = Pretty(seg != null ? Str(seg, "Name") : name);
                double dur = 0;
                try { dur = WaveSeconds(seg); } catch (Exception e) { App.Log("track length " + name + ": " + e.Message); }
                tracks.Add(new Dictionary<string, object> {
                    { "slot", Slot(c.Key, name) }, { "ctx", c.Key }, { "name", name }, { "artist", nm.Item1 }, { "title", nm.Item2 },
                    { "dur", Math.Round(dur, 1) }, { "order", i++ } });
            }
        }
        string ui = App.UiDir; Directory.CreateDirectory(Path.Combine(ui, "bg"));
        Say("STEP backgrounds");
        int k = 0;
        foreach (var b in BG)
        {
            Say("PCT " + (100 * k++ / BG.Length));
            try { SaveTexture(b[0], Path.Combine(ui, "bg", b[1]), int.Parse(b[2]), b[1].EndsWith(".png")); }
            catch (Exception e) { App.Log("background " + b[0] + ": " + e.Message); }
        }
        App.WriteJson(Path.Combine(ui, "tracks.json"), new Dictionary<string, object> {
            { "fingerprint", (string)job["fingerprint"] }, { "app", App.Version }, { "tracks", tracks }, { "when", DateTime.Now.ToString("s") } });
        Say("OK {\"tracks\":" + tracks.Count + "}");
    }

    public static string Slot(string ctx, string name) { return ctx + ":" + name; }

    // имя трека игры из имени сегмента: «DC_FelixJaehnBreakingBeattz_LIITA» -> («Felix Jaehn Breaking Beattz», «LIITA»)
    static Tuple<string, string> Pretty(string seg)
    {
        string s = System.Text.RegularExpressions.Regex.Replace(seg ?? "", @"^(S_)?(VS_)?(GA|DC|NC|DI|NI)_", "");
        string[] parts = s.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return Tuple.Create("", seg ?? "");
        Func<string, string> sp = x => System.Text.RegularExpressions.Regex.Replace(x, @"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ").Trim();
        if (parts.Length == 1) return Tuple.Create("", sp(parts[0]));
        return Tuple.Create(sp(parts[0]), string.Join(" ", parts.Skip(1).Select(sp)));
    }

    // длина песни игры: поле длины сегмента в RES ее ассета звука (SBle)
    static Dictionary<Guid, string> waveByGuid;
    static double WaveSeconds(object seg)
    {
        if (seg == null) return 0;
        var w = (PointerRef)Get(seg, "Wave");
        if (w.Type != PointerRefType.External) return 0;
        if (waveByGuid == null)   // ссылки на ассеты по GUID Frosty без полной индексации не находит — своя карта по папке музыки
        {
            waveByGuid = new Dictionary<Guid, string>();
            foreach (EbxAssetEntry ae in am.EnumerateEbx().Where(x => x.Name.StartsWith("audio/music/roboto/playlist/")).ToList())
                try { waveByGuid[am.GetEbx(ae).FileGuid] = ae.Name; } catch { }
        }
        string wn; if (!waveByGuid.TryGetValue(w.External.FileGuid, out wn)) return 0;
        ResAssetEntry r = am.GetResEntry(wn);
        if (r == null) return 0;
        byte[] b; using (Stream s = am.GetRes(r)) using (var ms = new MemoryStream()) { s.CopyTo(ms); b = ms.ToArray(); }
        var bank = SbleBank.Read(b);
        return BitConverter.ToSingle(BitConverter.GetBytes((uint)bank.F(SbleBank.DS_SEG, SEG_LEN).Vals[0]), 0);
    }

    static void SaveTexture(string ebxName, string path, int maxW, bool alpha)
    {
        EbxAssetEntry e = am.GetEbxEntry(ebxName);
        if (e == null) throw new Exception("missing " + ebxName);
        dynamic root = am.GetEbx(e).RootObject;
        ResAssetEntry re = am.GetResEntry((ulong)root.Resource);
        Texture t = am.GetResAs<Texture>(re);
        string fmt = t.PixelFormat.ToString();   // в Heat фоны — BC1 / BC3 (Bcn.cs), BC7 — как в Unbound
        bool bc7 = fmt.StartsWith("BC7"), bc1 = fmt.StartsWith("BC1"), bc3 = fmt.StartsWith("BC3");
        if (!bc7 && !bc1 && !bc3) throw new Exception("format " + fmt);
        byte[] data; using (var ms = new MemoryStream()) { t.Data.Position = 0; t.Data.CopyTo(ms); data = ms.ToArray(); }
        int w = t.Width, h = t.Height;
        int mip0 = ((w + 3) / 4) * ((h + 3) / 4) * (bc1 ? 8 : 16);
        if (data.Length < mip0) throw new Exception("not enough data " + data.Length + " < " + mip0);
        Img.Save(bc7 ? Bc7.Decode(data, w, h) : Bcn.Decode(data, w, h, bc3), w, h, path, maxW, alpha);
    }

    // ---------- build ----------
    static void Build()
    {
        var songs = ((ArrayList)job["songs"]).Cast<Dictionary<string, object>>().Select(d => App.Json.ConvertToType<Song>(d)).ToList();
        var choiceRaw = (Dictionary<string, object>)job["choice"];
        var choice = choiceRaw.ToDictionary(k => k.Key, k => ((ArrayList)k.Value).Cast<object>().Select(Convert.ToBoolean).ToArray());
        string outPath = (string)job["out"];
        songs = songs.Where(s => s.Any()).ToList();

        // 1) звук своих песен
        var encDirs = new List<string>();
        int n = 0;
        foreach (var s in songs)
        {
            n++;
            Tell("encode " + n + "/" + songs.Count + " · " + Clean(s.title), n - 1, songs.Count);
            encDirs.Add(Audio.Prepare(s, piece => Say("STEP encode " + n + "/" + songs.Count + " · " + Clean(s.title) + " · " + piece)));
        }

        // 2) игра
        Load();
        var assets = new Dictionary<string, EbxAsset>();
        foreach (var c in CTX) if (!assets.ContainsKey(c.Asset)) assets[c.Asset] = am.GetEbx(am.GetEbxEntry(c.Asset));
        // образцы — до правок списков (трек-образец могут выключить)
        object gaPhr = ByName(assets[GARAGE], "MusicPhraseData", GA_PHR);
        var segTpl = new Dictionary<string, object>();
        foreach (var c in CTX) if (!c.Phrase) segTpl[c.Key] = Items(Phrase(assets[c.Asset], c.List)).First();

        // 3) треки игры: выключенные — вон из списка (нет в choice = включен)
        Say("STEP logic");
        var on = new Dictionary<string, int>();
        var origItems = new Dictionary<string, List<object>>();
        foreach (var c in CTX)
        {
            IList list = (IList)Get(Phrase(assets[c.Asset], c.List), "Playables");
            origItems[c.Key] = list.Cast<object>().ToList();
            int before = list.Count;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                bool[] f; string slot = Slot(c.Key, Str(((PointerRef)list[i]).Internal, "Name"));
                if (choice.TryGetValue(slot, out f) && f.Length > 0 && !f[0]) list.RemoveAt(i);
            }
            on[c.Key] = list.Count;
            if (before != list.Count) App.Log("ctx " + c.Key + ": game tracks " + before + " -> " + list.Count);
        }

        // 4) свои песни: ассет звука + сегмент / фраза в каждый выбранный контекст
        int own = 0;
        for (int i = 0; i < songs.Count; i++)
        {
            var s = songs[i];
            Tell("bank " + (i + 1) + "/" + songs.Count + " · " + Clean(s.title), i, songs.Count, true);
            s.trackName = TrackName(s, i + 1);
            byte[] sps = File.ReadAllBytes(Path.Combine(encDirs[i], "A.sps"));
            var wave = AddWave(s.trackName, sps);
            string title = Game(Music.Cut(Clean(s.title), Music.MaxTitle)), artist = Game(Music.Cut(Clean(s.artist), Music.MaxArtist));
            if (title == "") title = "Track " + (i + 1);
            bool[] fl = s.Flags();
            for (int k = 0; k < CTX.Length; k++)
            {
                if (!fl[k]) continue;
                var c = CTX[k];
                EbxAsset a = assets[c.Asset];
                object item = c.Phrase ? (object)NewPhrase(a, gaPhr, s.trackName, wave, title, artist) : NewSegment(a, segTpl[c.Key], "S_" + s.trackName, wave, title, artist);
                ((IList)Get(Phrase(a, c.List), "Playables")).Add(new PointerRef(item));
                a.AddDependency(wave.Item1);
                on[c.Key]++;
            }
            own++;
            App.Log("song " + s.trackName + " (" + artist + " - " + title + "): " + sps.Length + " B, contexts " + string.Join(",", CTX.Where((c, k) => fl[k]).Select(c => c.Key)));
        }

        // пустой список ломает выбор музыки — в таком контексте возвращаем треки игры как были (только этот список: день и ночь города — один ассет)
        foreach (var c in CTX.Where(c => on[c.Key] == 0))
        {
            IList list = (IList)Get(Phrase(assets[c.Asset], c.List), "Playables");
            list.Clear(); foreach (object x in origItems[c.Key]) list.Add(x);
            on[c.Key] = list.Count;
            App.Log("ctx " + c.Key + ": empty - game tracks restored (" + list.Count + ")");
        }
        foreach (var kv in assets) am.ModifyEbx(kv.Key, kv.Value);

        // 5) мод
        Say("STEP writeMod");
        var ms = new Frosty.Core.Mod.ModSettings();
        ms.Title = "Heat Soundtrack"; ms.Author = "Kitivski + Claude"; ms.Category = "Audio"; ms.Version = App.Version;
        ms.Description = (job.ContainsKey("preset") ? "Profile: " + Clean((string)job["preset"]) + ". " : "") + (songs.Count > 0 ? "Own songs: " + string.Join(", ", songs.Select(s => Clean(s.artist) + " - " + Clean(s.title))) + ". " : "")
            + "Tracks: garage " + on["garage"] + ", city day " + on["cityDay"] + ", city night " + on["cityNight"] + ", race day " + on["raceDay"] + ", race night " + on["raceNight"]
            + ". Made with Heat Soundtrack (fan-made, not affiliated with EA).";
        ms.Icon = ModIcon();
        Frosty.Core.Config.Load(Path.Combine(App.FrostyDir, "config.json"));
        Frosty.Core.App.SelectedProfile = App.Profile;
        if (Frosty.Core.App.PluginManager == null) Frosty.Core.App.PluginManager = new Frosty.Core.PluginManager(new Lg(), (Frosty.Core.PluginManagerType)0);
        string tmp = Path.Combine(App.DataDir, "build.fbmod");
        if (File.Exists(tmp)) File.Delete(tmp);
        new Frosty.Core.FrostyProject().WriteToMod(tmp, ms);
        if (!File.Exists(tmp) || new FileInfo(tmp).Length < 1000) throw new Exception("mod was not written");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        // копия рядом (.part — Mod Manager его не видит) и подмена одним шагом: остановили посреди копирования — старый мод в Mods цел
        string part = outPath + ".part";
        try { File.Copy(tmp, part, true); if (File.Exists(outPath)) File.Replace(part, outPath, null); else File.Move(part, outPath); }
        catch (IOException e) { App.Log("copy mod: " + e.Message); try { File.Delete(part); } catch { } throw new UserError("modLocked", outPath); }
        File.Delete(tmp);
        var res = new Dictionary<string, object> { { "path", outPath }, { "size", new FileInfo(outPath).Length }, { "songs", own } };
        foreach (var c in CTX) res[c.Key] = on[c.Key];
        Say("OK " + App.Json.Serialize(res));
    }

    // ассет звука своей песни: копия песни города (EBX NewWaveAsset + RES SBle) со своим чанком. Возвращает (GUID файла, GUID корня)
    static Tuple<Guid, Guid> AddWave(string trackName, byte[] sps)
    {
        if (sps.Length < 16 || BE32(sps, 0) != 0x4800000c || sps[4] != 0x16) throw new Exception(trackName + ": not EA Layer3 v2 PCM");
        uint rate = BE32(sps, 4) & 0x3FFFF, samples = BE32(sps, 8) & 0x3FFFFFFF;
        if (rate != Audio.SR) throw new Exception("sample rate " + rate);
        EbxAssetEntry srcE = am.GetEbxEntry(WAVE_TPL); ResAssetEntry srcR = am.GetResEntry(WAVE_TPL);
        byte[] orig; using (Stream st = am.GetRes(srcR)) using (var m = new MemoryStream()) { st.CopyTo(m); orig = m.ToArray(); }
        SbleBank bank = SbleBank.Read(orig);
        if (!bank.Write().SequenceEqual(orig)) throw new UserError("gameChanged", "wave round-trip " + WAVE_TPL);
        if (bank[SbleBank.DS_CHK].Count != 1 || bank[SbleBank.DS_SEG].Count != 1) throw new UserError("gameChanged", "wave layout " + WAVE_TPL);
        var gv = bank.F(SbleBank.DS_CHK, CHK_GUID).Vals;
        ChunkAssetEntry tc = am.GetChunkEntry(new Guid(bank.Data.Skip((int)gv[0] - 1).Take(16).ToArray()));
        if (tc == null) throw new UserError("gameChanged", "wave chunk " + WAVE_TPL);
        Guid cg = am.AddChunk(sps, null, null, tc.Bundles.ToArray());
        ChunkAssetEntry ce = am.GetChunkEntry(cg);
        ce.AddedSuperBundles.AddRange(tc.SuperBundles); ce.H32 = tc.H32; ce.FirstMip = tc.FirstMip;
        Array.Copy(cg.ToByteArray(), 0, bank.Data, (int)gv[0] - 1, 16);
        bank.F(SbleBank.DS_CHK, CHK_SIZE).Vals[0] = (ulong)sps.Length;
        bank.F(SbleBank.DS_SEG, SEG_LEN).Vals[0] = BitConverter.ToUInt32(BitConverter.GetBytes(samples / (float)rate), 0);

        EbxAsset be = am.GetEbx(srcE, true);   // true = свежая копия ОРИГИНАЛА (грабли Unbound 12.11: иначе все копии — один объект)
        be.SetFileGuid(Guid.NewGuid());
        dynamic root = be.RootObject;
        string assetName = "Audio/Music/Roboto/Playlist/" + trackName, entryName = assetName.ToLowerInvariant();
        root.Name = assetName;
        Guid rg = Utils.GenerateDeterministicGuid(be.Objects, (Type)root.GetType(), be.FileGuid);
        root.SetInstanceGuid(new AssetClassGuid(rg, -1));
        bank.SetKey(SbleBank.KeyOf(rg));
        byte[] nb = bank.Write();
        if (!SbleBank.Read(nb).Write().SequenceEqual(nb)) throw new Exception(trackName + ": wave does not read back byte for byte");
        var chunks = (IList)((object)root).GetType().GetProperty("Chunks").GetValue(root);
        Type ct = chunks[0].GetType(); chunks.Clear();
        object c0 = Activator.CreateInstance(ct);
        ct.GetProperty("ChunkId").SetValue(c0, cg);
        var psz = ct.GetProperty("ChunkSize"); psz.SetValue(c0, Convert.ChangeType(sps.Length, psz.PropertyType));
        chunks.Add(c0);
        EbxAssetEntry newE = am.AddEbx(entryName, be, srcE.Bundles.ToArray());
        ResAssetEntry newR = am.AddRes(entryName, (ResourceType)srcR.ResType, rg.ToByteArray(), nb, srcR.Bundles.ToArray());   // ResMeta = GUID корня
        newE.LinkAsset(newR);
        foreach (var lst in new[] { newE.AddedBundles, newR.AddedBundles }) { var u = lst.Distinct().ToList(); lst.Clear(); lst.AddRange(u); }
        am.ModifyEbx(entryName, be);
        return Tuple.Create(be.FileGuid, rg);
    }

    // сегмент своей песни (город / гонка): копия сегмента-образца этого списка + своя MusicMetadata
    static object NewSegment(EbxAsset a, object tpl, string name, Tuple<Guid, Guid> wave, string title, string artist)
    {
        object seg = Clone(a, tpl);
        SetStr(seg, "Name", name); SetStr(seg, "__Id", name);
        Set(seg, "Wave", new PointerRef(new EbxImportReference { FileGuid = wave.Item1, ClassGuid = wave.Item2 }));
        Set(seg, "Metadata", new PointerRef(NewMeta(a, ((PointerRef)Get(tpl, "Metadata")).Internal, title, artist)));   // Internal = null, если у образца нет
        return seg;
    }

    // фраза гаража: копия P_Tycho_Easy (Sequential, 1 сегмент, FollowOnPlayable -> петля гаража) со своим сегментом
    static object NewPhrase(EbxAsset a, object tplPhr, string trackName, Tuple<Guid, Guid> wave, string title, string artist)
    {
        object tplSeg = FirstSegment(tplPhr);
        object seg = NewSegment(a, tplSeg, "S_GA_" + trackName, wave, title, artist);
        object phr = Clone(a, tplPhr);
        SetStr(phr, "Name", "P_" + trackName); SetStr(phr, "__Id", "P_" + trackName);
        var pp = (IList)Get(phr, "Playables"); pp.Clear(); pp.Add(new PointerRef(seg));
        Set(phr, "Metadata", new PointerRef(NewMeta(a, ((PointerRef)Get(tplPhr, "Metadata")).Internal, title, artist)));
        return phr;
    }

    static object NewMeta(EbxAsset a, object tpl, string title, string artist)
    {
        if (tpl == null) tpl = a.Objects.FirstOrDefault(x => x.GetType().Name == "MusicMetadata");   // у образца нет названия — любое из этого ассета
        if (tpl == null) throw new UserError("gameChanged", "no MusicMetadata");
        object m = Clone(a, tpl);
        SetStr(m, "Title", title); SetStr(m, "Artist", artist); SetStr(m, "Album", "");
        return m;
    }

    // копия объекта EBX в том же ассете: поля как у образца (списки — свои), новый GUID экземпляра
    static object Clone(EbxAsset a, object src)
    {
        Type t = src.GetType();
        object o = Activator.CreateInstance(t);
        foreach (var p in t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0 || p.Name == "__InstanceGuid") continue;
            object v = p.GetValue(src);
            var l = v as IList;
            if (l != null) { var nl = (IList)Activator.CreateInstance(p.PropertyType); foreach (object x in l) nl.Add(x); v = nl; }
            p.SetValue(o, v);
        }
        ((dynamic)o).SetInstanceGuid(new AssetClassGuid(Utils.GenerateDeterministicGuid(a.Objects, t, a.FileGuid), -1));
        a.AddObject(o);
        return o;
    }

    static object Phrase(EbxAsset a, string name) { return ByName(a, "MusicPhraseData", name); }
    static IEnumerable<object> Items(object phrase) { return ((IList)Get(phrase, "Playables")).Cast<PointerRef>().Select(p => p.Internal).Where(x => x != null); }
    static object FirstSegment(object phrase) { return Items(phrase).FirstOrDefault(x => x.GetType().Name == "MusicSegmentData"); }
    static object ByName(EbxAsset a, string type, string name)
    {
        object o = a.Objects.FirstOrDefault(x => x.GetType().Name == type && Str(x, "Name") == name);
        if (o == null) throw new UserError("gameChanged", type + " " + name);
        return o;
    }

    static string Clean(string s) { s = (s ?? "").Replace('\r', ' ').Replace('\n', ' ').Trim(); return s.Length > 80 ? s.Substring(0, 80) : s; }

    // Текст для игры — только ASCII (как в Unbound: кириллицу шрифт игры не показывает). Кириллица → латиница (укр. транслит),
    // буквы с диакритикой → без нее (ë → e, ł → l), остальное (эмодзи, †) — выкинуть. В программе и в song.json имена — как есть.
    static readonly Dictionary<char, string> Tr = new Dictionary<char, string> {
        {'а',"a"},{'б',"b"},{'в',"v"},{'г',"h"},{'ґ',"g"},{'д',"d"},{'е',"e"},{'є',"ye"},{'ж',"zh"},{'з',"z"},{'и',"y"},{'і',"i"},
        {'ї',"yi"},{'й',"y"},{'к',"k"},{'л',"l"},{'м',"m"},{'н',"n"},{'о',"o"},{'п',"p"},{'р',"r"},{'с',"s"},{'т',"t"},{'у',"u"},
        {'ф',"f"},{'х',"kh"},{'ц',"ts"},{'ч',"ch"},{'ш',"sh"},{'щ',"shch"},{'ь',""},{'ю',"yu"},{'я',"ya"},{'ы',"y"},{'э',"e"},
        {'ё',"yo"},{'ъ',""},{'ł',"l"},{'ß',"ss"},{'æ',"ae"},{'ø',"o"},{'đ',"d"},{'œ',"oe"},{'þ',"th"} };
    static string Game(string s)
    {
        s = s ?? "";
        var a = new StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i]; string t;
            if (!Tr.TryGetValue(char.ToLowerInvariant(c), out t)) { a.Append(c); continue; }
            if (char.IsUpper(c) && t.Length > 0)    // «Щ» в «ЩЕ» → SHCH, в «Ще» → Shch
                t = (i + 1 < s.Length && char.IsUpper(s[i + 1])) || (i > 0 && char.IsUpper(s[i - 1])) ? t.ToUpperInvariant() : char.ToUpperInvariant(t[0]) + t.Substring(1);
            a.Append(t);
        }
        var b = new StringBuilder();
        foreach (char c in a.ToString().Normalize(NormalizationForm.FormD))
        {
            if (c >= ' ' && c <= '~') b.Append(c);
            else if (char.IsWhiteSpace(c)) b.Append(' ');
        }
        return System.Text.RegularExpressions.Regex.Replace(b.ToString(), @"\s+", " ").Trim();
    }

    // внутреннее имя своей песни в игре: HS<номер>_<Название латиницей> (ассет звука, сегменты, фраза)
    static string TrackName(Song s, int num)
    {
        var sb = new StringBuilder();
        bool up = true;
        foreach (char ch in Game(s.title))
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9')) { sb.Append(up ? char.ToUpperInvariant(ch) : ch); up = false; }
            else up = true;
            if (sb.Length >= 24) break;
        }
        return "HS" + num + (sb.Length > 0 ? "_" + sb : "");
    }

    static uint BE32(byte[] b, int o) { return (uint)(b[o] << 24 | b[o + 1] << 16 | b[o + 2] << 8 | b[o + 3]); }

    static byte[] ModIcon()
    {
        string p = Path.Combine(App.WebDir, "img", "modicon.png");
        if (File.Exists(p)) return File.ReadAllBytes(p);
        using (var bmp = new Bitmap(64, 64))
        using (var g = Graphics.FromImage(bmp))
        using (var ms = new MemoryStream())
        {
            g.Clear(Color.FromArgb(255, 61, 127));
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }

    static object Get(object o, string p) { return o.GetType().GetProperty(p).GetValue(o); }
    static void Set(object o, string p, object v) { o.GetType().GetProperty(p).SetValue(o, v); }
    static string Str(object o, string prop) { var pi = o.GetType().GetProperty(prop); object v = pi == null ? null : pi.GetValue(o); return v == null ? "" : v.ToString(); }
    static void SetStr(object o, string prop, string val)
    {
        var pi = o.GetType().GetProperty(prop);
        pi.SetValue(o, pi.PropertyType == typeof(string) ? (object)val : Activator.CreateInstance(pi.PropertyType, new object[] { val }));
    }
}

class Lg : ILogger
{
    public void Log(string t, params object[] v) { }
    public void LogWarning(string t, params object[] v) { App.Log("frosty WARN " + t); }
    public void LogError(string t, params object[] v) { App.Log("frosty ERR " + t); }
}
