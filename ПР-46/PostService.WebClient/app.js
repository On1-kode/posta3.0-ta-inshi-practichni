'use strict';

const form = document.querySelector('#posting-form');
const editor = document.querySelector('#editor');
const rows = document.querySelector('#postings');
const notice = document.querySelector('#notice');
const message = document.querySelector('#message');
const errors = document.querySelector('#errors');
const retryActions = document.querySelector('#retry-actions');
let editingId = null;
let retryOperation = null;
let busy = false;

function showNotice(text, details = []) {
  notice.hidden = false;
  message.textContent = text;
  errors.replaceChildren();
  for (const detail of details) {
    const item = document.createElement('li');
    item.textContent = detail;
    errors.append(item);
  }
  retryActions.hidden = true;
  retryOperation = null;
}

function clearNotice() {
  notice.hidden = true;
  retryActions.hidden = true;
  retryOperation = null;
}

async function request(path, method = 'GET', value = null) {
  const response = await fetch(path, {
    method,
    headers: value === null ? {} : { 'Content-Type': 'application/json' },
    body: value === null ? null : JSON.stringify(value)
  });
  if (!response.ok) {
    const error = new Error('Не вдалося виконати запит.');
    error.status = response.status;
    if (response.status === 400) {
      const body = await response.json();
      error.details = Object.values(body.errors || {}).flat();
    }
    throw error;
  }
  return response.status === 204 ? null : response.json();
}

async function execute(operation) {
  if (busy) return;
  clearNotice();
  busy = true;
  document.querySelectorAll('button').forEach(button => button.disabled = true);
  try {
    await operation();
  } catch (error) {
    if (error instanceof TypeError) {
      showNotice('Сервер недоступний. Повторити спробу?');
      retryOperation = operation;
      retryActions.hidden = false;
    } else if (error.status === 400) {
      showNotice('Перевірте введені дані.', error.details);
    } else if (error.status === 404) {
      editor.hidden = true;
      try {
        await loadPostings();
        showNotice('Відправлення вже видалено на сервері. Список оновлено.');
      } catch (reloadError) {
        console.error(reloadError);
        showNotice('Не вдалося оновити список. Спробуйте ще раз.');
      }
    } else {
      console.error(error);
      showNotice('Сталася непередбачена помилка. Спробуйте оновити список пізніше.');
    }
  } finally {
    busy = false;
    document.querySelectorAll('button').forEach(button => button.disabled = false);
  }
}

async function loadPostings() {
  const postings = await request('/postings');
  rows.replaceChildren();
  document.querySelector('#empty').hidden = postings.length > 0;
  for (const posting of postings) {
    const row = document.createElement('tr');
    row.dataset.id = posting.id;
    for (const value of [posting.id, posting.trackNumber, posting.weight,
      posting.senderAddress, posting.recipientAddress, posting.status]) {
      const cell = document.createElement('td');
      cell.textContent = value;
      row.append(cell);
    }
    const actions = document.createElement('td');
    const edit = document.createElement('button');
    edit.type = 'button';
    edit.textContent = 'Редагувати';
    edit.className = 'secondary';
    edit.addEventListener('click', () => execute(() => editPosting(posting.id)));
    const remove = document.createElement('button');
    remove.type = 'button';
    remove.textContent = 'Видалити';
    remove.className = 'danger';
    remove.addEventListener('click', () => execute(async () => {
      await request(`/postings/${posting.id}`, 'DELETE');
      if (editingId === posting.id) editor.hidden = true;
      await loadPostings();
    }));
    actions.append(edit, remove);
    row.append(actions);
    rows.append(row);
  }
}

function openEditor(posting = null) {
  form.reset();
  editingId = posting ? posting.id : null;
  document.querySelector('#editor-title').textContent = posting ? 'Редагування відправлення' : 'Нове відправлення';
  for (const name of ['trackNumber', 'weight', 'senderAddress', 'recipientAddress', 'status']) {
    form.elements.namedItem(name).value = posting ? posting[name] : '';
  }
  editor.hidden = false;
  form.elements.namedItem('trackNumber').focus();
}

async function editPosting(id) {
  openEditor(await request(`/postings/${id}`));
}

document.querySelector('#add').addEventListener('click', () => {
  clearNotice();
  openEditor();
});
document.querySelector('#reload').addEventListener('click', () => execute(loadPostings));
document.querySelector('#cancel').addEventListener('click', () => {
  editor.hidden = true;
  clearNotice();
});
document.querySelector('#retry').addEventListener('click', () => {
  const operation = retryOperation;
  if (operation) execute(operation);
});
document.querySelector('#cancel-retry').addEventListener('click', clearNotice);
form.addEventListener('submit', event => {
  event.preventDefault();
  const value = Object.fromEntries(new FormData(form));
  value.weight = Number(value.weight);
  const id = editingId;
  execute(async () => {
    await request(id === null ? '/postings' : `/postings/${id}`, id === null ? 'POST' : 'PUT', value);
    editor.hidden = true;
    await loadPostings();
  });
});

execute(loadPostings);
