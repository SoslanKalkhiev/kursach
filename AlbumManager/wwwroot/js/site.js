// Soundbox — клиентская логика: вкладки, модалка (просмотр/форма), звёздный ввод, поиск iTunes.

// ---- Переключатель вида «Сетка / Список» (настройка отображения, без сервера) ----
function initViewToggle() {
    const lib = document.getElementById('library');
    const toggle = document.getElementById('viewToggle');
    if (!lib || !toggle) return;

    const apply = (view) => {
        lib.classList.toggle('album-grid', view !== 'list');
        lib.classList.toggle('album-list', view === 'list');
        toggle.querySelectorAll('.vt-btn').forEach(b =>
            b.classList.toggle('active', b.dataset.view === view));
    };

    apply(localStorage.getItem('soundbox-view') || 'grid');

    toggle.addEventListener('click', (e) => {
        const btn = e.target.closest('.vt-btn');
        if (!btn) return;
        localStorage.setItem('soundbox-view', btn.dataset.view);
        apply(btn.dataset.view);
    });
}

document.addEventListener('DOMContentLoaded', initViewToggle);

// ---- Вкладки-«библиотека» ----
function selectTab(tab) {
    const input = document.getElementById('tabInput');
    if (input) {
        input.value = tab;
        document.getElementById('filterForm').submit();
    }
}

// ---- Модалка ----
let albumModal = null;
function getModal() {
    if (!albumModal) {
        albumModal = new bootstrap.Modal(document.getElementById('albumModal'));
    }
    return albumModal;
}

function setModalBody(html) {
    document.getElementById('albumModalBody').innerHTML = html;
}

function modalSkeleton() {
    setModalBody('<div class="modal-body"><div class="modal-skeleton">'
        + '<div class="sk sk-cover"></div><div class="sk-lines">'
        + '<div class="sk sk-line"></div><div class="sk sk-line short"></div>'
        + '<div class="sk sk-line"></div></div></div></div>');
}

// Antiforgery-токен читаем из DOM в момент запроса (не кэшируем — форма может
// появиться позже скрипта). Возвращает текущее значение скрытого поля.
function antiforgeryToken() {
    return document.querySelector('#__af input[name="__RequestVerificationToken"]')?.value || '';
}

// POST с antiforgery-токеном
async function postForm(url, fields) {
    const data = new FormData();
    for (const [k, v] of Object.entries(fields)) data.append(k, v);
    return fetch(url, {
        method: 'POST',
        headers: { 'RequestVerificationToken': antiforgeryToken() },
        body: data
    });
}

// ---- Детальный просмотр ----
async function openDetail(id) {
    modalSkeleton();
    getModal().show();
    const resp = await fetch(`/Albums/Details?id=${encodeURIComponent(id)}`);
    if (!resp.ok) { setModalBody('<div class="modal-body">Не удалось загрузить альбом.</div>'); return; }
    setModalBody(await resp.text());
    wireDetail();
}

function wireDetail() {
    const body = document.getElementById('albumModalBody');

    body.querySelector('[data-action="edit"]')?.addEventListener('click', (e) => {
        openAlbum(e.currentTarget.dataset.id); // заменит содержимое модалки формой
    });

    body.querySelector('[data-action="toggle-fav"]')?.addEventListener('click', async (e) => {
        const resp = await postForm('/Albums/ToggleFavorite', { id: e.currentTarget.dataset.id });
        if (resp.ok) location.reload(); else alert('Не удалось обновить избранное.');
    });

    body.querySelector('[data-action="delete"]')?.addEventListener('click', async (e) => {
        if (!confirm('Удалить этот альбом?')) return;
        const resp = await postForm('/Albums/Delete', { id: e.currentTarget.dataset.id });
        if (resp.ok) location.reload(); else alert('Не удалось удалить альбом.');
    });
}

// ---- Форма добавления/редактирования ----
async function openAlbum(id) {
    modalSkeleton();
    getModal().show();
    const url = id ? `/Albums/Form?id=${encodeURIComponent(id)}` : '/Albums/Form';
    const resp = await fetch(url);
    if (!resp.ok) { setModalBody('<div class="modal-body">Не удалось загрузить форму.</div>'); return; }
    setModalBody(await resp.text());
    wireUpForm();
}

function wireUpForm() {
    const form = document.getElementById('albumForm');
    if (!form) return;

    initStarInput();
    initCoverPreview();
    initCoverUpload();
    initItunesSearch();

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorBox = document.getElementById('formError');
        errorBox.classList.add('d-none');

        const resp = await fetch('/Albums/Save', {
            method: 'POST',
            headers: { 'RequestVerificationToken': antiforgeryToken() },
            body: new FormData(form)
        });

        if (resp.ok) {
            location.reload();
        } else {
            errorBox.textContent = (await resp.text()) || 'Не удалось сохранить.';
            errorBox.classList.remove('d-none');
        }
    });

    document.getElementById('deleteBtn')?.addEventListener('click', async (e) => {
        if (!confirm('Удалить этот альбом?')) return;
        const resp = await postForm('/Albums/Delete', { id: e.currentTarget.dataset.id });
        if (resp.ok) location.reload(); else alert('Не удалось удалить альбом.');
    });
}

