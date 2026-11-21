// Heat Soundtrack: своя песня -> A.wav (полная, 48 кГц, стерео, 16 бит) -> кодировщик EA (Tool.exe) -> A.sps (+ .sek) = чанк звука в моде.
// v1 (13.11.2026): в Heat кусок нужен один — полная песня (гараж, город, гонка). Куски B (старт гонки, GO) / C (финиш) из Unbound — на v2:
// старт гонки в Heat — свое «вступление» из 6 сегментов (Pane_01-04, Countdown, Race): куски I1..I6 (17.11, IntroPieces; записка tasks/2026-11-17-heat-start-gonki.md).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

static class Audio
{
    public const int SR = 48000;
    // старт гонки: 17 с катсцены, потом 3 с отсчета 3-2-1, на GO — дроп. До 12.11 было 17 (только катсцена) — дроп бил на «3»
    // (видео Вани 12.11 + отзыв в Steam: «drop on go не на go, а на начало отсчета»)
    // старт гонки: катсцена + старт (отсчет 3-2-1), на GO — дроп. 12.11: 17+3 (отзыв MtheAX: при 17 дроп бил на «3»);
    // 13.11 Ваня в игре: 21+3 — «шляпа», 17+4 — мимо; 20,65 = замер по самой игре: дроп в куске B у треков игры на 20,3-21,1 с,
    // медиана сильных 20,65 (паспорт игры, «Старт гонки»; tools/frosty-reader/intro_drops.py); 20,65 в игре тоже мимо → Ваня: 20,45
    // (окно: те же числа в web/index.html B_CUT / B_COUNT; ключ кэша куска B содержит B_INTRO — старые куски перекодируются сами)
    public const double B_CUT = 17.0, B_COUNT = 3.45, B_INTRO = B_CUT + B_COUNT;

