HEAT SOUNDTRACK  v1.0
把你自己的歌曲放进 Need for Speed Heat —— 作为新曲目，和游戏曲目放在一起。
由 Kitivski + Claude 制作的免费粉丝工具。与 Electronic Arts 或 Ghost Games 没有任何关系。

英文版 —— 程序旁边的 README.txt。
翻译由 AI（Claude）完成。

=====================================================================
功能
  - 开启/关闭游戏曲目：车库、白天城市、夜晚城市、白天比赛、夜晚比赛。
  - 添加你自己的歌曲（mp3、flac、wav、ogg、m4a）作为全新的曲目，并勾选每首歌在哪里播放。
    游戏原有的曲目保持不变。
  - 来自 YouTube / YT Music 的歌曲：单曲、播放列表或专辑的链接，一次粘贴多个链接，或一个 .txt 列表。
    如果链接是 MV，程序会从 YT Music 获取同一首歌 —— 没有 MV 里的片头和小剧情。
  - 比赛开场：倒计时期间播放游戏的开场曲，你的歌在它之后播放。用你自己的歌开场 —— 下一个版本支持。
  - 配置：不同的曲目组合，一键切换。
  - 只为 Frosty Mod Manager 生成一个模组。不修改游戏文件。
  - 程序窗口支持 14 种语言。

需要准备
  - Windows 10 或 11（系统自带 Microsoft Edge —— 程序窗口要用到它）。
  - Need for Speed Heat（Steam 或 EA app）。
  - Frosty Mod Manager，HarGabt 为 NFS Heat 制作的版本：
    https://github.com/HarGabt/FrostyToolsuite/releases
    使用本工具之前，先运行它一次并选择游戏。
    第一次运行时它会要求输入游戏的加密密钥 —— NFS Heat Frosty Key (Nexus Mods)。
    https://www.nexusmods.com/needforspeedheat/mods/74   (NFSHEAT.key -> Frosty Mod Manager\)
    第一次点击 Launch 时（以及删除 ModData 文件夹之后），Frosty 会请求管理员权限，
    用来创建指向游戏数据的链接（“New installation detected”）—— 这是正常的，同意即可。

使用方法
  1. 把 zip 压缩包的全部内容解压到一个普通文件夹（桌面、文档、D:\Mods…），
     不要解压到 Program Files 里。
  2. 运行“Heat Soundtrack.exe”。
  3. 选择语言，接受条款。首次启动会读取游戏 —— 需要几分钟，只有这一次。
  4. + 添加歌曲 → 选择文件 → 勾选播放场景 → 保存。
     或者 ▶ 从 YT Music 导入 → 粘贴链接（单曲、播放列表、专辑）→ 下载。
     所有歌曲都放在程序旁边的“Music”文件夹里，每首歌一个带编号的文件夹：
     Music\1\、Music\2\… —— 也就是窗口里的顺序。“+ 添加歌曲”会自动创建下一个文件夹。
     手动添加：新建一个带编号的文件夹，放入歌曲文件 + 一张用作封面的图片（jpg、png）。
     删除文件夹，这首歌就会从程序里消失。
     图片：文件夹里只有一张时，文件名随意；有好几张时，优先用 cover.jpg。尺寸不限。
  5. 点击“生成模组”。模组会出现在 Frosty Mod Manager 的列表里。
  6. 在 Frosty Mod Manager 中：勾选“Heat Soundtrack”，关闭其他音乐模组，点击 Launch。

仅限离线
  模组仅用于单人游戏。进入 Online 之前，请先不带模组启动游戏。
  带模组玩 Online 可能导致你的 EA 账号被封。

“Windows 已保护你的电脑” / “未知发布者”
  本程序没有使用付费证书签名，所以 Windows SmartScreen 会发出警告。
  点击“更多信息”→“仍要运行”。源代码在“source”文件夹里。

文件夹里有什么
  Heat Soundtrack.exe      程序本体
  app\web                  程序窗口（页面、14 种语言的文本）
  app\frosty               Frosty Toolsuite 库 —— 读取游戏、写入模组（GPL v3）
  app\encoder              EA 音频编码器，来自 Frosty Editor 的 Sound Editor 插件
  app\yt-dlp.exe           从 YouTube / YT Music 下载歌曲（Unlicense）
  source\                  本程序的源代码（GPL v3）
  README\                  本说明的其他语言版本
  data\                    首次启动时创建：你的设置、歌曲、从你自己的游戏里读取的图片、日志
  Music\                   你的歌曲，每首一个带编号的文件夹：歌曲文件、封面、song.json（歌手、歌名）
  THIRD-PARTY.txt          用到了什么、作者、许可证、链接
  不附带任何游戏或 EA 的文件：背景是从你自己的游戏里读取的。

遇到问题时
  - 日志在 data\log.txt（窗口里的“日志”按钮）。
  - 游戏更新后：工具会自动重新读取游戏。如果生成模组时
    提示“游戏有变化” —— 请找工具的更新版本。
  - 想从头开始：关闭程序，删除“data”文件夹。

卸载
  在 Frosty Mod Manager 中关闭模组，然后删除这个文件夹。

---------------------------------------------------------------------
Need for Speed 和 NFS Heat 是 Electronic Arts Inc. 的商标。
本工具并非由 Electronic Arts 或 Ghost Games 制作、认可或支持。
制作名单与许可证：THIRD-PARTY.txt 以及程序中的“制作名单”按钮。
