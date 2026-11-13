HEAT SOUNDTRACK  v1.0
Need for Speed Heat에 내 노래를 — 게임 트랙 옆에 새 트랙으로.
Kitivski + Claude가 만든 무료 팬 메이드 도구예요. Electronic Arts나 Ghost Games와는 아무 관련이 없어요.

영어판 — 프로그램 옆에 있는 README.txt.
번역은 AI(Claude)가 했어요.

=====================================================================
할 수 있는 것
  - 게임 트랙을 켜고 끌 수 있어요: 차고, 낮 도시, 밤 도시, 낮 레이스, 밤 레이스.
  - 내 노래(mp3, flac, wav, ogg, m4a)를 새 트랙으로 추가하고, 어디서 나올지 체크로 고를 수 있어요.
    게임 트랙은 그대로 남아요.
  - YouTube / YT Music의 노래: 노래, 재생목록, 앨범 링크, 여러 링크를 한꺼번에, 또는 .txt 목록.
    링크가 뮤직비디오면 같은 노래를 YT Music에서 가져와요 — 뮤직비디오의 인트로나 스킷 없이요.
  - 레이스 출발: 카운트다운 동안 게임의 인트로 트랙이 나오고, 내 노래는 그다음에 나와요. 내 노래로 레이스 출발 — 다음 버전에서.
  - 프로필: 트랙 구성을 여러 개 만들고 클릭 한 번으로 바꿔요.
  - Frosty Mod Manager용 모드를 딱 하나 만들어요. 게임 파일은 바뀌지 않아요.
  - 창은 13개 언어를 지원해요.

필요한 것
  - Windows 10 또는 11(Microsoft Edge가 기본으로 들어 있어요 — 창이 Edge를 사용해요).
  - Need for Speed Heat(Steam 또는 EA app).
  - Frosty Mod Manager — HarGabt의 NFS Heat용 버전:
    https://github.com/HarGabt/FrostyToolsuite/releases
    이 도구를 쓰기 전에 한 번 실행해서 게임을 선택해 두세요.
    처음 실행하면 게임 암호화 키를 물어봐요 — NFS Heat Frosty Key (Nexus Mods).
    https://www.nexusmods.com/needforspeedheat/mods/74   (NFSHEAT.key -> Frosty Mod Manager\)
    처음 Launch할 때(그리고 ModData 폴더를 지운 뒤에) Frosty가 게임 데이터로 연결되는 링크를 만들려고
    관리자 권한을 요청해요(“New installation detected”) — 정상이니 허용하세요.

사용법
  1. zip을 꼭 통째로 일반 폴더(바탕 화면, 문서, D:\Mods…)에 압축 해제하세요.
     Program Files 안에는 풀지 마세요.
  2. “Heat Soundtrack.exe”를 실행하세요.
  3. 언어를 고르고 약관에 동의하세요. 처음 실행할 때 게임을 읽어요 — 몇 분 걸리지만 딱 한 번이에요.
  4. + 노래 추가 → 파일 선택 → 재생 위치 체크 → 저장.
     또는 ▶ YT Music에서 → 링크 붙여 넣기(노래, 재생목록, 앨범) → 다운로드.
     모든 노래는 프로그램 옆의 “Music” 폴더에 있어요. 노래마다 번호가 붙은 폴더가 하나씩:
     Music\1\, Music\2\… — 창에 보이는 순서예요. “+ 노래 추가”가 다음 폴더를 알아서 만들어요.
     직접 넣으려면: 번호를 붙인 새 폴더를 만들고 노래 파일 + 커버용 그림(jpg, png)을 넣으세요.
     폴더를 지우면 노래가 프로그램에서 사라져요.
     그림: 폴더에 하나뿐이면 이름은 상관없어요. 여러 개면 cover.jpg가 우선이에요. 크기도 상관없어요.
  5. “모드 만들기”를 누르세요. 모드가 Frosty Mod Manager 목록에 나타나요.
  6. Frosty Mod Manager에서 “Heat Soundtrack”에 체크하고, 다른 음악 모드는 끄고, Launch를 누르세요.

오프라인 전용
  모드는 싱글 플레이 전용이에요. 온라인에 접속하기 전에 모드 없이 게임을 실행하세요.
  모드를 켠 채 온라인을 하면 EA 계정이 정지될 수 있어요.

“Windows의 PC 보호” / “알 수 없는 게시자”
  이 프로그램은 유료 인증서로 서명되지 않아서 Windows SmartScreen이 경고를 띄워요.
  “추가 정보” → “실행”을 클릭하세요. 소스 코드는 “source” 폴더에 있어요.

폴더 안에 있는 것
  Heat Soundtrack.exe      프로그램
  app\web                  창(페이지, 13개 언어 텍스트)
  app\frosty               Frosty Toolsuite 라이브러리 — 게임을 읽고 모드를 만들어요(GPL v3)
  app\encoder              EA 오디오 인코더 — Frosty Editor의 Sound Editor 플러그인에서 가져왔어요
  app\yt-dlp.exe           YouTube / YT Music에서 노래를 내려받아요(Unlicense)
  source\                  이 프로그램의 소스 코드(GPL v3)
  README\                  이 글의 다른 언어판
  data\                    처음 실행할 때 생겨요: 설정, 노래, 바로 내 게임에서 읽어 온 그림, 로그
  Music\                   내 노래, 노래마다 번호 폴더 하나: 노래 파일, 커버, song.json(아티스트, 제목)
  THIRD-PARTY.txt          사용한 것, 만든 사람, 라이선스, 링크
  게임이나 EA의 파일은 하나도 들어 있지 않아요: 배경은 내 게임에서 읽어 와요.

문제가 생기면
  - 로그는 data\log.txt예요(창의 “로그” 버튼).
  - 게임 업데이트 후: 도구가 게임을 알아서 다시 읽어요. 모드를 만들 때
    “게임이 바뀌었어요” 오류가 나면 — 도구의 더 새로운 버전을 찾아보세요.
  - 처음부터 다시 하려면: 프로그램을 닫고 “data” 폴더를 지우세요.

삭제
  Frosty Mod Manager에서 모드를 끄고 이 폴더를 지우세요.

---------------------------------------------------------------------
Need for Speed와 NFS Heat는 Electronic Arts Inc.의 상표예요.
이 도구는 Electronic Arts나 Ghost Games가 만들거나 승인하거나 지원하는 도구가 아니에요.
크레딧과 라이선스: THIRD-PARTY.txt와 프로그램의 “크레딧” 버튼.
