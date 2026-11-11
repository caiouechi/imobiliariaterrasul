/* Imobiliária Terra Sul · front-end behaviours */
(function () {
  'use strict';
  const $ = (s, r = document) => r.querySelector(s);
  const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));
  const BRL = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });

  /* Preloader + AOS ------------------------------------------- */
  window.addEventListener('load', () => {
    setTimeout(() => document.documentElement.classList.add('is-loaded'), 350);
    if (window.AOS) AOS.init({ duration: 900, once: true, offset: 60, easing: 'ease-out-cubic' });
  });

  /* Header + mobile menu ---------------------------------------- */
  const header = $('#siteHeader');
  const onScroll = () => header && header.classList.toggle('is-scrolled', window.scrollY > 40);
  onScroll(); window.addEventListener('scroll', onScroll, { passive: true });

  const burger = $('#burger'), menu = $('#mobileMenu');
  if (burger && menu) {
    burger.addEventListener('click', () => {
      const open = !menu.classList.contains('is-open');
      menu.classList.toggle('is-open', open);
      menu.setAttribute('aria-hidden', String(!open));
      burger.setAttribute('aria-expanded', String(open));
      document.body.style.overflow = open ? 'hidden' : '';
      document.body.classList.toggle('menu-open', open);
    });
  }

  /* Parallax backgrounds ---------------------------------------- */
  const parallax = $$('[data-parallax]');
  if (parallax.length && !matchMedia('(prefers-reduced-motion: reduce)').matches) {
    const tick = () => {
      const vh = window.innerHeight;
      parallax.forEach(el => {
        const r = el.parentElement.getBoundingClientRect();
        if (r.bottom < 0 || r.top > vh) return;
        const p = (r.top + r.height / 2 - vh / 2) / vh;
        el.style.transform = `translateY(${p * 80}px)`;
      });
    };
    tick(); window.addEventListener('scroll', tick, { passive: true });
  }

  /* Counters ---------------------------------------------------- */
  const counters = $$('[data-count]');
  if (counters.length && 'IntersectionObserver' in window) {
    const io = new IntersectionObserver(entries => entries.forEach(e => {
      if (!e.isIntersecting) return;
      const el = e.target, target = parseFloat(el.dataset.count), dec = +(el.dataset.decimals || 0);
      const pre = el.dataset.prefix || '', suf = el.dataset.suffix || '', t0 = performance.now();
      const step = now => {
        const k = Math.min(1, (now - t0) / 1800), ease = 1 - Math.pow(1 - k, 3);
        el.textContent = pre + (target * ease).toLocaleString('pt-BR', { minimumFractionDigits: dec, maximumFractionDigits: dec }) + suf;
        if (k < 1) requestAnimationFrame(step);
      };
      requestAnimationFrame(step); io.unobserve(el);
    }), { threshold: .6 });
    counters.forEach(c => io.observe(c));
  }

  /* Hero slider ------------------------------------------------- */
  if ($('#heroSwiper') && window.Swiper) {
    const delay = 7000, prog = $('.hero__progress');
    const restart = () => { if (!prog) return; prog.classList.remove('is-running'); void prog.offsetWidth; prog.style.setProperty('--delay', delay + 'ms'); prog.classList.add('is-running'); };
    new Swiper('#heroSwiper', {
      effect: 'fade', fadeEffect: { crossFade: true }, speed: 1400, loop: true, parallax: true,
      autoplay: { delay, disableOnInteraction: false },
      navigation: { nextEl: '.hero__next', prevEl: '.hero__prev' },
      on: {
        init: restart,
        slideChange(s) { const c = $('.hero__cur'); if (c) c.textContent = String(s.realIndex + 1).padStart(2, '0'); restart(); }
      }
    });
  }

  /* Featured carousel ------------------------------------------- */
  if ($('#featuredSwiper') && window.Swiper) {
    new Swiper('#featuredSwiper', {
      slidesPerView: 'auto', spaceBetween: 26, speed: 800, grabCursor: true,
      navigation: { nextEl: '.feat-next', prevEl: '.feat-prev' },
      scrollbar: { el: '.featured__scrollbar', draggable: true },
      mousewheel: { forceToAxis: true }
    });
  }

  /* Testimonials / Instagram / dev galleries ------------------- */
  if ($('#testiSwiper') && window.Swiper) new Swiper('#testiSwiper', { loop: true, speed: 900, autoplay: { delay: 6000 }, pagination: { el: '.testi__pag', clickable: true }, autoHeight: true });
  if ($('#instaSwiper') && window.Swiper) new Swiper('#instaSwiper', { slidesPerView: 'auto', spaceBetween: 14, freeMode: true, grabCursor: true, mousewheel: { forceToAxis: true } });
  $$('.dev-row__swiper').forEach(el => new Swiper(el, { loop: true, speed: 1000, effect: 'fade', autoplay: { delay: 4000 }, pagination: { el: el.querySelector('.swiper-pagination'), clickable: true } }));
  if ($('#devGallery') && window.Swiper) new Swiper('#devGallery', { slidesPerView: 'auto', centeredSlides: true, spaceBetween: 20, loop: true, speed: 900, grabCursor: true, pagination: { el: '#devGallery .swiper-pagination', clickable: true }, autoplay: { delay: 5000 } });

  /* Lightbox ---------------------------------------------------- */
  let lightbox = null;
  if (window.GLightbox && $('.glightbox')) lightbox = GLightbox({ selector: '.glightbox', touchNavigation: true, loop: true, closeButton: true });

  /* Tabs -------------------------------------------------------- */
  const tabs = $('#devTabs');
  if (tabs) {
    const ink = $('.tabs__ink', tabs), btns = $$('[data-tab]', tabs);
    const moveInk = b => { ink.style.left = b.offsetLeft + 'px'; ink.style.width = b.offsetWidth + 'px'; };
    btns.forEach(b => b.addEventListener('click', () => {
      btns.forEach(x => { x.classList.toggle('is-active', x === b); x.setAttribute('aria-selected', String(x === b)); });
      $$('[data-panel]', tabs).forEach(p => p.classList.toggle('is-active', p.dataset.panel === b.dataset.tab));
      moveInk(b);
    }));
    const first = $('[data-tab].is-active', tabs); if (first) moveInk(first);
    window.addEventListener('resize', () => { const a = $('[data-tab].is-active', tabs); if (a) moveInk(a); });
  }

  /* Favorites (localStorage) ------------------------------------ */
  const FAV_KEY = 'ts:favs';
  const favs = new Set(JSON.parse(localStorage.getItem(FAV_KEY) || '[]'));
  const paintFavs = () => $$('[data-fav]').forEach(b => b.classList.toggle('is-active', favs.has(b.dataset.fav)));
  document.addEventListener('click', e => {
    const b = e.target.closest('[data-fav]'); if (!b) return;
    e.preventDefault();
    favs.has(b.dataset.fav) ? favs.delete(b.dataset.fav) : favs.add(b.dataset.fav);
    localStorage.setItem(FAV_KEY, JSON.stringify([...favs])); paintFavs();
  });
  paintFavs();

  /* Share ------------------------------------------------------- */
  document.addEventListener('click', async e => {
    const b = e.target.closest('[data-share]'); if (!b) return;
    const data = { title: b.dataset.share, url: location.href };
    try { if (navigator.share) await navigator.share(data); else { await navigator.clipboard.writeText(location.href); b.classList.add('is-active'); b.lastChild.textContent = 'Link copiado'; } } catch { }
  });

  /* Custom dropdowns (progressive enhancement of <select>) ------ */
  (function enhanceSelects() {
    const selects = $$('select:not([multiple]):not([data-native])');
    if (!selects.length) return;
    let openPanel = null;
    const closeOpen = () => { if (openPanel) openPanel.close(); };
    document.addEventListener('click', e => { if (openPanel && !openPanel.root.contains(e.target) && !openPanel.panel.contains(e.target)) closeOpen(); });
    document.addEventListener('keydown', e => { if (e.key === 'Escape') closeOpen(); });
    window.addEventListener('resize', closeOpen);
    window.addEventListener('scroll', () => openPanel && openPanel.place(), { passive: true, capture: true });

    selects.forEach((sel, n) => {
      const root = document.createElement('div');
      root.className = 'nsel' + (sel.classList.contains('input') ? ' nsel--input' : '') + (sel.closest('.toolbar__sort') ? ' nsel--line' : '') + (sel.closest('.search-bar__field') ? ' nsel--bare' : '');
      sel.parentNode.insertBefore(root, sel);
      root.appendChild(sel);
      sel.classList.add('nsel__native'); sel.tabIndex = -1; sel.setAttribute('aria-hidden', 'true');

      const id = 'nsel-' + n;
      const trigger = document.createElement('button');
      trigger.type = 'button'; trigger.className = 'nsel__trigger';
      trigger.setAttribute('aria-haspopup', 'listbox'); trigger.setAttribute('aria-expanded', 'false'); trigger.setAttribute('aria-controls', id);
      const lbl = sel.closest('.search-bar__field, .filters__group, .toolbar__sort')?.querySelector('label, .filters__label, span');
      if (lbl) trigger.setAttribute('aria-label', lbl.textContent.trim());
      trigger.innerHTML = '<span class="nsel__value"></span><svg class="nsel__chev" viewBox="0 0 24 24" aria-hidden="true"><path d="m6 9 6 6 6-6" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>';
      root.appendChild(trigger);

      const panel = document.createElement('ul');
      panel.className = 'nsel__panel'; panel.id = id; panel.setAttribute('role', 'listbox'); panel.tabIndex = -1;
      const items = Array.from(sel.options).map((o, i) => {
        const li = document.createElement('li');
        li.className = 'nsel__opt'; li.setAttribute('role', 'option'); li.dataset.i = i;
        li.innerHTML = '<span>' + o.textContent + '</span><svg class="nsel__check" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12 4.5 4.5L19 7" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>';
        li.addEventListener('click', () => { choose(i); api.close(); trigger.focus(); });
        li.addEventListener('mousemove', () => focusItem(i, false));
        panel.appendChild(li); return li;
      });

      let active = sel.selectedIndex;
      const paint = () => {
        const v = $('.nsel__value', trigger); v.textContent = sel.options[sel.selectedIndex]?.textContent || '';
        root.classList.toggle('is-placeholder', sel.selectedIndex === 0 && sel.options[0].value === '');
        items.forEach((li, i) => { li.classList.toggle('is-selected', i === sel.selectedIndex); li.setAttribute('aria-selected', String(i === sel.selectedIndex)); });
      };
      const focusItem = (i, scroll = true) => {
        active = Math.max(0, Math.min(items.length - 1, i));
        items.forEach((li, k) => li.classList.toggle('is-active', k === active));
        if (scroll) items[active].scrollIntoView({ block: 'nearest' });
      };
      const choose = i => {
        if (sel.selectedIndex === i) return;
        sel.selectedIndex = i; paint();
        sel.dispatchEvent(new Event('change', { bubbles: true }));
      };
      const api = {
        root, panel,
        place() {
          const r = trigger.getBoundingClientRect(), vh = window.innerHeight;
          const h = Math.min(panel.scrollHeight + 2, 320);
          const below = vh - r.bottom - 12;
          const up = below < h && r.top > below;
          panel.style.width = Math.max(r.width, 220) + 'px';
          panel.style.left = Math.min(r.left, window.innerWidth - Math.max(r.width, 220) - 8) + 'px';
          panel.style.maxHeight = (up ? r.top - 12 : below) + 'px';
          panel.style.top = up ? '' : r.bottom + 6 + 'px';
          panel.style.bottom = up ? vh - r.top + 6 + 'px' : '';
          panel.classList.toggle('nsel__panel--up', up);
        },
        open() {
          if (openPanel && openPanel !== api) openPanel.close();
          document.body.appendChild(panel); api.place();
          root.classList.add('is-open'); trigger.setAttribute('aria-expanded', 'true');
          requestAnimationFrame(() => panel.classList.add('is-open'));
          focusItem(sel.selectedIndex); openPanel = api;
        },
        close() {
          if (openPanel !== api) return;
          panel.classList.remove('is-open'); root.classList.remove('is-open'); trigger.setAttribute('aria-expanded', 'false');
          setTimeout(() => { if (!panel.classList.contains('is-open')) panel.remove(); }, 220);
          openPanel = null;
        }
      };

      trigger.addEventListener('click', () => root.classList.contains('is-open') ? api.close() : api.open());
      let typed = '', typeTimer;
      trigger.addEventListener('keydown', e => {
        const open = root.classList.contains('is-open');
        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
          e.preventDefault();
          if (!open) { api.open(); return; }
          focusItem(active + (e.key === 'ArrowDown' ? 1 : -1));
        } else if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          if (open) { choose(active); api.close(); } else api.open();
        } else if (e.key === 'Home' && open) { e.preventDefault(); focusItem(0); }
        else if (e.key === 'End' && open) { e.preventDefault(); focusItem(items.length - 1); }
        else if (e.key === 'Tab') { api.close(); }
        else if (e.key.length === 1 && /\S/.test(e.key)) {
          clearTimeout(typeTimer); typed += e.key.toLowerCase(); typeTimer = setTimeout(() => typed = '', 600);
          const hit = items.findIndex(li => li.textContent.trim().toLowerCase().startsWith(typed));
          if (hit > -1) { if (open) focusItem(hit); else choose(hit); }
        }
      });
      sel.addEventListener('change', paint);
      sel.form?.addEventListener('reset', () => setTimeout(paint, 0));
      paint();
    });
  })();

  /* Listing filters (AJAX) -------------------------------------- */
  const form = $('#filterForm');
  if (form) {
    const results = $('#results'), gridUrl = form.dataset.gridUrl;
    const priceEl = $('#priceRange');
    if (priceEl && window.noUiSlider) {
      const min = +priceEl.dataset.min, max = +priceEl.dataset.max;
      noUiSlider.create(priceEl, { start: [+priceEl.dataset.startMin, +priceEl.dataset.startMax], connect: true, range: { min, '30%': min + (max - min) * .05, '70%': min + (max - min) * .25, max }, step: 10000 });
      priceEl.noUiSlider.on('update', v => { $('#priceMinLbl').textContent = BRL.format(v[0]); $('#priceMaxLbl').textContent = BRL.format(v[1]); });
      priceEl.noUiSlider.on('change', v => { $('#min').value = Math.round(v[0]) === min ? '' : Math.round(v[0]); $('#max').value = Math.round(v[1]) === max ? '' : Math.round(v[1]); apply(); });
    }
    let timer;
    const apply = () => {
      clearTimeout(timer);
      timer = setTimeout(async () => {
        const params = new URLSearchParams(new FormData(form));
        [...params.keys()].forEach(k => { if (!params.get(k)) params.delete(k); });
        history.replaceState(null, '', location.pathname + (params.toString() ? '?' + params : ''));
        results.classList.add('is-loading');
        try {
          const html = await (await fetch(gridUrl + '?' + params, { headers: { 'X-Requested-With': 'fetch' } })).text();
          results.innerHTML = html; paintFavs();
        } finally { results.classList.remove('is-loading'); }
      }, 120);
    };
    form.addEventListener('submit', e => { e.preventDefault(); apply(); $('#filters').classList.remove('is-open'); });
    form.addEventListener('change', e => { if (e.target.type !== 'number') apply(); });
    $('#q').addEventListener('input', apply);
    $('#area').addEventListener('input', apply);
    $$('.chips input').forEach(r => r.addEventListener('click', function () { if (this.dataset.was === 'checked') { this.checked = false; this.dataset.was = ''; apply(); } else { $$('input', this.closest('.chips')).forEach(x => x.dataset.was = ''); this.dataset.was = 'checked'; } }));
    $('#sortSelect').addEventListener('change', e => { $('#ordem').value = e.target.value; apply(); });
    $$('.view-toggle button').forEach(b => b.addEventListener('click', () => { $$('.view-toggle button').forEach(x => x.classList.toggle('is-active', x === b)); $('#view').value = b.dataset.view; $('.pgrid', results)?.classList.toggle('pgrid--list', b.dataset.view === 'list'); history.replaceState(null, '', location.pathname + '?' + new URLSearchParams(new FormData(form))); }));
    $('#filterReset').addEventListener('click', () => { form.reset(); $$('input[type=hidden]', form).forEach(h => { if (h.id !== 'view') h.value = ''; }); $('#ordem').value = 'destaque'; if (priceEl?.noUiSlider) priceEl.noUiSlider.set([+priceEl.dataset.min, +priceEl.dataset.max]); apply(); });
    $('#filtersToggle')?.addEventListener('click', () => $('#filters').classList.toggle('is-open'));
  }

  /* Property gallery -------------------------------------------- */
  if ($('#galleryMain') && window.Swiper) {
    const thumbs = new Swiper('#galleryThumbs', { slidesPerView: 'auto', spaceBetween: 10, watchSlidesProgress: true, freeMode: true });
    const main = new Swiper('#galleryMain', {
      speed: 900, loop: false, grabCursor: true, keyboard: { enabled: true },
      navigation: { nextEl: '.gal-next', prevEl: '.gal-prev' }, thumbs: { swiper: thumbs },
      on: { slideChange(s) { const c = $('.gallery__cur'); if (c) c.textContent = s.realIndex + 1; } }
    });
    $('#galleryExpand')?.addEventListener('click', () => { if (lightbox) lightbox.openAt(main.realIndex); });
  }

  /* Mortgage simulator (Price table) ---------------------------- */
  const sim = $('#simulator');
  if (sim && window.noUiSlider) {
    const price = +sim.dataset.price;
    const mk = (id, start, min, max, step) => { const el = $(id); noUiSlider.create(el, { start, connect: [true, false], range: { min, max }, step }); return el.noUiSlider; };
    const down = mk('#simDown', 30, 10, 90, 5), term = mk('#simTerm', 360, 60, 420, 12), rate = mk('#simRate', 10.5, 6, 16, .25);
    const calc = () => {
      const d = +down.get(), n = +term.get(), r = +rate.get();
      const entrada = price * d / 100, pv = price - entrada, i = Math.pow(1 + r / 100, 1 / 12) - 1;
      const pmt = pv * i / (1 - Math.pow(1 + i, -n));
      $('#simDownLbl').textContent = d + '%'; $('#simTermLbl').textContent = n + ' meses'; $('#simRateLbl').textContent = r.toLocaleString('pt-BR') + '%';
      $('#simFinanced').textContent = BRL.format(pv); $('#simInstallment').textContent = BRL.format(pmt); $('#simDownValue').textContent = BRL.format(entrada);
    };
    [down, term, rate].forEach(s => s.on('update', calc));
  }
})();
