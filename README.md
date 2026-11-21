# Heat Soundtrack

Your own songs in **Need for Speed Heat** (PC) — as new tracks next to the game's ones.
Free fan-made tool by **Kitivski + Claude** (Claude — AI by Anthropic). Not affiliated with Electronic Arts or Ghost Games.

- Add your songs (mp3, flac, wav, ogg, m4a) as **new tracks** and tick where each one plays:
  garage, city by day, city at night, races by day, races at night. The game's tracks stay.
- **Race start with your song:** mark the drop — the four cutscene shots and the 3-2-1 countdown play over your song,
  and the drop hits right after GO!. The program finds the drop itself; a smooth fade-in under the cutscene.
- **Garage exit:** a flag on the wave — the part of your garage song that plays when you drive out (60 s of reserve while the city loads,
  fade-in and fade-out). Left alone — the program takes the loudest part.
- **Song volume** 0–300 % with Auto and overload protection. **Mute narrator** — Ana, in-drive chatter and the menu narrator go quiet; cutscenes stay.
- Songs from **YouTube / YT Music** (a song, a playlist, an album, many links or a .txt list, via yt-dlp) and from **Spotify**
  (a link or a .csv from exportify.app — only the list comes from Spotify, the songs are downloaded from YT Music).
- Turn any of the game's tracks on or off, separately for every place. Profiles — switch sets of tracks in one click.
- Builds **one mod** for Frosty Mod Manager. Game files are not changed. Interface in **14 languages**.
- **Offline only** — mods are for single player.

**Download:** [**HeatSoundtrack.zip** (latest)](https://github.com/kotovskivan/HeatSoundtrack/releases/latest/download/HeatSoundtrack.zip) ·
**Website:** https://kotovskivan.github.io/HeatSoundtrack/ (14 languages) ·
**All Need for Speed games:** [NFS Own Music](https://kotovskivan.github.io/NFSOwnMusic/) ·
**NFS Unbound:** [Unbound Soundtrack](https://github.com/kotovskivan/UnboundSoundtrack)

## How it works

`Heat Soundtrack.exe` is a small C# (.NET Framework 4.8) program: a local HTTP server on 127.0.0.1 plus a Microsoft Edge
`--app` window for the interface (`web/`). Heavy jobs run in the same exe as `--worker`: it reads the game with the
Frosty Toolsuite libraries (profile `NFSHEATSDK`), encodes your songs with the EA audio encoder and writes one `.fbmod`
into Frosty Mod Manager's mods folder. Each song is a new music asset added to the game's playlists — nothing is replaced.

- **Race start:** for a race song with a drop the program cuts the intro into six pieces — four cutscene shots
  (the game holds a shot for about 4 s and doesn't wait for the music, so they are ≤ 2.2 / 3.9 s), the 5.0 s countdown and the race;
  the race piece starts 1.9 s before the drop (GO! is on screen 1.2 s before it) — `Audio.Cuts`, `Worker.NewIntro`.
  The game picks the race intro at random among the enabled game race tracks and your songs with a race start.
- **Garage exit:** like the game's own `vs_<song>_loop` files — a separate `<track>_VS_Loop` asset for the exit phrase (`Worker`, `Audio.Prepare` piece `V`).
- **Mute narrator:** the voice-over phrases of `onboarding` and `narrative` (8 languages) point to a short silence; cutscenes and the police are not touched.

| Folder | What |
|---|---|
| `src/` | the program (C#) — server, game reading, audio cutting and limiter, BC7 / BCn texture codecs, mod writer, YouTube / YT Music (`Yt.cs`), Spotify (`Spotify.cs`) |
| `web/` | the window: HTML/JS/CSS, texts in 14 languages, Mr Dafoe font (SIL OFL) |
| `res/` | icon |
| `docs/` | README (14 languages) and THIRD-PARTY (shipped in the zip) |
| `site/` | the website (`gh-pages` branch is built from it) — see `site/README.txt` |

Internet use: only when you add songs by a link — YouTube / YT Music (`music.youtube.com` to find the song, `yt-dlp` to download it)
and Spotify's public embed page for the song list of a Spotify link (no login, no API key). Nothing about the user is sent anywhere.

## Build

Needs only Windows 10/11 (the C# compiler of .NET Framework 4.8 is built in) and **HarGabt's Frosty Mod Manager**
([releases](https://github.com/HarGabt/FrostyToolsuite/releases), version 1.0.7 v26.10.12 was used).

```bat
build.cmd "C:\path\to\Frosty Mod Manager" "C:\path\to\encoder files"
```

Output: `out\Heat Soundtrack\` — the same layout as the release zip.

- The first argument is the Frosty Mod Manager folder: `FrostySdk.dll`, `FrostyCore.dll` are compile references;
  `FrostyHash.dll`, `FrostyModSupport.dll`, `FrostyControls.dll`, `Profiles\NFSHEATSDK.dll` and `ThirdParty\` are copied next to the exe.
- The second argument (optional) is a folder with the EA audio encoder: `Tool.exe`, `eaxmaenc.dll`, `libatrac9.dll`,
  `libmp3lame.dll`, `msvcr120.dll`. They come from the Sound Editor plugin of Frosty Editor (HarGabt's fork) and are shipped
  unmodified in the release zip (`app\encoder\`) — you can copy them from there.
- `vcruntime140.dll` and `vcruntime140_1.dll` (Microsoft Visual C++ runtime) are copied from `System32` into `app\frosty\`
  (and `app\encoder\`) — so the program works on a clean Windows without the Visual C++ Redistributable.
- Not built by `build.cmd`: `app\yt-dlp.exe` ([yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases)) — copy it from the release zip if you need it.
- The zip has no Visual C++ installer: if the runtime still fails to load, the program opens Microsoft's download
  ([vc_redist.x64.exe](https://aka.ms/vs/17/release/vc_redist.x64.exe)).

You also need the game's encryption key for Frosty — **NFS Heat Frosty Key**
([Nexus Mods](https://www.nexusmods.com/needforspeedheat/mods/74)). It is not in this repository.

## Executables in the release zip

| File | Origin |
|---|---|
| `Heat Soundtrack.exe` | this repository (`build.cmd`), not signed — no paid certificate |
| `app\encoder\Tool.exe` | EA audio encoder from Frosty Editor's Sound Editor plugin (HarGabt's fork), unmodified |
| `app\frosty\ThirdParty\fifaconfig.exe` | part of Frosty Mod Manager's `ThirdParty` folder, unmodified |
| `app\yt-dlp.exe` | [yt-dlp](https://github.com/yt-dlp/yt-dlp) (Unlicense), official release, unmodified |

No game files and no EA audio are in the zip. The window background is a screenshot Kitivski took in the game's photo mode;
the other backgrounds are read from your own game. The song covers are only for the program window — Heat has no track covers,
they don't go into the mod. Credits and licenses — [docs/THIRD-PARTY.txt](docs/THIRD-PARTY.txt).

## Changelog

- **1.0** — first release: your songs as new tracks in five places, race start on GO, garage exit, song volume,
  Mute narrator, YouTube / YT Music and Spotify, profiles, 14 languages.

## License

GNU GPL v3 (the program uses the Frosty Toolsuite libraries) — see [LICENSE](LICENSE).
Need for Speed and NFS Heat are trademarks of Electronic Arts Inc.