// Живой предпросмотр обложки по ссылке
function initCoverPreview() {
    const input = document.getElementById('coverUrl');
    const img = document.getElementById('coverPreview');
    if (!input || !img) return;
    input.addEventListener('input', () => setCover(input.value.trim()));
}

function setCover(url) {
    const img = document.getElementById('coverPreview');
    if (!img) return;
    if (url) img.src = url;
    else img.removeAttribute('src');
}

// Загрузка обложки файлом: отправляем на сервер, в ответ получаем URL.
function initCoverUpload() {
    const btn = document.getElementById('coverUploadBtn');
    const fileInput = document.getElementById('coverFile');
    const urlInput = document.getElementById('coverUrl');
    const status = document.getElementById('coverStatus');
    if (!btn || !fileInput || !urlInput) return;

    btn.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', async () => {
        const f = fileInput.files && fileInput.files[0];
        if (!f) return;
        if (f.size > 5 * 1024 * 1024) { status.textContent = 'Файл больше 5 МБ'; status.className = 'cover-status err'; return; }

        status.textContent = 'Загрузка…';
        status.className = 'cover-status';
        const data = new FormData();
        data.append('file', f);

        try {
            const resp = await fetch('/Albums/UploadCover', {
                method: 'POST',
                headers: { 'RequestVerificationToken': antiforgeryToken() },
                body: data
            });
            if (!resp.ok) { status.textContent = (await resp.text()) || 'Не удалось загрузить'; status.className = 'cover-status err'; return; }
            const json = await resp.json();
            urlInput.value = json.url;
            setCover(json.url);
            status.textContent = '✓ Загружено: ' + f.name;
            status.className = 'cover-status ok';
        } catch {
            status.textContent = 'Ошибка загрузки';
            status.className = 'cover-status err';
        }
    });
}

// ---- Поиск в iTunes и автозаполнение формы ----
function initItunesSearch() {
    const input = document.getElementById('itunesQuery');
    const btn = document.getElementById('itunesBtn');
    const box = document.getElementById('itunesResults');
    if (!input || !btn || !box) return;

    async function run() {
        const q = input.value.trim();
        if (!q) return;
        box.innerHTML = '<div class="itunes-loading">Ищем…</div>';
        try {
            const resp = await fetch(`/Albums/Search?q=${encodeURIComponent(q)}`);
            const items = resp.ok ? await resp.json() : [];
            renderResults(items);
        } catch {
            box.innerHTML = '<div class="itunes-empty">Поиск недоступен. Введите данные вручную.</div>';
        }
    }

    function renderResults(items) {
        if (!items.length) {
            box.innerHTML = '<div class="itunes-empty">Ничего не найдено.</div>';
            return;
        }
        box.innerHTML = '';
        items.forEach(it => {
            const el = document.createElement('button');
            el.type = 'button';
            el.className = 'itunes-item';
            el.innerHTML =
                `<img src="${it.coverUrl}" alt="" onerror="this.style.visibility='hidden'" />` +
                `<span class="ii-text"><span class="ii-title">${escapeHtml(it.title)}</span>` +
                `<span class="ii-artist">${escapeHtml(it.artist)}${it.genre ? ' · ' + escapeHtml(it.genre) : ''}</span></span>`;
            el.addEventListener('click', () => fillFromResult(it, el));
            box.appendChild(el);
        });
    }

    btn.addEventListener('click', run);
    input.addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); run(); } });
}

function fillFromResult(it, el) {
    const form = document.getElementById('albumForm');
    if (!form) return;
    form.querySelector('[name="title"]').value = it.title || '';
    form.querySelector('[name="artist"]').value = it.artist || '';
    if (it.genre) form.querySelector('[name="genres"]').value = it.genre;
    if (it.releaseDate) form.querySelector('[name="releaseDate"]').value = it.releaseDate;
    form.querySelector('[name="coverUrl"]').value = it.coverUrl || '';
    setCover(it.coverUrl || '');
    // подсветим выбранный результат
    document.querySelectorAll('.itunes-item').forEach(x => x.classList.remove('selected'));
    el?.classList.add('selected');
}

function escapeHtml(s) {
    return (s || '').replace(/[&<>"']/g, c =>
        ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

// ---- Интерактивный ввод оценки 0–5 с половинами звёзд ----
function initStarInput() {
    const container = document.getElementById('starInput');
    const fill = document.getElementById('starFill');
    const hidden = document.getElementById('ratingInput');
    const label = document.getElementById('ratingLabel');
    if (!container || !fill || !hidden) return;

    const current = () => parseFloat(hidden.value) || 0;

    function valueFromEvent(e) {
        const rect = container.getBoundingClientRect();
        const ratio = (e.clientX - rect.left) / rect.width;
        return Math.min(5, Math.max(0.5, Math.ceil(ratio * 10) / 2));
    }

    function paint(value) {
        fill.style.width = (value / 5 * 100) + '%';
        if (label) label.textContent = value > 0 ? value.toString() : '';
        container.setAttribute('aria-valuenow', value);
    }

    container.addEventListener('mousemove', (e) => paint(valueFromEvent(e)));
    container.addEventListener('mouseleave', () => paint(current()));
    container.addEventListener('click', (e) => {
        const v = valueFromEvent(e);
        hidden.value = v;
        paint(v);
    });

    document.getElementById('clearRating')?.addEventListener('click', () => {
        hidden.value = '';
        paint(0);
    });

    paint(current());
}
