// Heat Soundtrack — точка входа (13.11.2026, на основе Unbound Soundtrack 1.0.1).
// Папка программы:  Heat Soundtrack.exe, README.txt, app\ (web, frosty, encoder), data\ (настройки, песни, лог — создается сама).
// Режимы: без аргументов — окно (сервер 127.0.0.1 + окно браузера, Ui.OpenWindow); "--worker <job.json>" — тяжелая работа с игрой через Frosty
// (отдельный процесс: чистая память на каждую сборку, падение Frosty не роняет окно).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.Script.Serialization;

static class App
{
    public const string Version = "1.0";
    public const string Profile = "NeedForSpeedHeat";
    public static string Root, AppDir, WebDir, FrostyDir, EncDir, DataDir, SongsDir, UiDir, LogPath, MusicDir;
    public static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 64 };

    [STAThread]
    static int Main(string[] args)
    {
        Root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        AppDir = Path.Combine(Root, "app");
        WebDir = Path.Combine(AppDir, "web");
        FrostyDir = Path.Combine(AppDir, "frosty");
        EncDir = Path.Combine(AppDir, "encoder");
        DataDir = Path.Combine(Root, "data");
        SongsDir = Path.Combine(DataDir, "songs");
        UiDir = Path.Combine(DataDir, "ui");
        LogPath = Path.Combine(DataDir, "log.txt");
        MusicDir = Path.Combine(Root, "Music");            // сюда человек кладет свои песни (Music.cs)
        // VCRUNTIME140.dll нужна FrostyHash / libzstd / zlibwapi; на чистой Windows ее нет (P2-01) — своя копия в app\frosty, ищется после папки exe
        try { SetDllDirectory(FrostyDir); } catch { }
        AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        try
        {
            if (args.Length >= 2 && args[0] == "--worker") return RunWorker(args[1]);
            return RunUi(args);
        }
        catch (Exception e)
        {
            Log("FATAL " + e);
            if (args.Length == 0 || args[0] != "--worker")
                System.Windows.Forms.MessageBox.Show("Heat Soundtrack could not start:\n\n" + e.Message + "\n\nDetails: data\\log.txt", "Heat Soundtrack");
            return 1;
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    static extern bool SetDllDirectory(string path);

    // настоящая версия Windows для лога: Environment.OSVersion без манифеста говорит 6.2.9200 (R1-13)
    public static string WinVersion()
    {
        try
        {
            using (var k = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                object ma = k.GetValue("CurrentMajorVersionNumber"), mi = k.GetValue("CurrentMinorVersionNumber"), b = k.GetValue("CurrentBuild"), u = k.GetValue("UBR"), d = k.GetValue("DisplayVersion");
                return ma + "." + mi + "." + b + (u != null ? "." + u : "") + (d != null ? " (" + d + ")" : "");
            }
        }
        catch { return Environment.OSVersion.Version.ToString(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)] static int RunWorker(string job) { return Worker.Run(job); }
    [MethodImpl(MethodImplOptions.NoInlining)] static int RunUi(string[] args) { return Ui.Run(args); }

    // DLL Frosty лежат в app\frosty (и SDK игры — в app\frosty\Profiles), не рядом с exe
    static Assembly Resolve(object s, ResolveEventArgs ev)
    {
        string n = new AssemblyName(ev.Name).Name;
        string[] tries = n == "EbxClasses"
            ? new[] { Path.Combine(FrostyDir, "Profiles", "NFSHEATSDK.dll") }
            : new[] { Path.Combine(FrostyDir, n + ".dll"), Path.Combine(FrostyDir, "ThirdParty", n + ".dll"), Path.Combine(FrostyDir, "Profiles", n + ".dll") };
        // UnsafeLoadFrom, не LoadFrom: zip из интернета → Проводник ставит метку на все DLL → LoadFrom падает 0x80131515 (баг 1.0, фикс 1.0.0a)
        foreach (string p in tries) if (File.Exists(p)) return Assembly.UnsafeLoadFrom(p);
        return null;
    }

    static readonly object logLock = new object();
    public static void Log(string s)
    {
        try
        {
            lock (logLock)
            {
                Directory.CreateDirectory(DataDir);
                var fi = new FileInfo(LogPath);
                if (fi.Exists && fi.Length > 2 * 1024 * 1024) { File.Copy(LogPath, LogPath + ".old", true); File.Delete(LogPath); }
                File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + System.Diagnostics.Process.GetCurrentProcess().Id + "] " + s + "\r\n", Encoding.UTF8);
            }
        }
        catch { }
    }

    public static void WriteJson(string path, object o)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, Json.Serialize(o), new UTF8Encoding(false));
        if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path);
    }
    public static T ReadJson<T>(string path) where T : class
    {
        try { return File.Exists(path) ? Json.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8)) : null; }
        catch (Exception e) { Log("json " + path + ": " + e.Message); return null; }
    }
}