    // Старт гонки Heat (v2, 17.11): вступление = 6 кусков, как у игры (PlayRaceIntro → фраза DI_/NI_Intro_<песня>, Sequential):
    // Pane_01..04 (4 кадра заставки) + Countdown (отсчет) — до GO, Race — от GO до конца песни. Каждый кусок — свой ассет звука.
    // Длины до GO у игры у каждой песни свои: Pane 1,2-4,2 с, Countdown 4,2-6,9 с, до GO 13,7-17,6 с; дроп песни игры — ровно на начале Race
    // (замер 17.11: igry/nfs-heat/razbor/intro/vstupleniya.txt). I_CUTS — наши длины по умолчанию (Song.iCuts — свои длины 4 кадров у песни).
    // Тест 1 в игре (видео Вани 17.11, outputs/nfs-heat/test-start/zamer.txt): кадры 2-4 сменились на концах наших кусков (+12-28 мс) —
    // длины кадров свободные. Отсчет: шторка Countdown+1,58, «3» +2,71, «2» +3,71, «1» +4,70, GO! +5,70 = начало Race + 0,70 с.
    // Для отсчета 5,0 с замер точный → отсчет всегда I_COUNT (не меняется), Race начинается за I_LEAD до метки GO.
    // 19.11 (Ваня: «ставят дроп позже, потому что на GO в игре затухание звука»): замер видео — на GO! игра проваливает музыку до −32 дБ,
    // к уровню гонки она возвращается через ~1,2 с после GO! (outputs/nfs-heat/test-start/zamer.txt). Ваня «да» → дроп на GO! + 1,2 с: I_LEAD 0,7 + 1,2.
    public const double I_COUNT = 5.0, I_LEAD = 1.9;
    public static readonly double[] I_CUTS = { 2.2, 3.9, 3.9, 3.9, I_COUNT };   // = самые длинные кадры (P1_MAX, P_MAX ×3), см. Cuts
    public static readonly string[] I_PARTS = { "S_Pane_01", "S_Pane_02", "S_Pane_03", "S_Pane_04", "S_Countdown", "S_Race" };
    // 19.11 (Ваня: «чтоб песню не пришлось резать… была полноценной» → «давай пробовать»): кадры заставки идут по нашей музыке (тест 1, кадры 2-4
    // сменились на концах наших кусков) → если дроп в песне рано, 4 кадра делим поровну так, чтобы вступление начиналось с 0:00 песни: песня целиком.
    // 19.11 тест в игре (видео 18-46-36, записка tasks/2026-11-19-heat-proverka-v-igre.md): кадр игра держит МАКСИМУМ ~4,0 с от его появления и дальше
    // идет сама — музыку длиннее не ждет (куски 4,75 → дроп на GO! + 5,5 вместо + 1,2). Первый кадр появляется за 1,2-1,4 с ДО музыки, его 4 с считаются
    // от кадра → кадр 1 ≤ P1_MAX 2,2 (запас до задержки 1,8 с), кадры 2-4 ≤ P_MAX 3,9. Песня целиком (с 0:00) — при дропе до I_LEAD + I_COUNT + 13,9 = 20,8 с:
    // делим поровну, кадр 1 не длиннее P1_MAX (остаток — поровну на 2-4). Дроп позже — самые длинные кадры (I_CUTS), песня с (дроп − 20,8) с;
    // раньше 10,9 с — кадры по P_MIN и тишина перед песней (IntroPieces). Свои длины (Song.iCuts) — главнее.
    public const double P_MIN = 1.0, P1_MAX = 2.2, P_MAX = 3.9;
    public static double[] Cuts(Song s)
    {
        if (s.iCuts != null && s.iCuts.Length >= 4 && s.iCuts.Take(4).All(x => x >= 0.5 && x <= 10)) return s.iCuts.Take(4).Concat(new[] { I_COUNT }).ToArray();
        double a = s.drop - I_LEAD - I_COUNT;   // сколько песни до начала Race
        if (a <= 4 * P_MIN) return new[] { P_MIN, P_MIN, P_MIN, P_MIN, I_COUNT };
        if (a >= P1_MAX + 3 * P_MAX) return I_CUTS;
        double p = a / 4;
        if (p <= P1_MAX) return new[] { p, p, p, p, I_COUNT };
        double q = (a - P1_MAX) / 3;
        return new[] { P1_MAX, q, q, q, I_COUNT };
    }
    // длина вступления: от начала музыки (или тишины) до дропа = 4 кадра + отсчет + I_LEAD; музыка начинается с max(0, drop − IntroLen)
    public static double IntroLen(Song s) { return Cuts(s).Sum() + I_LEAD; }
    public static string CutsKey(Song s) { return string.Join(",", Cuts(s).Select(x => x.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))); }

    // Выезд из гаража V (19.11): кусок = от флажка старта запас VS_LEN с (игра гасит его сама, когда загрузится город; кусок — петля, поэтому
    // на случай долгой загрузки в конце затухание vsFo); авто-старт ищем окном VS_AUTO с; затухание по умолчанию VS_FO. Окно — те же числа (web/index.html VS_*)
    public const double VS_LEN = 60, VS_AUTO = 30, VS_FO = 3;
    // громкость куска в секунде u от старта: нарастание с тишины за fi с, затухание в тишину за последние fo с; каждое — не больше половины куска.
    // Та же формула — в окне (web/index.html vsGain)
    public static Func<double, double> VsGain(Song s, double len, out double fi, out double fo)
    {
        fi = Math.Max(0, Math.Min(s.vsFi, len / 2)); fo = Math.Max(0, Math.Min(s.vsFo, len / 2));
        double a = fi, b = fo;
        return u => u < 0 || u >= len ? 0 : Math.Min(a > 0 && u < a ? u / a : 1, b > 0 && u > len - b ? (len - u) / b : 1);
    }
    // авто-кусок (Ваня: «не выбрал — программа сама берет 30 с от самого громкого места»): окно len с с наибольшей энергией (L² + R²),
    // шаг 0,1 с, равные — более раннее; считаем по звуку ДО громкости песни. Та же формула — в окне (web/index.html vsEnergy / vsAutoPick):
    // считаем в целых отсчетах 16 бит (окно — тоже по PCM song.wav, не по float браузера), суммы точные — окно показывает ровно то место, что возьмет сборка.
    // В окне числа — double: точно до 2^53, это окно поиска до ~87 с (VS_AUTO 30)
    public static double VsAuto(byte[] pcm, double len)
    {
        const int B = SR / 10;
        long n = pcm.Length / 4 / B; int w = (int)Math.Round(len * 10);
        if (n <= w) return 0;
        var e = new long[n];
        for (long i = 0; i < n; i++)
        {
            long s = 0; long p = i * B * 4;
            for (int j = 0; j < B; j++, p += 4)
            {
                long l = (short)(pcm[p] | pcm[p + 1] << 8), r = (short)(pcm[p + 2] | pcm[p + 3] << 8);
                s += l * l + r * r;
            }
            e[i] = s;
        }
        long cur = 0; for (int i = 0; i < w; i++) cur += e[i];
        long best = cur; long bi = 0;
        for (long i = 1; i + w <= n; i++) { cur = cur + e[i + w - 1] - e[i - 1]; if (cur > best) { best = cur; bi = i; } }
        return bi / 10.0;
    }

    // song.wav пишем сами (из браузера приходит готовый PCM), поэтому заголовок простой: 44 байта
    public static void WriteWav(string path, Stream pcm, long bytes)
    {
        using (var f = File.Create(path))
        {
            WriteHeader(f, bytes);
            var buf = new byte[1 << 20]; long left = bytes;
            while (left > 0) { int n = pcm.Read(buf, 0, (int)Math.Min(buf.Length, left)); if (n <= 0) break; f.Write(buf, 0, n); left -= n; }
            if (left > 0) throw new IOException("upload cut short");
        }
    }
    static void WriteHeader(Stream f, long bytes)
    {
        var w = new BinaryWriter(f, Encoding.ASCII, true);
        w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write((uint)(36 + bytes)); w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        w.Write(16); w.Write((short)1); w.Write((short)2); w.Write(SR); w.Write(SR * 4); w.Write((short)4); w.Write((short)16);
        w.Write(Encoding.ASCII.GetBytes("data")); w.Write((uint)bytes); w.Flush();
    }

    static byte[] ReadPcm(string wav)
    {
        byte[] all = File.ReadAllBytes(wav);
        if (all.Length < 44 || Encoding.ASCII.GetString(all, 0, 4) != "RIFF") throw new Exception("bad wav " + wav);
        int p = 12;
        while (p + 8 <= all.Length)
        {
            string id = Encoding.ASCII.GetString(all, p, 4); int sz = BitConverter.ToInt32(all, p + 4);
            if (id == "data") { int n = Math.Min(sz, all.Length - p - 8); var d = new byte[n - n % 4]; Buffer.BlockCopy(all, p + 8, d, 0, d.Length); return d; }
            p += 8 + sz + (sz & 1);
        }
        throw new Exception("no data in " + wav);
    }

    // pad — тишина перед куском (с): старт гонки, если GO в песне раньше 20 с (Ваня 12.11: «да делать на GO») — дроп все равно ровно на GO
    // gainIn(u) — громкость в секунде u от начала звука (после pad) на первых gainLen с: нарастание старта гонки (16.11, FadeIn);
    // gainOut(t) — громкость в секунде t от начала звука с outFrom с до конца: затухание финиша (16.11, FadeOut; до этого — всегда 3 с)
    static void Piece(byte[] pcm, double from, double len, string path, double pad = 0, Func<double, double> gainIn = null, double gainLen = 0,
        Func<double, double> gainOut = null, double outFrom = 0)
    {
        long total = pcm.Length / 4;
        long a = Math.Max(0, Math.Min(total, (long)Math.Round(from * SR)));
        long n = len < 0 ? total - a : Math.Min(total - a, (long)Math.Round(len * SR));
        long z = Math.Max(0, (long)Math.Round(pad * SR));
        var d = new byte[(z + n) * 4]; Buffer.BlockCopy(pcm, (int)(a * 4), d, (int)(z * 4), (int)(n * 4));   // массив — нули: тишина уже есть
        if (gainIn != null && gainLen > 0) Gain(d, z, 0, Math.Min(n, (long)Math.Ceiling(gainLen * SR)), gainIn);
        if (gainOut != null) Gain(d, z, Math.Max(0, Math.Min(n, (long)Math.Floor(outFrom * SR))), n, gainOut);
        using (var f = File.Create(path)) { WriteHeader(f, d.Length); f.Write(d, 0, d.Length); }
    }
    // кадры i0..i1 (от начала звука, после тишины z) умножить на g(секунда)
    static void Gain(byte[] d, long z, long i0, long i1, Func<double, double> g)
    {
        for (long i = i0; i < i1; i++)
        {
            double k = Math.Max(0, Math.Min(1, g((double)i / SR)));
            long s = (z + i) * 4;
            for (int c = 0; c < 2; c++) { short v = BitConverter.ToInt16(d, (int)s + 2 * c); short nv = (short)(v * k); d[s + 2 * c] = (byte)nv; d[s + 2 * c + 1] = (byte)(nv >> 8); }
        }
    }

    // затухание финиша (16.11, Ваня) — зеркало нарастания: кусок len с, желтая ручка за f с до конца с громкостью L.
    // L = 1: полная до ручки, от ручки до конца — в тишину (обычное: f = 3). L < 1: от начала куска громкость спускается к L у ручки, дальше — в тишину.
    // Та же формула — в окне (web/index.html fadeOutGain). from — с какой секунды куска менять громкость. Нет затухания — null.
    public static Func<double, double> FadeOut(Song s, double len, out double from)
    {
        double f = Math.Max(0, Math.Min(s.cFade, len)), L = Math.Max(0, Math.Min(1, s.cLevel)), h = len - f;
        from = L < 1 ? 0 : h;
        if (!(f > 0) && L >= 1) return null;
        return t => t >= len ? 0 : t > h ? (f > 0 ? L * (len - t) / f : 0) : (L < 1 && h > 0 ? 1 - (1 - L) * t / h : 1);
    }

    // нарастание старта (16.11, Ваня): от тишины до ручки (f с, громкость L), от ручки до GO (g с от начала музыки) — до полной, дальше полная.
    // Та же формула — в окне (web/index.html fadeGain), чтобы прослушка звучала как в игре. Нет нарастания — null.
    public static Func<double, double> FadeIn(Song s, out double len) { return FadeIn(s, B_INTRO, out len); }
    // intro — длина старта до GO (Unbound: B_INTRO; Heat: сумма Cuts)
    public static Func<double, double> FadeIn(Song s, double intro, out double len)
    {
        double g = Math.Min(s.drop, intro), f = Math.Min(s.bFade, g), L = Math.Max(0, Math.Min(1, s.bLevel));
        len = 0; if (!(f > 0)) return null;
        len = L < 1 ? g : f;
        return u => u <= 0 ? 0 : u < f ? L * u / f : (L < 1 && u < g ? L + (1 - L) * (u - f) / (g - f) : 1);
    }

    // готовит папку enc\ с A/B/C .sps/.sek. Кусок перекодируется, только если поменялось то, от чего он зависит:
    // A — звук; B — звук + GO + длина старта; C — звук + финиш. Двинул GO — кодируется один B (раньше все три, ~4 с на песню).
    // Ключи — enc\key_A.txt / key_B.txt / key_C.txt (пишутся после удачного кодирования куска).
    public static string Prepare(Song s, Action<string> say)
    {
        string dir = Path.Combine(App.SongsDir, s.id), enc = Path.Combine(dir, "enc");
        string wav = Path.Combine(dir, "song.wav");
        if (!File.Exists(wav)) throw new UserError("songMissing", s.title);
        var wi = new FileInfo(wav);
        string w = wi.Length + "|" + wi.LastWriteTimeUtc.Ticks;
        // vol — громкость песни (18.11, из Unbound): только если не 100 % — у остальных песен ключи прежние, перекодировать не нужно; меняет все куски
        string V = s.vol != 1 ? "|vol" + s.vol.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + (s.vol > 1 && !s.lim ? "|nolim" : "") : "";
        var keys = new Dictionary<string, string> { { "A", w + V + "|v1" } };   // Heat v1: только полная песня (B/C — v2, см. шапку)
        // старт гонки (v2): куски I1..I6 — зависят от звука, GO, длин кусков и нарастания
        if (s.Intro())
        {
            string ik = string.Join("|", w, s.drop.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture), CutsKey(s), I_LEAD.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                s.bFade.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), s.bLevel.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), "i1") + V;
            for (int i = 1; i <= 6; i++) keys["I" + i] = ik;
        }
        // 19.11: выезд из гаража V — от старта запас VS_LEN с, нарастание / затухание; vsStart < 0 — авто (самое громкое место, зависит только от звука)
        if (s.VsPiece())
            keys["V"] = string.Join("|", w, s.vsStart >= 0 ? s.vsStart.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) : "auto", VS_LEN.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                s.vsFi.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), s.vsFo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), "v2") + V;
        // кэш до 12.11 (один key.txt на три куска, B — с 17 с): A и C из него годятся, B — перекодировать
        string oldFile = Path.Combine(enc, "key.txt");
        string oldKey = string.Join("|", wi.Length, wi.LastWriteTimeUtc.Ticks, s.drop.ToString("0.00"), s.outro.ToString("0.00"), s.cDur.ToString("0.00"), "v1");
        bool oldOk = File.Exists(oldFile) && File.ReadAllText(oldFile) == oldKey;
        Directory.CreateDirectory(enc);
        var todo = new List<string>();
        foreach (var k in keys)
        {
            string kf = Path.Combine(enc, "key_" + k.Key + ".txt");
            bool have = File.Exists(Path.Combine(enc, k.Key + ".sps")) && File.Exists(Path.Combine(enc, k.Key + ".sek"));
            if (have && File.Exists(kf) && File.ReadAllText(kf) == k.Value) continue;
            if (have && oldOk && k.Key != "B") { File.WriteAllText(kf, k.Value); continue; }
            todo.Add(k.Key);
        }
        if (File.Exists(oldFile)) File.Delete(oldFile);
        if (todo.Count == 0) return enc;

        byte[] pcm = ReadPcm(wav);
        double vs0 = s.vsStart;   // начало куска выезда; авто — по звуку до громкости песни (как окно)
        if (todo.Contains("V") && s.vsStart < 0) { vs0 = VsAuto(pcm, VS_AUTO); App.Log("garage exit start (auto) " + s.title + ": " + vs0.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " s"); }
        if (s.vol != 1) Limiter.Apply(pcm, Math.Max(0, Math.Min(3, s.vol)), SR, s.lim);   // громкость — до нарезки: нарастание кусков старта считается уже от нее
        double dur = pcm.Length / 4.0 / SR;
        double b0 = Math.Max(0, s.drop - B_INTRO);
        double cLen = Math.Min(s.cDur, dur), c0 = Math.Max(0, Math.Min(s.outro, dur - cLen));
        foreach (string k in todo)
        {
            File.Delete(Path.Combine(enc, "key_" + k + ".txt"));
            foreach (string f in Directory.GetFiles(enc, k + ".*")) File.Delete(f);
            if (k == "A") Piece(pcm, 0, -1, Path.Combine(enc, "A.wav"));
            if (k == "B") { double fl; var fi = FadeIn(s, out fl); Piece(pcm, b0, -1, Path.Combine(enc, "B.wav"), Math.Max(0, B_INTRO - s.drop), fi, fl); }   // GO раньше B_INTRO — тишина перед песней
            if (k == "C") { double of; var fo = FadeOut(s, cLen, out of); Piece(pcm, c0, cLen, Path.Combine(enc, "C.wav"), 0, null, 0, fo, of); }
            if (k == "V")
            {
                double v0 = Math.Max(0, Math.Min(vs0, dur - 1)), vl = Math.Min(VS_LEN, dur - v0), fi, fo;
                var vg = VsGain(s, vl, out fi, out fo);
                Piece(pcm, v0, vl, Path.Combine(enc, "V.wav"), 0, fi > 0 ? vg : null, fi, fo > 0 ? vg : null, vl - fo);
            }
        }
        if (todo.Any(k => k.StartsWith("I"))) IntroPieces(pcm, s, enc, todo.Where(k => k.StartsWith("I")).Select(k => int.Parse(k.Substring(1))).ToList());
        pcm = null;
        foreach (string k in todo)
        {
            say(k);
            Encode(Path.Combine(enc, k + ".wav"), Path.Combine(enc, k));
            File.Delete(Path.Combine(enc, k + ".wav"));
            File.WriteAllText(Path.Combine(enc, "key_" + k + ".txt"), keys[k]);
        }
        return enc;
    }

    // старт гонки: звук вступления = [тишина, если GO раньше длины старта] + песня с (GO − I_LEAD − сумма Cuts) до конца, с нарастанием;
    // режется встык на 6 кусков I1..I6 (Cuts — длины первых пяти, I6 = Race — остаток). Границы — в отсчетах, сумма = весь звук.
    static void IntroPieces(byte[] pcm, Song s, string enc, List<int> which)
    {
        double[] cuts = Cuts(s); double pre = cuts.Sum();
        long total = pcm.Length / 4;
        long t0 = (long)Math.Round((s.drop - I_LEAD - pre) * SR);
        long z = Math.Max(0, -t0), a = Math.Max(0, t0);            // z — тишина перед песней, a — откуда в песне
        long len = z + Math.Max(0, total - a);
        var edge = new long[7]; double acc = 0;
        for (int k = 0; k < 5; k++) { acc += cuts[k]; edge[k + 1] = (long)Math.Round(acc * SR); }
        edge[6] = len;
        if (edge[6] - edge[5] < SR) throw new UserError("introShort", s.title);   // после GO меньше секунды песни
        double fl; var fi = FadeIn(s, pre + I_LEAD, out fl);
        foreach (int k in which)
        {
            long i0 = edge[k - 1], n = edge[k] - i0;
            var d = new byte[n * 4];
            long from = Math.Max(i0, z), to = edge[k];                  // часть куска, где уже песня
            if (to > from) Buffer.BlockCopy(pcm, (int)((a + from - z) * 4), d, (int)((from - i0) * 4), (int)((to - from) * 4));
            if (fi != null && fl > 0)
            {
                long g1 = Math.Min(to, z + (long)Math.Ceiling(fl * SR));   // нарастание: секунды от начала музыки (после тишины)
                for (long i = from; i < g1; i++)
                {
                    double g = Math.Max(0, Math.Min(1, fi((double)(i - z) / SR)));
                    long p = (i - i0) * 4;
                    for (int c = 0; c < 2; c++) { short v = BitConverter.ToInt16(d, (int)p + 2 * c); short nv = (short)(v * g); d[p + 2 * c] = (byte)nv; d[p + 2 * c + 1] = (byte)(nv >> 8); }
                }
            }
            using (var f = File.Create(Path.Combine(enc, "I" + k + ".wav"))) { WriteHeader(f, d.Length); f.Write(d, 0, d.Length); }
        }
    }

    public static Process Current;   // кодировщик, который работает сейчас
    static void Encode(string wav, string outBase)
    {
        string tool = Path.Combine(App.EncDir, "Tool.exe");
        if (!File.Exists(tool)) throw new UserError("encoderMissing", tool);
        // кодировщик EA не понимает кириллицу в путях (P2-03: «Мої моди», C:\Users\Іван) — запускаем из папки wav, в аргументах только имена
        // (wav и outBase всегда в одной папке; сам Tool.exe из папки с кириллицей работает — проверено 12.11)
        var psi = new ProcessStartInfo(tool, "-sndplayer -fileformatversion1 -ealayer3pcm_int -seekable \"" + Path.GetFileName(wav) + "\" \"-=" + Path.GetFileName(outBase) + "\"")
        { WorkingDirectory = Path.GetDirectoryName(wav), UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        using (var p = Process.Start(psi))
        {
            Current = p;   // программу закрыли посреди кодирования — Worker.WatchParent гасит и его
            string o = p.StandardOutput.ReadToEnd(), e = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0 || !File.Exists(outBase + ".sps") || !File.Exists(outBase + ".sek"))
            {
                App.Log("encoder exit " + p.ExitCode + "\n" + o + "\n" + e);
                if (p.ExitCode == -1073741515) throw new UserError("vcMissing", "Tool.exe");   // 0xC0000135: нет DLL (VCRUNTIME140 в app\encoder пропала)
                throw new UserError("encodeFailed", Path.GetFileName(wav));
            }
        }
    }
}

// ошибка, которую окно покажет на языке человека (key — ключ перевода, arg — подробность)
class UserError : Exception
{
    public string Key, Arg;
    public UserError(string key, string arg = null) : base(key + (arg != null ? ": " + arg : "")) { Key = key; Arg = arg; }
}
