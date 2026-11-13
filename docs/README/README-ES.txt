HEAT SOUNDTRACK  v1.0
Tus propias canciones en Need for Speed Heat — como pistas nuevas, junto a las del juego.
Herramienta gratuita hecha por fans: Kitivski + Claude. SIN relación con Electronic Arts ni Ghost Games.

English — README.txt junto al programa.
Traducciones hechas con IA (Claude).

=====================================================================
QUÉ HACE
  - Activa/desactiva las pistas del juego: garaje, ciudad de día, ciudad de noche, carreras de día, carreras de noche.
  - Añade tus propias canciones (mp3, flac, wav, ogg, m4a) como pistas NUEVAS y marca dónde suena cada una.
    Las pistas del juego se quedan como están.
  - Canciones de YouTube / YT Music: el enlace de una canción, playlist o álbum, varios enlaces a la vez o una lista .txt.
    Si el enlace es un videoclip, el programa toma la misma canción de YT Music — sin las intros ni los sketches del vídeo.
  - Salida de la carrera: con la cuenta atrás suena la pista de entrada del juego, tus canciones van después. Salida con tu propia canción — en la próxima versión.
  - Perfiles: distintos conjuntos de pistas, se cambian con un clic.
  - Crea UN solo mod para Frosty Mod Manager. Los archivos del juego no se modifican.
  - La ventana habla 14 idiomas.

NECESITAS
  - Windows 10 u 11 (Microsoft Edge viene integrado — la ventana lo usa).
  - Need for Speed Heat (Steam o EA app).
  - Frosty Mod Manager, la versión de HarGabt para NFS Heat:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Ábrelo una vez y elige el juego antes de usar esta herramienta.
    La primera vez te pide la clave de cifrado del juego — NFS Heat Frosty Key (Nexus Mods).
    https://www.nexusmods.com/needforspeedheat/mods/74   (NFSHEAT.key -> Frosty Mod Manager\)
    En su primer Launch (y después de borrar la carpeta ModData) Frosty pide permisos de administrador
    para crear enlaces a los datos del juego ("New installation detected") — es normal, acéptalo.

CÓMO SE USA
  1. Descomprime el zip ENTERO en una carpeta normal (Escritorio, Documentos, D:\Mods…),
     no en Program Files.
  2. Abre "Heat Soundtrack.exe".
  3. Elige el idioma y acepta las condiciones. El primer inicio lee el juego — unos minutos, solo una vez.
  4. + AÑADIR CANCIÓN → elige un archivo → marca dónde suena → Guardar.
     O ▶ DESDE YT MUSIC → pega un enlace (canción, playlist, álbum) → Descargar.
     Todas las canciones están en la carpeta "Music" junto al programa, cada una en su carpeta numerada:
     Music\1\, Music\2\… — el orden de la ventana. + AÑADIR CANCIÓN crea la siguiente carpeta por sí solo.
     A mano: una carpeta numerada nueva con el archivo de la canción + una imagen (jpg, png) para la portada.
     Borras una carpeta — la canción desaparece del programa.
     Imagen: cualquier nombre si es la única en la carpeta; varias — gana cover.jpg. Cualquier tamaño.
  5. CREAR MOD. El mod aparece en la lista de Frosty Mod Manager.
  6. En Frosty Mod Manager: marca "Heat Soundtrack", desactiva otros mods de música y pulsa Launch.

SOLO SIN CONEXIÓN
  Los mods son para el modo un jugador. Antes de jugar Online, abre el juego sin mods.
  Jugar Online con mods puede costarte un baneo de la cuenta EA.

"WINDOWS PROTEGIÓ SU PC" / "EDITOR DESCONOCIDO"
  El programa no está firmado con un certificado de pago, por eso Windows SmartScreen avisa.
  Haz clic en "Más información" → "Ejecutar de todas formas". El código fuente está en la carpeta "source".

QUÉ HAY EN LA CARPETA
  Heat Soundtrack.exe      el programa
  app\web                  la ventana (páginas, textos en 14 idiomas)
  app\frosty               bibliotecas de Frosty Toolsuite — leen el juego, escriben el mod (GPL v3)
  app\encoder              codificador de audio de EA, del plugin Sound Editor de Frosty Editor
  app\yt-dlp.exe           descarga canciones de YouTube / YT Music (Unlicense)
  source\                  código fuente de este programa (GPL v3)
  README\                  este texto en otros idiomas
  data\                    se crea en el primer inicio: tus ajustes, canciones, imágenes leídas de TU juego, log
  Music\                   tus canciones, cada una en su carpeta numerada: archivo, portada, song.json (artista, título)
  THIRD-PARTY.txt          qué usamos, autores, licencias, enlaces
  No se incluye ningún archivo del juego ni de EA: los fondos se leen de tu propio juego.

SI ALGO NO FUNCIONA
  - El log está en data\log.txt (botón "Log" en la ventana).
  - Tras una actualización del juego: la herramienta vuelve a leer el juego sola. Si al crear el mod sale
    "El juego ha cambiado" — busca una versión más nueva de la herramienta.
  - Para empezar de cero: cierra el programa y borra la carpeta "data".

ELIMINAR
  Desactiva el mod en Frosty Mod Manager y borra la carpeta del programa.

---------------------------------------------------------------------
Need for Speed y NFS Heat son marcas comerciales de Electronic Arts Inc.
Esta herramienta no ha sido creada, aprobada ni respaldada por Electronic Arts ni por Ghost Games.
Créditos y licencias: THIRD-PARTY.txt y el botón "Créditos" del programa.
