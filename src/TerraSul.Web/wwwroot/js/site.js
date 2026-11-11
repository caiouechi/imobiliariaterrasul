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
