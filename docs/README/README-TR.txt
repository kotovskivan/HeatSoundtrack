HEAT SOUNDTRACK  v1.0
Need for Speed Heat oyununda kendi şarkıların — oyunun parçalarının yanında, yeni parçalar olarak.
Kitivski + Claude tarafından yapılmış ücretsiz bir hayran aracı. Electronic Arts veya Ghost Games ile bağlantılı DEĞİLDİR.

İngilizcesi — programın yanındaki README.txt.
Çeviriler yapay zekâ ile yapıldı (Claude).

=====================================================================
NE YAPAR
  - Oyunun parçalarını aç/kapat: garaj, gündüz şehir, gece şehir, gündüz yarışları, gece yarışları.
  - Kendi şarkılarını (mp3, flac, wav, ogg, m4a) YENİ parçalar olarak ekle ve her birinin nerede çalacağını işaretle.
    Oyunun parçaları olduğu gibi kalır.
  - YouTube / YT Music'ten şarkılar: bir şarkının, çalma listesinin ya da albümün linki, aynı anda birçok link ya da bir .txt listesi.
    Link bir müzik videosuysa program aynı şarkıyı YT Music'ten alır — videodaki girişler ve skeçler olmadan.
  - Yarış başlangıcı: geri sayım sırasında oyunun giriş parçası çalar, senin şarkıların ondan sonra gelir. Yarışa kendi şarkınla başlamak — sonraki sürümde.
  - Profiller: farklı parça setleri, tek tıkla geçiş.
  - Frosty Mod Manager için TEK bir mod oluşturur. Oyun dosyaları değişmez.
  - Pencere 13 dil biliyor.

GEREKENLER
  - Windows 10 ya da 11 (Microsoft Edge yerleşik gelir — pencere onu kullanır).
  - Need for Speed Heat (Steam ya da EA app).
  - Frosty Mod Manager, HarGabt'ın NFS Heat sürümü:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Bu aracı kullanmadan önce onu bir kez başlat ve oyunu seç.
    İlk açılışta oyunun şifreleme anahtarını ister — NFS Heat Frosty Key (Nexus Mods).
    https://www.nexusmods.com/needforspeedheat/mods/74   (NFSHEAT.key -> Frosty Mod Manager\)
    Launch'a ilk bastığında (ve ModData klasörü silindikten sonra) Frosty, oyun verilerine link oluşturmak için
    yönetici izni ister ("New installation detected") — bu normal, onayla.

NASIL KULLANILIR
  1. Zip arşivinin TAMAMINI normal bir klasöre çıkar (Masaüstü, Belgeler, D:\Mods…),
     Program Files klasörüne değil.
  2. "Heat Soundtrack.exe" dosyasını başlat.
  3. Dili seç, koşulları kabul et. İlk açılışta program oyunu okur — birkaç dakika, sadece bir kez.
  4. + ŞARKI EKLE → bir dosya seç → nerede çalacağını işaretle → Kaydet.
     Ya da ▶ YT MUSIC'TEN → bir link yapıştır (şarkı, çalma listesi, albüm) → İndir.
     Tüm şarkılar programın yanındaki "Music" klasöründe durur, her şarkı için numaralı bir klasör:
     Music\1\, Music\2\… — penceredeki sıra. + ŞARKI EKLE sıradaki klasörü kendisi oluşturur.
     Elle eklemek için: şarkı dosyası + kapak için bir resim (jpg, png) olan yeni bir numaralı klasör.
     Klasörü silersen şarkı programdan kaybolur.
     Resim: klasörde tek resim varsa adı önemli değil; birkaç tane varsa cover.jpg kazanır. Boyutu fark etmez.
  5. MOD OLUŞTUR. Mod, Frosty Mod Manager'ın listesinde görünür.
  6. Frosty Mod Manager'da: "Heat Soundtrack" modunu işaretle, diğer müzik modlarını kapat, Launch'a bas.

SADECE ÇEVRİMDIŞI
  Modlar tek oyunculu oyun içindir. Online'a girmeden önce oyunu modsuz başlat.
  Modlarla Online oynamak EA hesabının yasaklanmasına yol açabilir.

"WINDOWS BİLGİSAYARINIZI KORUDU" / "BİLİNMEYEN YAYIMCI"
  Program ücretli bir sertifikayla imzalanmadığı için Windows SmartScreen uyarı verir.
  "Daha fazla bilgi" → "Yine de çalıştır" seçeneğine tıkla. Kaynak kodu "source" klasöründe.

KLASÖRDE NELER VAR
  Heat Soundtrack.exe      program
  app\web                  pencere (sayfalar, 13 dilde metinler)
  app\frosty               Frosty Toolsuite kütüphaneleri — oyunu okur, modu yazar (GPL v3)
  app\encoder              Frosty Editor'ün Sound Editor eklentisinden EA ses kodlayıcısı
  app\yt-dlp.exe           YouTube / YT Music'ten şarkı indirir (Unlicense)
  source\                  bu programın kaynak kodu (GPL v3)
  README\                  bu metin başka dillerde
  data\                    ilk açılışta oluşur: ayarların, şarkıların, SENİN oyunundan okunan resimler, log
  Music\                   şarkıların, her biri numaralı bir klasörde: şarkı dosyası, kapak, song.json (sanatçı, ad)
  THIRD-PARTY.txt          neler kullanıldı, yazarlar, lisanslar, linkler
  Oyunun ya da EA'nın hiçbir dosyası dağıtılmaz: arka planlar senin oyunundan okunur.

BİR ŞEY ÇALIŞMAZSA
  - Log: data\log.txt (penceredeki "Log" düğmesi).
  - Oyun güncellemesinden sonra: araç oyunu kendisi yeniden okur. Mod oluşturma
    "Oyun değişti" hatasıyla başarısız olursa — aracın daha yeni bir sürümünü ara.
  - Baştan başlamak için: programı kapat ve "data" klasörünü sil.

KALDIRMA
  Modu Frosty Mod Manager'da kapat ve bu klasörü sil.

---------------------------------------------------------------------
Need for Speed ve NFS Heat, Electronic Arts Inc. şirketinin ticari markalarıdır.
Bu araç Electronic Arts ya da Ghost Games tarafından yapılmadı, onaylanmadı ve desteklenmiyor.
Emeği geçenler ve lisanslar: THIRD-PARTY.txt ve programdaki "Yapımcılar" düğmesi.
