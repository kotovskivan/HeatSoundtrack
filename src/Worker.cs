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
//   Старт гонки (v2, 17.11, тест в игре — outputs/nfs-heat/test-start): PlayRaceIntro → свое вступление из 6 кусков у песни с меткой GO
//   (Intros / NewIntro ниже, звук — Audio.IntroPieces); вступления игры — только у включенных треков игры. Окна для GO в Heat пока нет.
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
    const string GA_OUTRO = "VirtualSpace_Outro";   // список переходных фраз гаража (FollowOnPlayable) — туда и наши P_VS_
    const string GA_PHR = "P_Tycho_Easy";   // образец фразы гаража (сегмент + 2 MusicMetadata + FollowOnPlayable -> петля гаража) — проверено в игре 13.11
    // старт гонки (v2, 17.11): список вступлений и образцы фраз (6 сегментов: Pane_01-04, Countdown, Race) — у конкурента эти два проверены в игре
    const string INTRO_LIST = "PlayRaceIntro", INTRO_GEN = "PlayGenericRace";
    static readonly Dictionary<string, string> INTRO_TPL = new Dictionary<string, string> {
        { "raceDay", "DI_Intro_TOG_TurningUpForTheSunshine" }, { "raceNight", "NI_Intro_MNKYBSNSS_OtroLugar" } };
    const uint SEG_LEN = 0x6CFCCE5B, CHK_SIZE = 0xDC19107B, CHK_GUID = 0xF4369173;   // поля SBle (как в Unbound, Sble.cs)
    const uint WAVE_ID = 0xF5F914D9;   // Id звука (в DS_SEL и DS_VAR) — у каждой песни свой (баг 14.11: у всех 20 был Id образца)
    static readonly HashSet<ulong> waveIds = new HashSet<ulong>();
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
            // 19.11: свой кусок выезда из гаража (V.sps) — отдельный ассет звука, как vs_<песня>_loop у игры; нет — петля выезда = вся песня (как раньше)
            string vsFile = Path.Combine(encDirs[i], "V.sps");
            Tuple<Guid, Guid> vsWave = s.VsPiece() && File.Exists(vsFile) ? AddWave(s.trackName + "_VS_Loop", File.ReadAllBytes(vsFile)) : null;
            for (int k = 0; k < CTX.Length; k++)
            {
                if (!fl[k]) continue;
                var c = CTX[k];
                EbxAsset a = assets[c.Asset];
                object item = c.Phrase ? (object)NewPhrase(a, gaPhr, s.trackName, wave, title, artist, vsWave) : NewSegment(a, segTpl[c.Key], "S_" + s.trackName, wave, title, artist);
                ((IList)Get(Phrase(a, c.List), "Playables")).Add(new PointerRef(item));
                a.AddDependency(wave.Item1);
                if (c.Phrase && vsWave != null) a.AddDependency(vsWave.Item1);
                on[c.Key]++;
            }
            own++;
            App.Log("song " + s.trackName + " (" + artist + " - " + title + "): " + sps.Length + " B, contexts " + string.Join(",", CTX.Where((c, k) => fl[k]).Select(c => c.Key)));
        }

        // 4b) старт гонки (v2): вступления игры — только у включенных треков игры; свои — у песен с меткой GO
        Intros(assets, songs, encDirs, choice, origItems);
        object spk; if (job.TryGetValue("speakersDb", out spk) && spk != null) SpeakersDb(Convert.ToDouble(spk));

        // пустой список ломает выбор музыки — в таком контексте возвращаем треки игры как были (только этот список: день и ночь города — один ассет)
        foreach (var c in CTX.Where(c => on[c.Key] == 0))
        {
            IList list = (IList)Get(Phrase(assets[c.Asset], c.List), "Playables");
            list.Clear(); foreach (object x in origItems[c.Key]) list.Add(x);
            on[c.Key] = list.Count;
            App.Log("ctx " + c.Key + ": empty - game tracks restored (" + list.Count + ")");
        }
        foreach (var kv in assets) am.ModifyEbx(kv.Key, kv.Value);
        // Ана и диктор меню — пункт меню окна «Без диктора» (Settings.muteVo → job.muteVo). 15.11 вместе с правкой меню повесило загрузку; одна глушилка в игре не проверена
        object mv; int voLangs = job.TryGetValue("muteVo", out mv) && mv != null && Convert.ToBoolean(mv) ? MuteVo() : 0;

        // 5) мод
        Say("STEP writeMod");
        var ms = new Frosty.Core.Mod.ModSettings();
        ms.Title = job.ContainsKey("title") ? (string)job["title"] : "Heat Soundtrack"; ms.Author = "Kitivski + Claude"; ms.Category = "Audio"; ms.Version = App.Version;
        ms.Description = (job.ContainsKey("preset") ? "Profile: " + Clean((string)job["preset"]) + ". " : "") + (songs.Count > 0 ? "Own songs: " + string.Join(", ", songs.Select(s => Clean(s.artist) + " - " + Clean(s.title))) + ". " : "")
            + "Tracks: garage " + on["garage"] + ", city day " + on["cityDay"] + ", city night " + on["cityNight"] + ", race day " + on["raceDay"] + ", race night " + on["raceNight"]
            + (voLangs > 0 ? ". In-drive chatter (Ana, crew, hints) and menu narrator muted, cutscenes untouched" : "") + ". Made with Heat Soundtrack (fan-made, not affiliated with EA).";
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
        // поля образца — одно значение на поле (st 0): Write пишет Sp2, а не Vals → после правки Vals обязательно Restore (баг 14.11: длина и размер оставались от образца)
        ulong lenBits = BitConverter.ToUInt32(BitConverter.GetBytes(samples / (float)rate), 0);
        ulong oldId = bank.F(SbleBank.DS_VAR, WAVE_ID).Vals[0], id;
        do { id = BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0); } while (id == 0 || id == oldId || !waveIds.Add(id));
        var set = new[] { Tuple.Create(SbleBank.DS_CHK, CHK_SIZE, (ulong)sps.Length), Tuple.Create(SbleBank.DS_SEG, SEG_LEN, lenBits),
                          Tuple.Create(SbleBank.DS_SEL, WAVE_ID, id), Tuple.Create(SbleBank.DS_VAR, WAVE_ID, id) };
        foreach (var t in set) { var f = bank.F(t.Item1, t.Item2); for (int k = 0; k < f.Vals.Count; k++) f.Vals[k] = t.Item3; SbleBank.Restore(f); }

        EbxAsset be = am.GetEbx(srcE, true);   // true = свежая копия ОРИГИНАЛА (грабли Unbound 12.11: иначе все копии — один объект)
        be.SetFileGuid(Guid.NewGuid());
        dynamic root = be.RootObject;
        // правило выбора: у песен игры [29] Once, история 6, общая на гараж / город / гонки (audiosystem_nfs20_selectionbehaviors) —
        // сыгранная песня не выбирается, пока ее не вытеснят 6 других → 3 песни = 3 и тишина. У петель игры (меню, VS, погоня) правила нет (null).
        // Наши — без правила: проверено в игре 15.11 (Ваня: «лупы остались, история все портила»)
        root.DefaultSelectionBehavior = new PointerRef();
        string assetName = "Audio/Music/Roboto/Playlist/" + trackName, entryName = assetName.ToLowerInvariant();
        root.Name = assetName;
        Guid rg = Utils.GenerateDeterministicGuid(be.Objects, (Type)root.GetType(), be.FileGuid);
        root.SetInstanceGuid(new AssetClassGuid(rg, -1));
        bank.SetKey(SbleBank.KeyOf(rg));
        byte[] nb = bank.Write();
        var back = SbleBank.Read(nb);
        if (!back.Write().SequenceEqual(nb)) throw new Exception(trackName + ": wave does not read back byte for byte");
        foreach (var t in set) if (back.F(t.Item1, t.Item2).Vals.Any(v => v != t.Item3)) throw new Exception(trackName + ": wave field " + t.Item2.ToString("X8") + " not written");
        App.Log("wave " + trackName + ": " + (samples / (float)rate).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s, " + sps.Length + " B, id " + id);
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
    static object NewPhrase(EbxAsset a, object tplPhr, string trackName, Tuple<Guid, Guid> wave, string title, string artist, Tuple<Guid, Guid> vsWave = null)
    {
        object tplSeg = FirstSegment(tplPhr);
        object seg = NewSegment(a, tplSeg, "S_GA_" + trackName, wave, title, artist);
        object phr = Clone(a, tplPhr);
        SetStr(phr, "Name", "P_" + trackName); SetStr(phr, "__Id", "P_" + trackName);
        var pp = (IList)Get(phr, "Playables"); pp.Clear(); pp.Add(new PointerRef(seg));
        Set(phr, "Metadata", new PointerRef(NewMeta(a, ((PointerRef)Get(tplPhr, "Metadata")).Internal, title, artist)));
        // своя переходная фраза, как у каждой песни игры: P_VS_<песня> → та же песня по кругу (PlayCount 0). Ее играет VirtualSpace_Outro
        // (SelectionType FollowOnPlayable) — в конце списка гаража (NextPlayable у Play_Garage) и по событию (выезд?). 15.11: вела на петлю Tycho — тишина.
        object tplVs = ((PointerRef)Get(tplPhr, "FollowOnPlayable")).Internal;
        if (tplVs == null) throw new UserError("gameChanged", "no FollowOnPlayable " + GA_PHR);
        // 19.11: вся песня здесь = на выезде она начинается заново (Ваня). Тот же сегмент + RestartIfAlreadyPlaying False (тест v3) — тоже с начала.
        // Поэтому — свой кусок-петля vsWave (Song.vsStart / vsFi / vsFo, Audio.VS_LEN), как у игры; без него — как раньше.
        var vw = vsWave ?? wave;
        object vsSeg = Clone(a, FirstSegment(tplVs));   // Metadata у петли игры нет — так и оставляем
        SetStr(vsSeg, "Name", "S_VS_" + trackName); SetStr(vsSeg, "__Id", "S_VS_" + trackName);
        Set(vsSeg, "Wave", new PointerRef(new EbxImportReference { FileGuid = vw.Item1, ClassGuid = vw.Item2 }));
        object vs = Clone(a, tplVs);
        SetStr(vs, "Name", "P_VS_" + trackName); SetStr(vs, "__Id", "P_VS_" + trackName);
        var vp = (IList)Get(vs, "Playables"); vp.Clear(); vp.Add(new PointerRef(vsSeg));
        Set(phr, "FollowOnPlayable", new PointerRef(vs));
        ((IList)Get(ByName(a, "MusicPhraseData", GA_OUTRO), "Playables")).Add(new PointerRef(vs));
        return phr;
    }

    // ---------- старт гонки (v2, 17.11) ----------
    // PlayRaceIntro (PlayOneAndExit) → фразы вступлений DI_/NI_Intro_<песня> (Sequential: 6 сегментов, у каждого свой ассет звука) → NextPlayable PlayGenericRace.
    // Вступление игры = кусок песни игры: в S_Race та же MusicMetadata (ключи ID_MUSIC_…), что у ее трека в PlayGenericRace — по ней и связь:
    // трек игры выключен → его вступление вон (15.11 Ваня: «на старте гонки вступление игровое… вылазят»). Нет пары — вон, если выключены все треки игры.
    // Свое: 6 ассетов звука (I1..I6 из Audio.Prepare, общие на день и ночь) + фраза-копия образца. Пустой список — вступления игры как были.
    static void Intros(Dictionary<string, EbxAsset> assets, List<Song> songs, List<string> encDirs, Dictionary<string, bool[]> choice, Dictionary<string, List<object>> origItems)
    {
        var waves = new Dictionary<int, Tuple<Guid, Guid>[]>();
        foreach (var ctx in new[] { "raceDay", "raceNight" })
        {
            var c = CTX.First(x => x.Key == ctx);
            EbxAsset a = assets[c.Asset];
            object list = Phrase(a, INTRO_LIST), tpl = Phrase(a, INTRO_TPL[ctx]);
            IList pl = (IList)Get(list, "Playables");
            var orig = pl.Cast<object>().ToList();
            Func<object, string> meta = seg => { object m = seg == null ? null : ((PointerRef)Get(seg, "Metadata")).Internal; return m == null ? null : Str(m, "Artist") + "|" + Str(m, "Title"); };
            var gameOn = (IList)Get(Phrase(a, c.List), "Playables");
            var onNames = new HashSet<string>(gameOn.Cast<PointerRef>().Select(p => Str(p.Internal, "Name")));
            bool anyGameOn = origItems[ctx].Any(p => onNames.Contains(Str(((PointerRef)p).Internal, "Name")));
            for (int i = pl.Count - 1; i >= 0; i--)
            {
                object phr = ((PointerRef)pl[i]).Internal; if (phr == null) continue;
                string key = meta(Items(phr).LastOrDefault());
                object track = key == null ? null : origItems[ctx].Select(p => ((PointerRef)p).Internal).FirstOrDefault(t => t != null && meta(t) == key);
                bool keep = track != null ? onNames.Contains(Str(track, "Name")) : anyGameOn;
                if (!keep) { App.Log("intro " + ctx + ": game " + Str(phr, "Name") + " off" + (track == null ? " (no track pair)" : "")); pl.RemoveAt(i); }
            }
            int own = 0;
            for (int i = 0; i < songs.Count; i++)
            {
                var s = songs[i]; bool[] fl = s.Flags();
                if (!s.Intro() || !fl[Array.IndexOf(Song.Ctx, ctx)]) continue;
                Tuple<Guid, Guid>[] w;
                if (!waves.TryGetValue(i, out w))
                {
                    w = new Tuple<Guid, Guid>[6];
                    for (int k = 0; k < 6; k++) w[k] = AddWave(s.trackName + "_Intro_" + Audio.I_PARTS[k], File.ReadAllBytes(Path.Combine(encDirs[i], "I" + (k + 1) + ".sps")));
                    waves[i] = w;
                }
                string title = Game(Music.Cut(Clean(s.title), Music.MaxTitle)), artist = Game(Music.Cut(Clean(s.artist), Music.MaxArtist));
                if (title == "") title = "Track " + (i + 1);
                pl.Add(new PointerRef(NewIntro(a, tpl, (ctx == "raceDay" ? "DI" : "NI") + "_Intro_" + s.trackName, w, title, artist, "S_" + s.trackName)));
                foreach (var x in w) a.AddDependency(x.Item1);
                own++;
            }
            if (pl.Count == 0) { foreach (object x in orig) pl.Add(x); App.Log("intro " + ctx + ": empty - game intros restored (" + pl.Count + ")"); }
            App.Log("intro " + ctx + ": " + orig.Count + " game -> " + pl.Count + " (own " + own + ")");
        }
    }

    // Громкость музыки на заставке старта (17.11, тест 3): заставка звучит «из колонок события» — группа микшера Event_Speakers
    // (Music < Music_Speakers), в базовом миксе cortexshared/audio/mixers/mixer_music −22 дБ; в гонке музыка идет напрямую (Event_Music −7).
    // Замер по видео теста 1: на заставке музыка на 13-15 дБ тише, чем в гонке (так же и у вступлений игры). db — новое значение группы.
    const string MIX_MUSIC = "cortexshared/audio/mixers/mixer_music";
    static readonly Guid EVENT_SPEAKERS = new Guid("d6e7936c-9a23-4494-bf29-0200854fb5bc");   // MixGroup Event_Speakers в mixersystem_nfs20
    static void SpeakersDb(double db)
    {
        EbxAssetEntry e = am.GetEbxEntry(MIX_MUSIC);
        if (e == null) throw new UserError("gameChanged", "no " + MIX_MUSIC);
        EbxAsset a = am.GetEbx(e);
        int n = 0;
        foreach (object pr in a.Objects.Where(x => x.GetType().Name == "MixerPreset"))
            foreach (object g in (IList)Get(pr, "Groups"))
            {
                var r = (PointerRef)Get(g, "Group");
                if (r.Type != PointerRefType.External || r.External.ClassGuid != EVENT_SPEAKERS) continue;
                var props = (IList)Get(g, "Properties");   // список — ссылка, общая с оригиналом (элементы — struct: копию правим и кладем назад)
                for (int k = 0; k < props.Count; k++)
                {
                    object p = props[k];
                    if (Convert.ToInt32(Get(p, "Property")) != 0) continue;
                    var vp = p.GetType().GetProperty("Value");
                    App.Log("mixer Event_Speakers: " + vp.GetValue(p) + " -> " + db);
                    vp.SetValue(p, Convert.ChangeType(db, vp.PropertyType)); props[k] = p; n++;
                }
            }
        if (n == 0) throw new UserError("gameChanged", "no Event_Speakers in " + MIX_MUSIC);
        am.ModifyEbx(MIX_MUSIC, a);
    }

    // фраза вступления: копия образца (Sequential, NextPlayable → PlayGenericRace этого же ассета) + 6 своих сегментов-копий образца
    static object NewIntro(EbxAsset a, object tplPhr, string name, Tuple<Guid, Guid>[] w, string title, string artist, string equiv)
    {
        var tplSegs = Items(tplPhr).ToList();
        if (tplSegs.Count != 6 || tplSegs.Any(x => x.GetType().Name != "MusicSegmentData")) throw new UserError("gameChanged", "intro layout " + Str(tplPhr, "Name"));
        object phr = Clone(a, tplPhr);
        SetStr(phr, "Name", name); SetStr(phr, "__Id", name);
        var pp = (IList)Get(phr, "Playables"); pp.Clear();
        for (int k = 0; k < 6; k++)
        {
            object seg = Clone(a, tplSegs[k]);
            string sn = name + "_" + Audio.I_PARTS[k];
            SetStr(seg, "Name", sn); SetStr(seg, "__Id", sn);
            Set(seg, "Wave", new PointerRef(new EbxImportReference { FileGuid = w[k].Item1, ClassGuid = w[k].Item2 }));
            // название — только у Race (как у игры: у кадров и отсчета MusicMetadata пустая, общая)
            if (k == 5) Set(seg, "Metadata", new PointerRef(NewMeta(a, ((PointerRef)Get(tplSegs[k], "Metadata")).Internal, title, artist)));
            pp.Add(new PointerRef(seg));
        }
        Set(phr, "Metadata", new PointerRef(NewMeta(a, ((PointerRef)Get(tplPhr, "Metadata")).Internal, title, artist)));
        // у игры (19 из 21) — сегмент этой же песни в PlayGenericRace (вступление = «сыграли трек» для выбора), у 2 — сама фраза
        object eq = a.Objects.FirstOrDefault(x => x.GetType().Name == "MusicSegmentData" && Str(x, "Name") == equiv);
        Set(phr, "EquivalentPlayableForSelection", new PointerRef(eq ?? phr));
        return phr;
    }

    // Ана и диктор меню (15.11, своя правка из файлов игры — мод 810 не берем): в таблице фраз <набор>_<язык> каждая фраза (NewWave) →
    // silence_100ms этого языка и набора. Все 8 языков игры. Нет таблицы / тишины у языка — пропуск; нет ни одной — gameChanged.
    // Наборы (18.11 Ваня: «Ану не замутить в катсценах, а только после выезда с гаража… всю [болтовню], что после гаража»):
    //   onboarding — обучение: выезд 1-й ночи, итоги ночи, звонок CEP, свап мотора, диктор меню / доступности (26 фраз в en);
    //   narrative  — вся болтовня в езде: звонки / callbanter, по пути на миссию, гонки / дрифт / погони, подсказки (1423 фразы в en).
    // НЕ трогаем: катсцены (audio/cinematics/audioassets/<сцена>/…_dx_m|_dx_f — свой звук) и полицейское радио (audio/vo/cops).
    // Таблица и тишина лежат в одном бандле loadvoiceover_<язык> (heat-reader bundles, 18.11). Сообщения фраз (EventStartVO, PursuitRadioOn/Off)
    // стоят в процентах длины — с тишиной срабатывают сразу.
    static readonly string[] VO_LANGS = { "en", "ru", "de", "fr", "es", "it", "pl", "jp" };
    static readonly string[] VO_SETS = { "onboarding", "narrative" };
    static int MuteVo()
    {
        int langs = 0;
        foreach (string l in VO_LANGS)
        {
            int done = 0;
            foreach (string set in VO_SETS)
            {
                string tn = "audio/vo/" + set + "/ctxtfiles/" + l + "/" + set + "_" + l, sn = "audio/vo/" + set + "/content/" + l + "/silence_100ms";
                EbxAssetEntry te = am.GetEbxEntry(tn), se = am.GetEbxEntry(sn);
                if (te == null || se == null) { App.Log("muteVo " + set + " " + l + ": no " + (te == null ? tn : sn)); continue; }
                EbxAsset sil = am.GetEbx(se), t = am.GetEbx(te);
                int n = 0;
                foreach (object o in t.Objects)
                {
                    var p = o.GetType().GetProperty("NewWave");
                    if (p == null || p.PropertyType != typeof(PointerRef)) continue;
                    p.SetValue(o, new PointerRef(new EbxImportReference { FileGuid = sil.FileGuid, ClassGuid = sil.RootInstanceGuid }));
                    n++;
                }
                if (n == 0) { App.Log("muteVo " + set + " " + l + ": no phrases"); continue; }
                t.AddDependency(sil.FileGuid);
                am.ModifyEbx(tn, t);
                done++;
                App.Log("muteVo " + set + " " + l + ": " + n + " phrases -> silence_100ms");
            }
            if (done > 0) langs++;
        }
        if (langs == 0) throw new UserError("gameChanged", "no onboarding VO");
        return langs;
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
