HEAT SOUNDTRACK  v1.0
Le tue canzoni in Need for Speed Heat — come brani nuovi, accanto a quelli del gioco.
Programma gratuito fatto da fan: Kitivski + Claude. NON affiliato con Electronic Arts o Ghost Games.

English — README.txt accanto al programma.
Traduzioni fatte con l'IA (Claude).

=====================================================================
COSA FA
  - Attiva/disattiva i brani del gioco: garage, città di giorno, città di notte, gare di giorno, gare di notte.
  - Aggiunge le tue canzoni (mp3, flac, wav, ogg, m4a) come brani NUOVI e con le caselle scegli dove suona ognuna.
    I brani del gioco restano come sono.
  - Canzoni da YouTube / YT Music: il link di una canzone, playlist o album, più link insieme o un elenco .txt.
    Se il link è un videoclip, il programma prende la stessa canzone da YT Music — senza intro e scenette del video.
  - Partenza della gara: durante il conto alla rovescia suona il brano d'apertura del gioco, le tue canzoni arrivano dopo. Partenza con una tua canzone — nella prossima versione.
  - Profili: diversi set di brani, si cambiano con un clic.
  - Crea UN solo mod per Frosty Mod Manager. I file del gioco non vengono modificati.
  - La finestra parla 14 lingue.

COSA TI SERVE
  - Windows 10 o 11 (Microsoft Edge è integrato — la finestra lo usa).
  - Need for Speed Heat (Steam o EA app).
  - Frosty Mod Manager, la versione di HarGabt per NFS Heat:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Avvialo una volta e scegli il gioco prima di usare questo programma.
    Al primo avvio chiede la chiave di crittografia del gioco — NFS Heat Frosty Key (Nexus Mods).
    https://www.nexusmods.com/needforspeedheat/mods/74   (NFSHEAT.key -> Frosty Mod Manager\)
    Al suo primo Launch (e dopo aver cancellato la cartella ModData) Frosty chiede i diritti di amministratore
    per creare collegamenti ai dati del gioco ("New installation detected") — è normale, accetta.

COME SI USA
  1. Estrai TUTTO lo zip in una cartella normale (Desktop, Documenti, D:\Mods…),
     non in Program Files.
  2. Avvia "Heat Soundtrack.exe".
  3. Scegli la lingua, accetta le condizioni. Il primo avvio legge il gioco — qualche minuto, solo una volta.
  4. + AGGIUNGI CANZONE → scegli un file → spunta dove suona → Salva.
     Oppure ▶ DA YT MUSIC → incolla un link (canzone, playlist, album) → Scarica.
     Tutte le canzoni stanno nella cartella "Music" accanto al programma, una cartella numerata per canzone:
     Music\1\, Music\2\… — l'ordine nella finestra. + AGGIUNGI CANZONE crea da solo la cartella successiva.
     A mano: una nuova cartella numerata con il file della canzone + un'immagine (jpg, png) per la copertina.
     Cancelli una cartella — la canzone sparisce dal programma.
     Immagine: nome qualsiasi se è l'unica nella cartella; più di una — vince cover.jpg. Qualsiasi misura.
  5. CREA MOD. Il mod compare nella lista di Frosty Mod Manager.
  6. In Frosty Mod Manager: spunta "Heat Soundtrack", disattiva gli altri mod musicali, premi Launch.

SOLO OFFLINE
  I mod sono per il gioco in singolo. Prima di andare Online, avvia il gioco senza mod.
  Giocare Online con i mod può costarti il ban dell'account EA.

"PC PROTETTO DA WINDOWS" / "AUTORE SCONOSCIUTO"
  Il programma non è firmato con un certificato a pagamento, quindi Windows SmartScreen mostra un avviso.
  Clicca su "Ulteriori informazioni" → "Esegui comunque". Il codice sorgente è nella cartella "source".

COSA C'È NELLA CARTELLA
  Heat Soundtrack.exe      il programma
  app\web                  la finestra (pagine, testi in 14 lingue)
  app\frosty               librerie di Frosty Toolsuite — leggono il gioco, scrivono il mod (GPL v3)
  app\encoder              encoder audio di EA, dal plugin Sound Editor di Frosty Editor
  app\yt-dlp.exe           scarica le canzoni da YouTube / YT Music (Unlicense)
  source\                  codice sorgente di questo programma (GPL v3)
  README\                  questo testo in altre lingue
  data\                    creata al primo avvio: le tue impostazioni, canzoni, immagini lette dal TUO gioco, log
  Music\                   le tue canzoni, ognuna nella sua cartella numerata: file, copertina, song.json (artista, titolo)
  THIRD-PARTY.txt          cosa usiamo, autori, licenze, link
  Nessun file del gioco o di EA è incluso: gli sfondi vengono letti dal tuo gioco.

SE QUALCOSA NON FUNZIONA
  - Il log è in data\log.txt (pulsante "Log" nella finestra).
  - Dopo un aggiornamento del gioco: il programma rilegge il gioco da solo. Se la creazione del mod fallisce con
    "Il gioco è cambiato" — cerca una versione più recente del programma.
  - Per ricominciare da zero: chiudi il programma e cancella la cartella "data".

RIMUOVERE
  Disattiva il mod in Frosty Mod Manager e cancella la cartella del programma.

---------------------------------------------------------------------
Need for Speed e NFS Heat sono marchi commerciali di Electronic Arts Inc.
Questo programma non è stato creato, approvato né supportato da Electronic Arts o Ghost Games.
Crediti e licenze: THIRD-PARTY.txt e il pulsante "Crediti" nel programma.
