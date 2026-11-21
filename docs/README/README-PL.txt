HEAT SOUNDTRACK  v1.0
Twoje piosenki w Need for Speed Heat — jako nowe utwory obok utworów gry.
Darmowe narzędzie fanowskie od Kitivski + Claude. NIE jest powiązane z Electronic Arts ani Ghost Games.

English — README.txt obok programu.
Tłumaczenia wykonane przez AI (Claude).

=====================================================================
CO ROBI
  - Włącza/wyłącza utwory gry: garaż, miasto w dzień, miasto w nocy, wyścigi w dzień, wyścigi w nocy.
  - Dodaje twoje piosenki (mp3, flac, wav, ogg, m4a) jako NOWE utwory — zaznaczasz, gdzie każda gra.
    Utwory gry zostają bez zmian.
  - Piosenki z YouTube / YT Music: link do piosenki, playlisty lub albumu, wiele linków naraz albo lista w .txt.
    Jeśli link prowadzi do teledysku, program bierze tę samą piosenkę z YT Music — bez wstawek i scenek z klipu.
  - Spotify: link do piosenki, albumu lub playlisty (do 100 piosenek na link; większa playlista — jako plik .csv z exportify.app).
    Ze Spotify brana jest tylko lista — same piosenki pobierają się z YT Music.
  - Start wyścigu z własną piosenką: zaznacz drop — ujęcia przerywnika i odliczanie 3-2-1 polecą pod twoją piosenkę, drop uderzy tuż po GO!. Drop program może znaleźć sam.
  - Wyjazd z garażu: flaga na fali — skąd gra piosenka, gdy wyjeżdżasz z garażu. Nie ruszasz — program bierze najgłośniejsze miejsce.
  - Głośność każdej piosenki: 0–300 %, „Auto” i ochrona przed przesterowaniem.
  - „Bez narratora”: milczą Ana, rozmowy w trakcie jazdy i narrator menu; przerywniki zostają.
  - Profile: różne zestawy utworów, przełączanie jednym kliknięciem.
  - Buduje JEDEN mod do Frosty Mod Manager. Pliki gry się nie zmieniają.
  - Okno mówi w 14 językach.

POTRZEBUJESZ
  - Windows 10 lub 11 (Microsoft Edge jest wbudowany — okno z niego korzysta).
  - Need for Speed Heat (Steam lub EA app).
  - Frosty Mod Manager w wersji HarGabt dla NFS Heat:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Uruchom go raz i wybierz grę, zanim zaczniesz korzystać z tego programu.
    Przy pierwszym uruchomieniu prosi o klucz szyfrowania gry — NFS Heat Frosty Key (Nexus Mods).
    https://www.nexusmods.com/needforspeedheat/mods/74   (NFSHEAT.key -> Frosty Mod Manager\)
    Gdy pierwszy raz naciśniesz Launch (i po usunięciu folderu ModData), Frosty prosi o uprawnienia administratora,
    żeby utworzyć łącza do danych gry ("New installation detected") — to normalne, zgódź się.

JAK UŻYWAĆ
  1. Rozpakuj CAŁY zip do zwykłego folderu (Pulpit, Dokumenty, D:\Mods…),
     nie do Program Files.
  2. Uruchom "Heat Soundtrack.exe".
  3. Wybierz język, zaakceptuj warunki. Pierwsze uruchomienie czyta grę — kilka minut, tylko raz.
  4. + DODAJ PIOSENKĘ → wybierz plik → zaznacz, gdzie gra → Zapisz.
     Albo ▶ Z YT MUSIC → wklej link (piosenka, playlista, album) → Pobierz.
     Wszystkie piosenki leżą w folderze "Music" obok programu, każda w folderze z numerem:
     Music\1\, Music\2\… — to kolejność w oknie. + DODAJ PIOSENKĘ sam tworzy kolejny folder.
     Ręcznie: nowy folder z numerem, w nim plik piosenki + obrazek (jpg, png) na okładkę.
     Usuniesz folder — piosenka zniknie z programu.
     Obrazek: dowolna nazwa, gdy jest w folderze jeden; kilka — brany jest cover.jpg. Rozmiar dowolny.
     Okładka jest tylko dla programu — nie trafia do moda ani do gry (Heat nie ma okładek utworów).
  5. ZBUDUJ MODA. Mod pojawi się na liście we Frosty Mod Manager.
  6. We Frosty Mod Manager: zaznacz "Heat Soundtrack", wyłącz inne mody muzyczne, naciśnij Launch.

TYLKO OFFLINE
  Mody są do gry dla jednego gracza. Zanim wejdziesz do gry Online, uruchom grę bez modów.
  Granie Online z modami grozi banem konta EA.

"SYSTEM WINDOWS OCHRONIŁ TEN KOMPUTER" / "NIEZNANY WYDAWCA"
  Program nie jest podpisany płatnym certyfikatem, więc Windows SmartScreen przed nim ostrzega.
  Kliknij "Więcej informacji" → "Uruchom mimo to". Kod źródłowy jest w folderze "source".

CO JEST W FOLDERZE
  Heat Soundtrack.exe      program
  app\web                  okno (strony, teksty w 14 językach)
  app\frosty               biblioteki Frosty Toolsuite — czytają grę, zapisują mod (GPL v3)
  app\encoder              koder audio EA z wtyczki Sound Editor do Frosty Editor
  app\yt-dlp.exe           pobiera piosenki z YouTube / YT Music (Unlicense)
  source\                  kod źródłowy tego programu (GPL v3)
  README\                  ten tekst w innych językach
  data\                    tworzony przy pierwszym uruchomieniu: twoje ustawienia, piosenki, obrazki z TWOJEJ gry, log
  Music\                   twoje piosenki, każda w folderze z numerem: plik piosenki, okładka, song.json (wykonawca, tytuł)
  THIRD-PARTY.txt          czego używamy, autorzy, licencje, linki
  W archiwum nie ma plików gry ani EA: tła są czytane z twojej własnej gry.

JEŚLI COŚ NIE DZIAŁA
  - Log to data\log.txt (przycisk "Log" w oknie).
  - Po aktualizacji gry program sam ponownie wczyta grę. Jeśli budowanie kończy się błędem
    "Gra się zmieniła" — poszukaj nowszej wersji programu.
  - Żeby zacząć od nowa: zamknij program i usuń folder "data".

JAK USUNĄĆ
  Wyłącz mod we Frosty Mod Manager i usuń folder z programem.

---------------------------------------------------------------------
Need for Speed i NFS Heat to znaki towarowe Electronic Arts Inc.
To narzędzie nie zostało stworzone, zatwierdzone ani wspierane przez Electronic Arts ani Ghost Games.
Podziękowania i licencje: THIRD-PARTY.txt i przycisk "Podziękowania" w programie.
