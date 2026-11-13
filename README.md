# Heat Soundtrack

Your own songs in **Need for Speed Heat** (PC) — as new tracks next to the game's ones.
Free fan-made tool by **Kitivski + Claude** (Claude — AI by Anthropic). Not affiliated with Electronic Arts or Ghost Games.

**Website:** https://kotovskivan.github.io/HeatSoundtrack/ (14 languages) ·
**All Need for Speed games:** [NFS Own Music](https://kotovskivan.github.io/NFSOwnMusic/) ·
**NFS Unbound:** [Unbound Soundtrack](https://github.com/kotovskivan/UnboundSoundtrack)

> **Status: coming soon.** Version 1.0 is built; it is being tested in the game before release.

- Add your songs (mp3, flac, wav, ogg, m4a) as **new tracks** and tick where each one plays:
  garage, city by day, city at night, races by day, races at night. The game's tracks stay.
- Turn any of the game's tracks on or off, separately for every place. Profiles — switch sets of tracks in one click.
- Songs from **YouTube / YT Music**: a link to a song, a playlist or an album, many links at once or a .txt list (via yt-dlp).
- Race start: the game's intro track plays under the countdown, your songs come after it.
- Builds **one mod** for Frosty Mod Manager. Game files are not changed. Interface in **14 languages**.
- **Offline only** — mods are for single player.

## How it works

`Heat Soundtrack.exe` is a small C# (.NET Framework 4.8) program: a local HTTP server on 127.0.0.1 plus a Microsoft Edge
`--app` window for the interface (`web/`). Heavy jobs run in the same exe as `--worker`: it reads the game with the
Frosty Toolsuite libraries (profile `NFSHEATSDK`), encodes your songs with the EA audio encoder and writes one `.fbmod`
into Frosty Mod Manager's mods folder. Each song is a new music asset added to the game's playlists — nothing is replaced.

| Folder | What |
|---|---|
| `src/` | the program (C#) — server, game reading, audio cutting, BC7 / BCn texture codecs, mod writer, YouTube / YT Music (`Yt.cs`) |
| `web/` | the window: HTML/JS/CSS, texts in 14 languages, Mr Dafoe font (SIL OFL) |
| `res/` | icon |
| `docs/` | README (14 languages) and THIRD-PARTY (shipped in the zip) |
| `site/` | the website (`gh-pages` branch is built from it) — see `site/README.txt` |

Internet use: the ▶ previews (`api.deezer.com`, the audio streams from Deezer / iTunes) and, only when you add songs by a link,
YouTube / YT Music (`music.youtube.com` to find the song, `yt-dlp` to download it). Nothing about the user is sent anywhere.

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
  unmodified in the release zip (`app\encoder\`).
- `vcruntime140.dll` and `vcruntime140_1.dll` (Microsoft Visual C++ runtime) are copied from `System32` into `app\frosty\`
  (and `app\encoder\`) — so the program works on a clean Windows without the Visual C++ Redistributable.
- Not built by `build.cmd`: `app\yt-dlp.exe` ([yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases)) and
  `app\vcredist\VC_redist.x64.exe` (Microsoft's installer) — copy them from the release zip if you need them.

You also need the game's encryption key for Frosty — **NFS Heat Frosty Key**
([Nexus Mods](https://www.nexusmods.com/needforspeedheat/mods/74)). It is not in this repository.

## Executables in the release zip

| File | Origin |
|---|---|
| `Heat Soundtrack.exe` | this repository (`build.cmd`), not signed — no paid certificate |
| `app\encoder\Tool.exe` | EA audio encoder from Frosty Editor's Sound Editor plugin (HarGabt's fork), unmodified |
| `app\frosty\ThirdParty\fifaconfig.exe` | part of Frosty Mod Manager's `ThirdParty` folder, unmodified |
| `app\yt-dlp.exe` | [yt-dlp](https://github.com/yt-dlp/yt-dlp) (Unlicense), official release, unmodified |
| `app\vcredist\VC_redist.x64.exe` | Microsoft Visual C++ Redistributable x64 installer, signed by Microsoft, unmodified |

No game files and no EA audio are in the zip. The window background is a screenshot Kitivski took in the game's photo mode.
Credits and licenses — [docs/THIRD-PARTY.txt](docs/THIRD-PARTY.txt).

## License

GNU GPL v3 (the program uses the Frosty Toolsuite libraries) — see [LICENSE](LICENSE).
Need for Speed and NFS Heat are trademarks of Electronic Arts Inc.