// ---- настройки и песни (data\settings.json) ----
class Song
{
    public string id, title = "", artist = "", file = "";
    public double dur, drop, outro, cDur = 25;
    // где играет своя песня (Heat): гараж, город днем, город ночью, гонка днем, гонка ночью — порядок как в Ctx
    public bool garage = true, cityDay = true, cityNight = true, raceDay = true, raceNight = true, draft = true, cover;
    public static readonly string[] Ctx = { "garage", "cityDay", "cityNight", "raceDay", "raceNight" };
    public bool[] Flags() { return new[] { garage, cityDay, cityNight, raceDay, raceNight }; }
    public void SetFlags(bool[] f) { Func<int, bool> g = i => f != null && f.Length > i && f[i]; garage = g(0); cityDay = g(1); cityNight = g(2); raceDay = g(3); raceNight = g(4); }
    public bool Any() { return garage || cityDay || cityNight || raceDay || raceNight; }
    public double vol = 1;       // 18.11 (из Unbound 17.11, Ваня): громкость всей песни 0…3 (0–300 %); 1 — как есть, звук не трогаем; выше 1 — мягкий лимитер (Limiter.cs)
    public bool lim = true;      // защита от перегруза (лимитер) выше 100 %; false — громче как есть, пики срезаются (трещит — окно предупреждает)
    public double bFade;         // 16.11 (Ваня): нарастание в начале «Старта гонки» — секунды от начала музыки до ручки в окне; 0 = сразу на полную; не дальше GO
    public double cFade = 3, cLevel = 1;   // 16.11 (Ваня): затухание финиша, с (было всегда 3) и громкость в точке желтой ручки — зеркально нарастанию старта
    public double bLevel = 1;    // громкость в точке ручки (0…1): 1 — от ручки уже полная; ниже — от ручки до GO дорастает до полной («нарастание более флекс»)
    public string src, asig, isig;   // песня из папки Music: путь звука относительно Music, подписи звука и картинки (размер:время)
    public long added;
    public string trackName;     // внутреннее имя в игре (ставит сборка: имя ассета звука и сегментов)
    public double[] iCuts;       // старт гонки (v2, 17.11): свои длины 4 кадров заставки + отсчета, с; null — Audio.I_CUTS
    // свое вступление старта гонки: песня в гонке и стоит метка GO (после нее — больше секунды песни)
    public bool Intro() { return (raceDay || raceNight) && drop > 0 && (dur <= 0 || drop < dur - 1); }
    // 19.11 (Ваня: на выезде из гаража песня начиналась заново): кусок выезда — свой файл-петля, как vs_<песня>_loop у игры (16-63 с).
    // Окно (19.11, «как в Unbound»): флажок старта на волне; кусок = от старта запас Audio.VS_LEN с (игра гасит его сама, когда загрузится город),
    // vsFi — нарастание в начале, vsFo — затухание в конце запаса, с. vsStart < 0 — авто: начало самых громких Audio.VS_AUTO с (Audio.VsAuto; Ваня:
    // «не выбрал — программа сама берет 30 с от самого громкого места»), считает сборка. Кусок есть у каждой песни гаража.
    public double vsStart = -1, vsFi = 0, vsFo = Audio.VS_FO;
    // 19.11 (Ваня: «чтоб люди не забывали проставлять»): выезд уже смотрели — песню сохранили в окне, когда она была в гараже. Нет — окно напоминает
    // (точка на карандаше, «Налаштувати виїзд · N» у раздела «Гараж», вопрос перед сборкой). На сборку не влияет: не смотрели — авто-старт.
    public bool vsOk;
    public bool VsPiece() { return garage; }
    // 19.11 окно «Старт гонки» (Ваня: «проделай все»): старт уже смотрели — песню сохранили в окне, когда она была в гонке (drop > 0 — со вступлением под GO,
    // drop 0 — человек выбрал «без вступления»). Нет — окно напоминает, как у выезда. На сборку не влияет: вступление только при drop > 0.
    public bool goOk;
    public bool Race() { return raceDay || raceNight; }
}

