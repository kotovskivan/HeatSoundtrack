САЙТ HEAT SOUNDTRACK — https://kotovskivan.github.io/HeatSoundtrack/
Репо: https://github.com/kotovskivan/HeatSoundtrack  — main: код программы + эта папка site\ (исходник сайта),
gh-pages: готовый сайт (папка site\gotovo). Хостинг: GitHub Pages. Собран 13.11.2026, дизайн черновой — по окну программы.
Страницы: / — украинский (главный), /en/ /de/ /es/ /fr/ /it/ /pl/ /pt/ /tr/ /ru/ /zh/ /ja/ /ko/ /fi/ (fi — 13.11 вечер). Переводы — ИИ (Claude).

ВЫПУСК (сейчас на сайте «Скоро», zip — черновик релиза v1.0 на GitHub, публично не виден)
  1. GitHub -> Releases -> черновик «Heat Soundtrack 1.0» -> Edit -> Publish release  (или попросить Claude)
  2. sajt.txt: released = yes, в versions — дата выпуска; проверить version и size
  3. opublikovat.cmd — на сайте появятся «Завантажити» (ведет на последний релиз GitHub) и «Що нового»
  4. Сайт NFS Own Music: igry.txt — Heat soon -> ready, sajt.txt heat_version / heat_download -> opublikovat.cmd там

КАК ОБНОВИТЬ
  Версия, размер zip, ссылки -> sajt.txt.  Тексты -> teksty\<язык>.txt (ключи одинаковые во всех 14).
  Новая версия: sajt.txt version / size / versions + в каждом teksty\*.txt строка «log <версия> = ...»
  (нет в каком-то языке — возьмется из en.txt, сборка напишет).
  prosmotr.cmd    — собрать и посмотреть (http://localhost:8767/)
  opublikovat.cmd — собрать и отправить: исходник сайта -> main (только папка site), сайт -> gh-pages; обновится через 1-2 минуты.
  Разметка в текстах: **жирный**, [текст](ключ-ссылки из sajt.txt или адрес), {version} {size}, \n — перенос.

ЧТО ГДЕ
  shablon.html  разметка и стиль (менять — Claude)
  sobrat.py     сборщик -> gotovo\
  static\       googleeef38fee5f2ab13e.html — подтверждение Google Search Console файлом (13.11 вечер), НЕ удалять — слетит доступ;
                img\bg.jpg — фон первого экрана (скрин Kitivski из фоторежима, тот же, что в окне программы);
                img\okno.jpg — скрин окна (proverka-okna\14-heat-dizajn-en.png), img\og.jpg — превью ссылки 1200x630 (верх того же скрина);
                fonts\MrDafoe-Regular.ttf + OFL.txt — подпись «Soundtrack» (SIL OFL, лицензию не удалять); favicon.png — иконка программы 32 px
  gotovo\       готовый сайт = ветка gh-pages (git worktree), руками не править

16.11 (Ваня: «приведи в порядок и сайт Heat» — как сайт Unbound, но в стиле Heat):
  - после первого экрана блок про общий сайт серии (.hubx) между неоновыми линиями циан→маджента: лого NFS Own Music (оранжевый, как хаб),
    текст, кнопка, карточки Unbound «Готово» / Heat «Скоро» (static\img\hub-*.jpg — копии с хаба). Тексты: hubTag hubLead hubSub hubReady hubSoon hubGo.
    ПРИ ВЫПУСКЕ HEAT: на карточке Heat в shablon.html — class soon -> ready и {{hubSoon}} -> {{hubReady}}.
  - подвал: плитки как в окне Heat — NFS Own Music, «Зробили»: Claude, Kitivski (кот на мадженте); тексты hubRole hMade madeC madeK. Ключ «hub» больше не выводится.
  - телефон: до 900px первый экран и блок хаба по центру; до 560px лого крупнее (курсив сдвинут к центру по замеру пикселей),
    «Скоро» / «Вихідний код» на всю строку, язык — только «UA» (.ln прячется; cur_label в sobrat.py).
  Тексты teksty\*.txt — LF, кроме fi.txt (CRLF); при правке скриптом сохранять как было.

SEO — СДЕЛАНО
  title / description на каждом языке под поиск («своя музыка / custom music / eigene Musik…» + NFS Heat), canonical + hreflang,
  Open Graph / Twitter с картинкой og.jpg, JSON-LD SoftwareApplication (после выпуска — версия, размер, ссылка на скачивание),
  sitemap.xml, подсказка «есть на твоем языке», ссылка на NFS Own Music (страница того же языка).

SEO — СДЕЛАТЬ ТЕБЕ (один раз)
  (13.11 вечер: подтверждение ФАЙЛОМ — он уже в static и в сети; тег google_verify не нужен)
  Google Search Console: ресурс «Префикс URL» https://kotovskivan.github.io/HeatSoundtrack/ -> «HTML-тег» -> content="..." ->
  sajt.txt google_verify -> opublikovat.cmd -> «Подтвердить» -> Файлы Sitemap -> sitemap.xml. Bing — импорт из Google.
