// Окно да/нет для всех уведомлений приложения.
// askYesNo({title, html, yes, no}) -> Promise<true|false>.  no: null — одна кнопка.
// Клавиши: ←/→ (Tab) — выбор, Enter — нажать, Esc — «нет».
(function () {

  function rnd(seed) { let s = seed >>> 0; return () => ((s = (s * 1664525 + 1013904223) >>> 0) / 4294967296); }

  // 17.11 стиль трейлеров Heat (dialog.css): мягкие глитч-плиты. Цвета полупрозрачные (Ваня: «глаза режет» от резких)
  const SOFT = ["rgba(244,246,251,.38)", "rgba(244,246,251,.3)", "rgba(33,230,243,.5)", "rgba(59,95,208,.7)", "rgba(106,61,240,.65)"];
  // снизу полосы: рваная кромка из плит (canvas 18 px)
  function paintFx(cv) {
    const r = cv.getBoundingClientRect(), dpr = window.devicePixelRatio || 1;
    const W = Math.max(1, Math.round(r.width)), H = Math.max(1, Math.round(r.height));
    cv.width = W * dpr; cv.height = H * dpr;
    const g = cv.getContext("2d"); g.scale(dpr, dpr);
    const R = rnd(W * 31 + 7);
    for (let x = -20; x < W + 20;) {
      const w = 40 + R() * 200, h = 3 + R() * 11;
      if (R() < .78) {
        const y = H - h - R() * 3;
        if (R() < .3) { g.fillStyle = "rgba(255,0,128,.45)"; g.fillRect(x - 2, y, 2, h); g.fillStyle = "rgba(0,255,255,.4)"; g.fillRect(x + w, y, 2, h); }
        g.fillStyle = SOFT[Math.floor(R() * SOFT.length)]; g.fillRect(x, y, w, h);
      }
      x += w + R() * 26;
    }
  }
  // SVG-фильтр «обвод» для слова-обоев (один на страницу)
  function ensureObvod() {
    if (document.getElementById("dlg-obvod")) return;
    const d = document.createElement("div");
    d.innerHTML = '<svg width="0" height="0" style="position:absolute" aria-hidden="true"><filter id="dlg-obvod" x="-5%" y="-5%" width="110%" height="110%">' +
      '<feMorphology in="SourceAlpha" operator="erode" radius="2" result="e"/><feComposite in="SourceGraphic" in2="e" operator="out"/></filter></svg>';
    document.body.appendChild(d.firstChild);
  }
  const ghost = t => `<div class="dlg-ghost" aria-hidden="true">${String(t || "").replace(/[&<>]/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;" })[c])}</div>`;

  const SWOOSH = "";   // 17.11: мазок под кнопкой убран (стиль Unbound)

  // ===== 18.11 анимации Heat (Ваня по демо outputs/nfs-heat/animacii/demo-animacii.html: «все эти мне все нравится» — оба типа по очереди) =====
  // По самой игре (knowledge/stil-heat.md «Анимации в самой игре»): кадры по 1/30 с; дуотон фиолет #220066 → синий #1f45a6 → циан #4fe3ea.
  // Окна: 1 «Колонки» / 2 «Мозаика»; смена раздела: 1 «Полоса» / 2 «Ступени» — по очереди, свой счет у открытия и закрытия.
  // Куски = копии (cloneNode), обрезанные clip-path и сдвинутые; дуотон — SVG-фильтр; розово-голубой край 2 px — canvas поверх.
  // HX.force = "1" / "2" — один тип (проверки), HX.manual — кадры по HX.next(), HX.slow — замедление.
  const HX = window.HX = { manual: false, next: null, slow: 1, n: {}, force: null };
  const reduced = () => !!(window.matchMedia && matchMedia("(prefers-reduced-motion: reduce)").matches);
  const pickTy = key => { if (HX.force) return HX.force; const n = HX.n[key] = (HX.n[key] || 0) + 1; return n % 2 ? "1" : "2"; };
  const FR = () => 1000 / 30 * HX.slow;
  let svgDone = false;
  function ensureSvg() {
    if (svgDone) return; svgDone = true;
    const s = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    s.setAttribute("width", "0"); s.setAttribute("height", "0"); s.setAttribute("aria-hidden", "true");
    s.style.cssText = "position:absolute;width:0;height:0";
    s.innerHTML = '<filter id="hx-duo" color-interpolation-filters="sRGB">' +   // темное UI тянем к синему, светлое — циан
      '<feColorMatrix type="matrix" values=".3 .59 .11 0 0  .3 .59 .11 0 0  .3 .59 .11 0 0  0 0 0 1 0"/>' +
      '<feComponentTransfer><feFuncR type="table" tableValues="0.11 0.13 0.12 0.2 0.31"/>' +
      '<feFuncG type="table" tableValues="0 0.06 0.27 0.6 0.89"/><feFuncB type="table" tableValues="0.28 0.46 0.65 0.8 0.92"/></feComponentTransfer></filter>' +
      '<filter id="hx-duo2" color-interpolation-filters="sRGB">' +   // светлый — для мозаики (куски читаются на темном окне), держать с прозрачностью ~.55
      '<feColorMatrix type="matrix" values=".3 .59 .11 0 0  .3 .59 .11 0 0  .3 .59 .11 0 0  0 0 0 1 0"/>' +
      '<feComponentTransfer><feFuncR type="table" tableValues="0.16 0.13 0.2 0.45 0.75"/>' +
      '<feFuncG type="table" tableValues="0.08 0.3 0.75 0.95 1"/><feFuncB type="table" tableValues="0.5 0.78 0.92 0.97 1"/></feComponentTransfer></filter>';
    document.body.appendChild(s);
  }
  const duoF = d => d === 2 ? "url(#hx-duo2)" : d ? "url(#hx-duo)" : "";
  const noFocus = g => { g.querySelectorAll("[id]").forEach(e => e.removeAttribute("id")); g.querySelectorAll("button, a, input, select, textarea").forEach(e => e.tabIndex = -1); };
  const copyCanvas = (src, dst) => {   // cloneNode не копирует пиксели canvas (грабли Unbound)
    const a = src.matches("canvas") ? [src] : [...src.querySelectorAll("canvas")], b = dst.matches("canvas") ? [dst] : [...dst.querySelectorAll("canvas")];
    a.forEach((x, i) => { const y = b[i]; if (y && x.width && x.height) { y.width = x.width; y.height = x.height; y.getContext("2d").drawImage(x, 0, 0); } });
  };
  // копия без хвостов длинных списков (окно «+» на сотни песен): у каждого узла — не больше 60 детей
  const cloneLite = el => {
    const c = el.cloneNode(false); let n = 0;
    for (const ch of el.childNodes) { if (++n > 60) break; c.appendChild(ch.nodeType === 1 ? cloneLite(ch) : ch.cloneNode(false)); }
    return c;
  };
  function makeGhost(dlg) {
    const g = document.createElement("div");
    g.className = "dlg-gh"; g.setAttribute("aria-hidden", "true");
    for (const ch of dlg.children) if (!ch.classList.contains("dlg-gh") && !ch.classList.contains("dlg-glc")) { const c = cloneLite(ch); copyCanvas(ch, c); g.appendChild(c); }
    noFocus(g);
    dlg.appendChild(g);
    return g;
  }
  // глитч окна: rec(A) → кадры; A.cut — кусок, A.cols — колонки, A.ring — рамка, A.only — видно только куски, A.line — линия в центре
  function dlgGlitch(back, rec, cb) {
    const dlg = back.querySelector(".dlg");
    if (dlg._gl) dlg._gl.stop();
    if (!dlg.isConnected) { cb && cb(); return; }
    ensureSvg();
    const W = dlg.clientWidth, Hh = dlg.clientHeight, R = rnd((Date.now() ^ (W * 7 + Hh * 13)) >>> 0);
    if (!W || !Hh) { cb && cb(); return; }
    const gh = [], G = i => gh[i] || (gh[i] = makeGhost(dlg));
    const cv = document.createElement("canvas"); cv.className = "dlg-glc";
    const dpr = window.devicePixelRatio || 1; cv.width = W * dpr; cv.height = Hh * dpr; dlg.appendChild(cv);
    const c2 = cv.getContext("2d"); c2.scale(dpr, dpr);
    const line = back.querySelector(".hx-line");
    const show = (g, css) => { g.style.visibility = "visible"; Object.assign(g.style, css); };
    const A = {
      W, H: Hh, R,
      cut(i, x, y, w, h, o = {}) {   // кусок окна в прямоугольнике x,y,w,h; содержимое внутри сдвинуто на sx,sy
        const sx = o.sx || 0, sy = o.sy || 0;
        const t = Math.max(0, y - sy), l = Math.max(0, x - sx), r = Math.max(0, W - x - w + sx), b = Math.max(0, Hh - y - h + sy);
        show(G(i), { clipPath: `inset(${t}px ${r}px ${b}px ${l}px)`, transform: `translate(${sx}px, ${sy}px)`, filter: duoF(o.duo), opacity: o.op != null ? String(o.op) : "" });
        if (o.edge) A.edge(x, y, w, h);
      },
      cols(xs, dys, duos) { for (let i = 0; i < xs.length - 1; i++) A.cut(i, xs[i], 0, xs[i + 1] - xs[i], Hh, { sy: dys[i] * Hh, duo: !!duos[i], op: duos[i] ? .9 : 1, edge: true }); },
      ring(i, t, sc) {   // рамка из светлого дуотона толщиной t, чуть увеличенная — уходит наружу
        show(G(i), { clipPath: `path(evenodd, "M0 0H${W}V${Hh}H0Z M${t} ${t}V${Hh - t}H${W - t}V${t}Z")`, filter: duoF(2), opacity: ".55",
          transform: sc ? `scale(${sc})` : "", transformOrigin: "50% 50%" });
        c2.fillStyle = "rgba(255,0,128,.45)"; c2.fillRect(t - 2, t, 2, Hh - 2 * t); c2.fillRect(t, t - 2, W - 2 * t, 2);
        c2.fillStyle = "rgba(0,255,255,.4)"; c2.fillRect(W - t, t, 2, Hh - 2 * t); c2.fillRect(t, Hh - t, W - 2 * t, 2);
      },
      edge(x, y, w, h) { c2.fillStyle = "rgba(255,0,128,.45)"; c2.fillRect(x - 2, y, 2, h); c2.fillStyle = "rgba(0,255,255,.4)"; c2.fillRect(x + w, y, 2, h); },
      line(on) { if (line) line.style.display = on ? "block" : "none"; },
      only(on) { dlg.classList.toggle("gl-only", on); }
    };
    const frames = rec(A);
    let i = 0, t = 0;
    const reset = () => { gh.forEach(g => g.removeAttribute("style")); c2.clearRect(0, 0, W, Hh); dlg.classList.remove("gl-only"); A.line(false); };
    const stop = () => { clearTimeout(t); reset(); gh.forEach(g => g.remove()); cv.remove(); dlg._gl = null; if (HX.next === step) HX.next = null; };
    const step = () => {
      reset();
      if (i >= frames.length || !dlg.isConnected) { stop(); cb && cb(); return; }
      frames[i++](A);
      if (HX.manual) HX.next = step; else t = setTimeout(step, FR());
    };
    dlg._gl = { stop };
    step();
  }
  const WIN = {
    1: { // заставка игры 0:12 / 0:25: боковые колонки в дуотоне, сдвиг ступенькой; полоса раскрывается из линии (окна игры, CSS .u)
      in: A => {
        const W = A.W, xs = [0, W * (.2 + A.R() * .08), W * (.58 + A.R() * .08), W], xs4 = [0, W * .26, W * .5, W * .77, W];
        return [
          A => A.line(true),
          A => { A.line(true); A.cols(xs, [.22, -.1, .3], [1, 0, 1]); },
          A => A.cols(xs, [.32, -.16, .14], [1, 0, 1]),
          A => A.cols(xs4, [-.08, .18, -.05, .1], [0, 1, 0, 1]),
          A => A.cols(xs, [.08, -.04, .06], [1, 0, 0]),
          A => A.cols(xs, [.03, 0, .02], [0, 0, 1]),
          () => {}];
      },
      out: A => { const W = A.W, xs = [0, W * .3, W * .64, W]; return [A => A.cols(xs, [.05, -.08, .1], [0, 1, 0]), A => A.cols(xs, [.12, -.14, .06], [1, 0, 1]), () => {}]; }
    },
    2: { // заставка 0:31 / VG247 0:51: куски выскакивают (2 из 3 — слева, где текст), потом рамка из кусков уходит наружу
      in: A => {
        const W = A.W, H = A.H, R = A.R;
        const rs = Array.from({ length: 10 }, () => { const w = W * (.08 + R() * .26), h = H * (.18 + R() * .5); return { x: R() < .65 ? R() * Math.max(0, W * .55 - w) : R() * (W - w), y: R() * (H - h), w, h, sx: (R() - .5) * W * .1, sy: (R() - .5) * H * .2 }; });
        const pop = n => A => { A.only(true); for (let k = 0; k < n; k++) { const r = rs[k]; A.cut(k + 1, r.x, r.y, r.w, r.h, { sx: r.sx, sy: r.sy, duo: 2, op: .6, edge: true }); } };
        return [pop(3), pop(6), pop(10), A => A.ring(0, H * .14), A => A.ring(0, H * .08, 1.01), A => A.ring(0, H * .045, 1.025), A => A.ring(0, H * .02, 1.04), () => {}];
      },
      out: A => {
        const W = A.W, H = A.H, R = A.R;
        return [
          A => { for (let k = 0; k < 4; k++) { const w = W * (.1 + R() * .25), h = H * (.2 + R() * .5); A.cut(k, R() * Math.max(0, W * .6 - w), R() * (H - h), w, h, { duo: 2, op: .6, sx: (R() - .5) * W * .08, edge: true }); } },
          A => { A.only(true); for (let k = 0; k < 5; k++) { const w = W * (.06 + R() * .2), h = H * (.15 + R() * .4); A.cut(k, R() * Math.max(0, W * .6 - w), R() * (H - h), w, h, { duo: 2, op: .5, edge: true }); } },
          () => {}];
      }
    }
  };
  // тип решается до вставки окна: класс .u (полоса из линии) должен быть в первом стиле, иначе clip-path не анимируется
  function prepWin(back) {
    const ty = reduced() ? "0" : pickTy("win-in");
    back._ty = ty;
    if (ty !== "0") { back.classList.add("hx"); if (ty === "1") back.classList.add("u"); }
    back.insertAdjacentHTML("beforeend", '<div class="hx-line" aria-hidden="true"></div>');
  }
  function openWin(back) {
    if (HX.slow !== 1) back.style.setProperty("--slow", HX.slow);
    if (back._ty === "0") { back.classList.add("show"); return; }
    back.getBoundingClientRect();
    back.classList.add("show"); back.classList.remove("u");
    dlgGlitch(back, WIN[back._ty].in);
  }
  // закрыть как в игре: выбранная строка сереет → полоса сжимается в линию (+ 2 кадра глитча) → фон светлеет
  function closeWin(back) {
    back.style.pointerEvents = "none";
    const gone = () => { back.classList.remove("show"); setTimeout(() => back.remove(), 220 * HX.slow); };
    if (back._ty === "0" || reduced()) { gone(); return; }
    const ty = pickTy("win-out");
    back.classList.add("gray");
    setTimeout(() => {
      back.classList.add("closing");
      dlgGlitch(back, WIN[ty].out);
      setTimeout(gone, 140 * HX.slow);
    }, 50 * HX.slow);
  }

  // ===== глитч области страницы: сетка после смены раздела =====
  // Видимые дети меряются ОДИН раз (грабли Unbound: на сотнях плиток каждая копия заставляет пересчитать раскладку).
  // Слой .hx-ov — поверх по месту на экране; копии видимых плиток — на своих местах; сам элемент на время перехода прозрачный.
  function snapArea(el) {
    const r0 = el.getBoundingClientRect(), vw = window.innerWidth, vh = window.innerHeight;
    const box = { x: Math.max(0, r0.left), y: Math.max(0, r0.top) };
    box.w = Math.min(vw, r0.right) - box.x; box.h = Math.min(vh, r0.bottom) - box.y;
    const vis = [];
    for (const ch of el.children) {
      const r = ch.getBoundingClientRect();
      if (!r.width || !r.height || r.bottom <= box.y || r.top >= box.y + box.h || r.right <= box.x || r.left >= box.x + box.w) continue;
      vis.push({ ch, r });
    }
    return { el, cls: el.className, box, vis };
  }
  function areaClone(snap) {   // копия видимых детей — className как у элемента (стили «.grid.small .card» работают), раскладку перекрывает .hx-agh
    const g = document.createElement("div");
    g.className = snap.cls + " hx-agh"; g.setAttribute("aria-hidden", "true");
    for (const v of snap.vis) {
      const c = v.ch.cloneNode(true), r = v.r;
      Object.assign(c.style, { position: "absolute", left: (r.left - snap.box.x) + "px", top: (r.top - snap.box.y) + "px", width: r.width + "px", height: r.height + "px",
        margin: "0", boxSizing: "border-box", transform: "none", transition: "none", animation: "none" });
      copyCanvas(v.ch, c);
      g.appendChild(c);
    }
    noFocus(g);
    return g;
  }
  // снимок старого раздела — ДО render() (потом детей уже нет); копия делается сразу
  window.hxSnap = el => { if (reduced() || !el || !el.isConnected) return null; const s = snapArea(el); s.clone = areaClone(s); return s; };
  const AREA = {
    grid: {
      1: A => {   // VG247 0:52,5: колонка-дуотон в треть ширины шагает слева направо в 3 шага; слева уже новый раздел, справа еще старый
        const W = A.W, H = A.H, w = W * .34, xs = [0, .33, .66].map(k => W * k);
        const f = k => A => {
          if (xs[k] > 0) A.cut(A.N(0), 0, 0, xs[k], H);
          const right = xs[k] + w; if (right < W) A.cut(A.O(0), right, 0, W - right, H);
          A.cut(A.N(1), xs[k], 0, w, H, { duo: 1, op: .88, edge: true, tint: true });
        };
        return [f(0), f(0), f(1), f(1), f(2), f(2)];
      },
      2: A => {   // заставка 0:25: 4 колонки со сдвигом вниз / вверх, старый и новый раздел вперемешку, одна колонка в дуотоне
        const W = A.W, H = A.H, R = A.R, xs = [0, .23 + R() * .06, .48 + R() * .06, .73 + R() * .06, 1].map(k => W * k);
        const dys = [.07, -.05, .11, -.03].map(v => v * H);
        const cols = (olds, mul, duoI) => A => {
          for (let i = 0; i < 4; i++) A.cut(olds[i] ? A.O(i) : A.N(i), xs[i], 0, xs[i + 1] - xs[i], H, { sy: dys[i] * mul, duo: i === duoI ? 1 : 0, op: i === duoI ? .9 : 1, edge: mul > .3, tint: i === duoI });
        };
        return [cols([0, 1, 0, 1], 1, 2), cols([0, 1, 0, 1], 1.5, 0), cols([0, 0, 0, 0], .6, 3), cols([0, 0, 0, 0], .15, -1)];
      }
    }
  };
  // переход области: el — новый вид (после render), old — снимок hxSnap до него; key — чей счет очереди
  window.hxArea = function (el, old, key = "grid", cb) {
    if (el._gl) el._gl.stop();
    if (reduced() || !el || !el.isConnected || !old) { cb && cb(); return; }
    ensureSvg();
    const now = snapArea(el), box = now.box, W = Math.round(box.w), H = Math.round(box.h);
    if (W < 20 || H < 20) { cb && cb(); return; }
    const ty = pickTy(key), R = rnd((Date.now() ^ (W * 7 + H * 13)) >>> 0);
    const ov = document.createElement("div"); ov.className = "hx-ov"; ov.setAttribute("aria-hidden", "true");
    Object.assign(ov.style, { left: box.x + "px", top: box.y + "px", width: W + "px", height: H + "px" });
    const cv = document.createElement("canvas"); cv.className = "hx-glc";
    const dpr = window.devicePixelRatio || 1; cv.width = W * dpr; cv.height = H * dpr; ov.appendChild(cv);
    const c2 = cv.getContext("2d"); c2.scale(dpr, dpr);
    document.body.appendChild(ov);
    const nw = [], od = [];
    const mk = g => { ov.insertBefore(g, cv); return g; };
    const A = {
      W, H, R,
      N: i => nw[i] || (nw[i] = mk(areaClone(now))),
      O: i => od[i] || (od[i] = mk(i === 0 && old.clone ? old.clone : old.clone ? old.clone.cloneNode(true) : areaClone(old))),
      cut(g, x, y, w, h, o = {}) {
        const sx = o.sx || 0, sy = o.sy || 0;
        const t = Math.max(0, y - sy), l = Math.max(0, x - sx), r = Math.max(0, W - x - w + sx), b = Math.max(0, H - y - h + sy);
        Object.assign(g.style, { visibility: "visible", clipPath: `inset(${t}px ${r}px ${b}px ${l}px)`, transform: `translate(${sx}px, ${sy}px)`,
          filter: duoF(o.duo), opacity: o.op != null ? String(o.op) : "", background: o.tint ? "rgba(40,16,110,.5)" : "" });
        if (o.edge) { c2.fillStyle = "rgba(255,0,128,.45)"; c2.fillRect(x - 2, y, 2, h); c2.fillStyle = "rgba(0,255,255,.4)"; c2.fillRect(x + w, y, 2, h); }
      }
    };
    const frames = AREA[key][ty](A);
    let i = 0, t = 0;
    const reset = () => { nw.concat(od).forEach(g => g && g.removeAttribute("style")); c2.clearRect(0, 0, W, H); };
    const stop = () => { clearTimeout(t); ov.remove(); el.style.opacity = ""; el._gl = null; if (HX.next === step) HX.next = null; };
    const step = () => {
      reset();
      if (i >= frames.length || !el.isConnected) { stop(); cb && cb(); return; }
      frames[i++](A);
      if (HX.manual) HX.next = step; else t = setTimeout(step, FR());
    };
    el._gl = { stop };
    el.style.opacity = "0";   // сам раздел прозрачный, пока идут куски (зазоры между колонками — фон страницы)
    step();
  };

  // ===== 18.11 редактор песни: 1 «Удар» / 2 «Двоение» (Ваня по демо: «да все теперь как надо») — по очереди, свой счет у открытия и закрытия =====
  // Слой .hx-ov на весь экран (копии-двоение выходят левее панели). Куски — копии панели целиком (cloneNode + пиксели canvas + прокрутка),
  // обрезанные clip-path и сдвинутые; светлый дуотон hx-duo2 держать op ≤ .55. Сама панель, пока идут кадры, прозрачная.
  const SLABS = [[0, .3], [.3, .62], [.62, 1]];   // 3 крупные горизонтальные плиты (для разъезда)
  const PAN = {
    1: {
      in: A => {   // вход в меню игры 1:12,6: «удар» — панель крупнее светлым дуотоном, плиты разъезжаются и встают; потом блоки сверху вниз: кадр дуотоном правее → на место
        const p = A.P, pk = A.pk, B = A.bands;
        const band = (k, o) => { const [a, b] = B[k]; A.pc({ x: p.x + pk, y: p.y + a, w: p.w - pk, h: b - a }, o); };
        const fr = [
          () => { A.back(); A.pc(p, { zoom: 1.08, duo: 2, op: .55 }); A.slab(1, { dx: p.w * .07, op: .85, edge: true }); },
          () => { A.back(); A.pc(p, { zoom: 1.035, duo: 2, op: .4 }); [-.05, .06, -.025].forEach((v, k) => A.slab(k, { dx: p.w * v, op: .85, edge: true })); },
          () => { A.back(); [.02, -.02, 0].forEach((v, k) => A.slab(k, { dx: p.w * v, duo: k === 2 ? 2 : 0, op: k === 2 ? .5 : .6, edge: k === 2 })); }
        ];
        for (let k = 0; k < B.length; k++) fr.push(() => {   // блок k: выше — на месте, сам — дуотоном правее с краем, ниже — тускло
          A.back();
          B.forEach((_, j) => { if (j < k) band(j); else if (j > k) band(j, { op: .3 }); });
          band(k, { dx: p.w * .04, duo: 2, op: .55, edge: true });
        });
        fr.push(() => { A.back(); B.forEach((_, j) => band(j)); });
        return fr;
      },
      out: A => {   // колонки со сдвигом (средняя дуотоном) → плиты вбок → верх и низ вправо → узкая полоса у розовой линии → нет
        const p = A.P, xs = [p.x, p.x + p.w * .36, p.x + p.w * .7, p.x + p.w];
        return [
          () => [.05, -.07, .1].forEach((v, i) => A.pc({ x: xs[i], y: p.y, w: xs[i + 1] - xs[i], h: p.h }, { sy: v * p.h, duo: i === 1 ? 2 : 0, op: i === 1 ? .55 : 1, edge: true })),
          () => [.08, -.05, .14].forEach((v, k) => A.slab(k, { dx: p.w * v, duo: k === 0 ? 2 : 0, op: k === 0 ? .5 : .75, edge: true })),
          () => { A.slab(0, { dx: p.w * .2, duo: 2, op: .45, edge: true }); A.slab(2, { dx: p.w * .3, op: .4 }); },
          () => A.pc({ x: p.x, y: p.y, w: A.pk + 4, h: p.h }, { edge: true }),
          () => {}];
      }
    },
    2: {
      in: A => {   // заставка 0:38: двоение — сначала одни копии левее (светлый дуотон + темный), панель встает на 3-м кадре, копии съезжаются к ней
        const p = A.P, gh = (v, o) => A.pc(p, Object.assign({ dx: -p.w * v, edge: true }, o)), panel = () => A.pc(p);
        const mid = o => A.pc({ x: p.x, y: p.y + p.h * .3, w: p.w, h: p.h * .32 }, o);
        return [
          () => gh(.3, { duo: 2, op: .55 }),
          () => { gh(.46, { duo: 2, op: .55 }); gh(.16, { duo: 1, op: .75 }); },
          () => { gh(.5, { duo: 2, op: .5 }); gh(.26, { duo: 1, op: .7 }); panel(); },
          () => { gh(.38, { duo: 2, op: .5 }); mid({ dx: -p.w * .62, duo: 2, op: .45, edge: true }); panel(); },
          () => { gh(.24, { duo: 2, op: .45 }); panel(); },
          () => { gh(.13, { duo: 2, op: .4 }); panel(); },
          () => { gh(.05, { duo: 1, op: .35, edge: false }); panel(); },
          () => { panel(); mid({ dx: p.w * .02, edge: true }); },
          () => panel()];
      },
      out: A => {   // копии разлетаются: светлая влево, темная вправо, панель гаснет кусками
        const p = A.P, gh = (v, o) => A.pc(p, Object.assign({ dx: -p.w * v, edge: true }, o));
        return [
          () => { gh(.12, { duo: 2, op: .5 }); A.pc(p); },
          () => { gh(.3, { duo: 2, op: .55 }); A.pc(p, { op: .6 }); gh(-.1, { duo: 1, op: .6 }); },
          () => { gh(.45, { duo: 2, op: .45 }); A.pc({ x: p.x, y: p.y + p.h * .3, w: p.w, h: p.h * .32 }, { dx: p.w * .15, op: .5, edge: true }); },
          () => gh(.56, { duo: 1, op: .35 }),
          () => {}];
      }
    }
  };
  // el — панель (.rpan.show, уже на месте); kind — "in" / "out"; sel — блоки панели сверху вниз (верх каждого видимого = граница полосы)
  window.hxPanel = function (el, kind, cb, sel) {
    if (el._gl) el._gl.stop();
    const vw = window.innerWidth, vh = window.innerHeight, r0 = el.getBoundingClientRect();
    if (reduced() || !el.isConnected || r0.width < 20 || r0.height < 20 || r0.left >= vw - 4) { cb && cb(); return; }
    ensureSvg();
    const P = { x: Math.round(r0.left), y: Math.round(r0.top), w: Math.round(r0.width), h: Math.round(r0.height) };
    const cs = getComputedStyle(el), pk = parseFloat(cs.borderLeftWidth) || 3;
    const ys = [];
    for (const b of el.querySelectorAll(sel || ":scope > *")) {
      const r = b.getBoundingClientRect();
      if (r.height > 0 && r.bottom > r0.top && r.top < r0.bottom) ys.push(Math.round(r.top - r0.top));
    }
    const cuts = [0];   // верх первого блока не граница: полоса 0 — от верха панели (иначе пустая полоска над заголовком — лишний кадр)
    ys.sort((a, b) => a - b).slice(1).forEach(y => { if (y - cuts[cuts.length - 1] >= 24 && P.h - y >= 24) cuts.push(y); });
    cuts.push(P.h);
    const bands = cuts.slice(1).map((y, i) => [cuts[i], y]);
    // прокрутка внутри панели (.rp-in) — копиям как у оригинала; меряется один раз
    const all = [el, ...el.querySelectorAll("*")], scr = [];
    all.forEach((e, i) => { if (e.scrollTop) scr.push([i, e.scrollTop]); });
    const ov = document.createElement("div"); ov.className = "hx-ov"; ov.setAttribute("aria-hidden", "true");
    Object.assign(ov.style, { left: "0", top: "0", width: vw + "px", height: vh + "px", zIndex: "8" });
    const bk = document.createElement("div"); bk.className = "hx-pb";   // пустая панель: свой фон + розовая линия
    Object.assign(bk.style, { left: P.x + "px", top: P.y + "px", width: P.w + "px", height: P.h + "px", background: cs.backgroundColor,
      borderLeft: cs.borderLeftWidth + " solid " + cs.borderLeftColor, boxSizing: "border-box", visibility: "hidden" });
    ov.appendChild(bk);
    const cv = document.createElement("canvas"); cv.className = "hx-glc";
    const dpr = window.devicePixelRatio || 1; cv.width = vw * dpr; cv.height = vh * dpr; ov.appendChild(cv);
    const c2 = cv.getContext("2d"); c2.scale(dpr, dpr);
    document.body.appendChild(ov);
    const pool = [];
    const copy = () => {
      const w = document.createElement("div"); w.className = "hx-pc";
      Object.assign(w.style, { left: P.x + "px", top: P.y + "px", width: P.w + "px", height: P.h + "px" });
      const c = el.cloneNode(true); c.removeAttribute("id"); c.removeAttribute("role"); c.style.cssText = "";
      noFocus(c); copyCanvas(el, c);
      w.appendChild(c); ov.insertBefore(w, cv);
      if (scr.length) { const ca = [c, ...c.querySelectorAll("*")]; scr.forEach(([i, top]) => { if (ca[i]) ca[i].scrollTop = top; }); }
      return w;
    };
    let k = 0;
    const A = {
      P, pk, bands, R: rnd((Date.now() ^ (P.w * 7 + P.h * 13)) >>> 0),
      back() { bk.style.visibility = "visible"; },
      // кусок панели в прямоугольнике r (координаты экрана): dx/dy — сдвиг куска, sx/sy — сдвиг содержимого внутри, zoom — крупнее от центра куска
      pc(r, o = {}) {
        const w = pool[k] || (pool[k] = copy()), c = w.firstChild; k++;
        const lx = r.x - P.x, ly = r.y - P.y, tr = [];
        if (o.sx || o.sy) tr.push(`translate(${o.sx || 0}px, ${o.sy || 0}px)`);
        if (o.zoom) tr.push(`scale(${o.zoom})`);
        c.style.transform = tr.join(" "); c.style.transformOrigin = o.zoom ? `${lx + r.w / 2}px ${ly + r.h / 2}px` : "";
        Object.assign(w.style, { visibility: "visible", clipPath: `inset(${ly}px ${P.w - lx - r.w}px ${P.h - ly - r.h}px ${lx}px)`,
          transform: o.dx || o.dy ? `translate(${o.dx || 0}px, ${o.dy || 0}px)` : "", filter: duoF(o.duo), opacity: o.op != null ? String(o.op) : "" });
        if (o.edge) { const x = r.x + (o.dx || 0), y = r.y + (o.dy || 0); c2.fillStyle = "rgba(255,0,128,.45)"; c2.fillRect(x - 2, y, 2, r.h); c2.fillStyle = "rgba(0,255,255,.4)"; c2.fillRect(x + r.w, y, 2, r.h); }
      },
      slab(i, o) { const [a, b] = SLABS[i]; A.pc({ x: P.x, y: P.y + P.h * a, w: P.w, h: P.h * (b - a) }, o); }
    };
    const frames = PAN[pickTy("rpan-" + kind)][kind](A);
    let i = 0, t = 0;
    const t0 = performance.now();   // кадры по общим часам: медленная отрисовка не растягивает анимацию (стенд: 0,52 с вместо 0,36 при цепочке setTimeout)
    const reset = () => { k = 0; bk.style.visibility = "hidden"; pool.forEach(w => { w.style.visibility = "hidden"; }); c2.clearRect(0, 0, vw, vh); };
    const stop = () => { clearTimeout(t); ov.remove(); el.style.opacity = ""; el._gl = null; if (HX.next === step) HX.next = null; };
    const step = () => {
      reset();
      if (i >= frames.length || !el.isConnected) { stop(); cb && cb(); return; }
      frames[i++](A);
      if (HX.manual) HX.next = step; else t = setTimeout(step, Math.max(10, t0 + i * FR() - performance.now()));   // ≥ 10 мс — каждый кадр успевает нарисоваться
    };
    el._gl = { stop };
    el.style.opacity = "0";
    step();
  };

  window.askYesNo = function (o) {
    return new Promise(resolve => {
      const back = document.createElement("div");
      back.className = "dlg-back" + (o.tall ? " dlg-tall" : "") + (o.cls ? " " + o.cls : "");
      // o.buttons: [{label, val}] — свои кнопки (выбор языка), o.start — какая подсвечена сначала; Esc — false
      const btns = o.buttons || [{ label: o.yes || "YES", val: true }];
      if (!o.buttons && o.no !== null) btns.push({ label: o.no || "NO", val: false });
      ensureObvod();
      back.innerHTML =
        '<div class="dlg" role="alertdialog" aria-modal="true" aria-labelledby="dlg-t">' +
        '<div class="dlg-top"></div>' + ghost(o.title) +
        '<canvas class="dlg-fx"></canvas>' +
        '<div class="dlg-body">' +
        `<h1 class="dlg-title" id="dlg-t"></h1><div class="dlg-text">${o.html || ""}</div>` +
        '<div class="dlg-btns">' + btns.map((b, i) =>
          `<button class="dlg-btn${i === (o.start > 0 ? o.start : 0) ? " focus" : ""}" data-i="${i}"><span class="key">↵</span><span class="lbl"></span>${SWOOSH}</button>`).join("") +
        "</div></div></div>";
      back.querySelector(".dlg-title").textContent = o.title || "";
      back.querySelectorAll(".dlg-btn .lbl").forEach((el, i) => el.textContent = btns[i].label);
      prepWin(back);
      document.body.appendChild(back);
      const els = [...back.querySelectorAll(".dlg-btn")];
      let cur = 0;
      const focus = i => { cur = (i + els.length) % els.length; els.forEach((e, k) => e.classList.toggle("focus", k === cur)); els[cur].focus({ preventScroll: true }); };
      const done = v => {
        document.removeEventListener("keydown", key, true); window.removeEventListener("resize", repaint);
        closeWin(back); resolve(v);
      };
      const key = e => {
        if (e.target && e.target.tagName === "INPUT" && e.key !== "Escape") return;   // поле ввода внутри окна — клавиши его
        // переключатель языка поверх первых окон (R1-05): Esc закрывает список языков, Enter открывает его — не «Вийти» / не кнопка окна
        if (document.querySelector(".lsel.open") || (e.target && e.target.closest && e.target.closest(".lang"))) return;
        if (e.key === "ArrowRight" || e.key === "ArrowLeft" || e.key === "Tab") { e.preventDefault(); focus(cur + (e.key === "ArrowLeft" || e.shiftKey ? -1 : 1)); }
        else if (e.key === "Enter" || e.key === " ") { e.preventDefault(); done(btns[cur].val); }
        else if (e.key === "Escape" && btns.length > 1) { e.preventDefault(); done(false); }
      };
      els.forEach((e, i) => { e.addEventListener("mouseenter", () => focus(i)); e.addEventListener("click", () => done(btns[i].val)); });
      document.addEventListener("keydown", key, true);
      const cv = back.querySelector(".dlg-fx"), repaint = () => paintFx(cv);
      window.addEventListener("resize", repaint);
      requestAnimationFrame(() => { repaint(); focus(o.start > 0 ? o.start : 0); openWin(back); });
      // o.re() — те же {title, html, yes, no} на новом языке (язык сменили, пока окно открыто); живое содержимое (onOpen) не трогаем — для него o.onRelang
      if (o.re || o.onRelang) back._re = () => {
        if (o.re) {
          const x = o.re();
          back.querySelector(".dlg-title").textContent = x.title || "";
          back.querySelector(".dlg-ghost").textContent = x.title || "";
          if (x.html != null && !o.onOpen) back.querySelector(".dlg-text").innerHTML = x.html;
          const labels = x.buttons ? x.buttons.map(b => b.label) : [x.yes || "YES"].concat(x.no !== null ? [x.no || "NO"] : []);
          back.querySelectorAll(".dlg-btn .lbl").forEach((el, i) => { if (labels[i] != null) el.textContent = labels[i]; });
        }
        if (o.onRelang) o.onRelang(back);
      };
      if (o.onOpen) o.onOpen(back.querySelector(".dlg-text"), done);   // живое содержимое (выбор папки и т.п.)
    });
  };
  window.relangDialogs = () => document.querySelectorAll(".dlg-back").forEach(b => b._re && b._re());

  // «идет работа»: та же полоса, без кнопок — заголовок, шаг, полоска прогресса (pct < 0 — бегущая)
  let busy = null;
  window.showBusy = function (title, text, stop) {   // stop = {label, fn}: кнопка «Стоп» под полосой, Esc — то же
    if (!busy) {
      busy = document.createElement("div");
      busy.className = "dlg-back";
      ensureObvod();
      busy.innerHTML = '<div class="dlg" role="status" aria-live="polite">' +
        '<div class="dlg-top"></div>' + ghost(title) + '<canvas class="dlg-fx"></canvas>' +
        '<div class="dlg-body"><h1 class="dlg-title"></h1><div class="dlg-text"><p class="busy-step"></p></div>' +
        '<div class="busy-bar"><i></i></div><div class="busy-log"></div></div></div>';
      prepWin(busy);
      document.body.appendChild(busy);
      const b = busy; requestAnimationFrame(() => { paintFx(b.querySelector(".dlg-fx")); openWin(b); });
    }
    busy.querySelector(".dlg-title").textContent = title || "";
    busy.querySelector(".dlg-ghost").textContent = title || "";
    setStop(stop);
    updateBusy(text, -1);
  };
  function setStop(o) {
    let row = busy.querySelector(".dlg-btns");
    busy.onStop = o ? o.fn : null;
    if (!o) { if (row) row.remove(); return; }
    if (!row) {
      row = document.createElement("div"); row.className = "dlg-btns";
      row.innerHTML = `<button class="dlg-btn focus"><span class="key txt">Esc</span><span class="lbl"></span>${SWOOSH}</button>`;
      busy.querySelector(".dlg-body").appendChild(row);
    }
    row.querySelector(".lbl").textContent = o.label;
    row.querySelector("button").onclick = () => busy && busy.onStop && busy.onStop();
  }
  document.addEventListener("keydown", e => { if (busy && busy.onStop && e.key === "Escape") { e.preventDefault(); busy.onStop(); } }, true);
  // строка под полосой (ошибка / замена / повтор) — видно сразу, по ходу работы; kind: err | warn
  window.busyLog = function (text, kind) {
    if (!busy) return;
    const box = busy.querySelector(".busy-log"), d = document.createElement("div");
    d.className = kind || ""; d.textContent = text; box.appendChild(d); box.scrollTop = box.scrollHeight;
  };
  window.updateBusy = function (text, pct) {
    if (!busy) return;
    if (text != null) busy.querySelector(".busy-step").textContent = text;
    const bar = busy.querySelector(".busy-bar");
    bar.classList.toggle("run", !(pct >= 0));
    bar.querySelector("i").style.width = pct >= 0 ? Math.min(100, pct) + "%" : "";
  };
  window.hideBusy = function () {
    if (!busy) return;
    const b = busy; busy = null; closeWin(b);
  };
})();