// профиль = набор: какие треки игры где играют + какие свои песни включены и где. Сами песни — общая библиотека (songs).
class Preset
{
    public string id, name;
    public Dictionary<string, bool[]> choice = new Dictionary<string, bool[]>();   // трек игры (имя сегмента / фразы) -> [включен]; нет в списке = включен
    public Dictionary<string, bool[]> songs = new Dictionary<string, bool[]>();    // id своей песни -> 5 флагов Song.Ctx; нет в списке = выключена
}

class Settings
{
    public bool agreed, pathsOk;   // pathsOk — человек подтвердил папки игры и Mod Manager
    public string lang;            // язык окна (13 кодов — Ui.Langs); null — первый запуск, окно спросит
    public bool musicTold;         // окно «Пісні з папки» уже показали
    public List<string> skipped = new List<string>();   // песни из Music, удаленные в программе (папка осталась) — не подтягивать снова
    public string gamePath, fmmPath;
    // рабочее состояние = активный профиль (choice + флаги контекстов у песен); при каждом сохранении копируется в профиль
    public Dictionary<string, bool[]> choice = new Dictionary<string, bool[]>();
    public List<Song> songs = new List<Song>();
    public List<Preset> presets = new List<Preset>();
    public string activePreset;
    public string lastBuild;
    public string browser;                         // в чем открывать окно: путь exe браузера, "default" — браузер по умолчанию, null — Edge (Ui.OpenWindow)
    public Dictionary<string, string> view;        // вид главного окна: sort / art / where / view (P2-07: localStorage окна каждый запуск новый — порт другой)
    public bool muteVo;                            // пункт меню «Без диктора» (Ваня 15.11): true — сборка глушит Ану и диктора меню (Worker.MuteVo); общий для всех профилей

    static readonly object l = new object();
    static Settings cur;
    static string P { get { return Path.Combine(App.DataDir, "settings.json"); } }
    public static Settings Get()
    {
        lock (l)
        {
            if (cur == null) cur = App.ReadJson<Settings>(P) ?? new Settings();
            if (cur.choice == null) cur.choice = new Dictionary<string, bool[]>();
            if (cur.songs == null) cur.songs = new List<Song>();
            if (cur.presets == null) cur.presets = new List<Preset>();
            if (cur.skipped == null) cur.skipped = new List<string>();
            if (cur.presets.Count == 0) { var p = new Preset { id = "p1", name = "Main" }; cur.presets.Add(p); cur.activePreset = p.id; cur.Snapshot(p); }
            if (cur.Active == null) cur.activePreset = cur.presets[0].id;
            return cur;
        }
    }
    public Preset Active { get { return presets.Find(p => p.id == activePreset); } }

    // рабочее состояние -> профиль
    public void Snapshot(Preset p)
    {
        p.choice = choice.ToDictionary(k => k.Key, k => (bool[])k.Value.Clone());
        p.songs = songs.Where(s => !s.draft).ToDictionary(s => s.id, s => s.Flags());
    }
    // профиль -> рабочее состояние (песни, которых в профиле нет, — выключены, но остаются в библиотеке)
    public void Load(Preset p)
    {
        choice = p.choice.ToDictionary(k => k.Key, k => (bool[])k.Value.Clone());
        foreach (var s in songs) { bool[] f; s.SetFlags(p.songs.TryGetValue(s.id, out f) ? f : null); }
        activePreset = p.id;
    }

    public static void Save() { lock (l) { var s = Get(); s.Snapshot(s.Active); App.WriteJson(P, s); } }
    public static void Edit(Action<Settings> f) { lock (l) { f(Get()); Save(); } }
}
