"use strict";
const $ = s => document.querySelector(s);
const fields = [
 ["from","Відправник","text"], ["to","Отримувач","text"], ["content","Вміст","text"],
 ["deliveryType","Спосіб доставки","select"], ["weight","Вага, кг","number"],
 ["width","Ширина, см","number"], ["height","Висота, см","number"], ["depth","Глибина, см","number"],
 ["value","Цінність, грн","number"], ["description","Опис","text"], ["price","Вартість, грн","readonly"], ["createdAt","Створено UTC","readonly"]
];
const deliveryNames = ["Відділення", "Курʼєр", "Експрес-курʼєр"];
let pendingDelete = null, routeVersion = 0;
function message(text) { $("#notice").textContent = text; $("#notice").hidden = !text; }
function retryChoice() {
 return new Promise(resolve => {
  const dialog = $("#retry-dialog"); dialog.showModal();
  const finish = answer => { dialog.close(); resolve(answer); };
  $("#retry").onclick = () => finish(true); $("#cancel-retry").onclick = () => finish(false);
  dialog.oncancel = e => { e.preventDefault(); finish(false); };
 });
}
/** Виконує запит до API та дозволяє повторити спробу при втраті з’єднання.
 * @param {string} path Маршрут API.
 * @param {RequestInit} options Параметри HTTP-запиту.
 * @returns {Promise<object|null>} Дані відповіді або null для 204.
 */
async function api(path, options = {}) {
 let response;
 while (true) {
  try { response = await fetch(path, options); break; }
  catch (error) { if (!await retryChoice()) throw { cancelled: true }; }
 }
 if (!response.ok) {
  let problem = {}; try { problem = await response.json(); } catch {}
  throw { status: response.status, errors: problem.errors };
 }
 return response.status === 204 ? null : response.json();
}
function report(error) {
 if (error.cancelled) return;
 if (error.status === 404) { message("Відправлення вже видалено на сервері."); location.hash = "list"; return; }
 console.error(error); message("Сталася помилка. Спробуйте повторити дію пізніше.");
}
function link(text, hash) { const a = document.createElement("a"); a.textContent = text; a.href = hash; return a; }
async function loadList(version) {
 const data = await api("/postings"); if (version !== routeVersion) return;
 $("#rows").replaceChildren(); $("#count").textContent = data.length ? `Відправлень: ${data.length}` : "Відправлень поки немає.";
 for (const p of data) {
  const row = document.createElement("tr"); row.dataset.id = p.id;
  row.onclick = () => { document.querySelectorAll("tr.selected").forEach(r => r.classList.remove("selected")); row.classList.add("selected"); };
  const values = [p.from,p.to,p.content,deliveryNames[p.deliveryType],p.weight,p.price.toFixed(2)];
  const labels = ["Відправник","Отримувач","Вміст","Доставка","Вага, кг","Вартість, грн"];
  values.forEach((value,i) => { const td = document.createElement("td"); td.dataset.label = labels[i]; td.textContent = value; row.append(td); });
  const actions = document.createElement("td"); actions.className = "actions";
  actions.append(link("Деталі",`#details/${p.id}`),link("Редагувати",`#edit/${p.id}`));
  const del = document.createElement("button"); del.textContent = "Видалити";
  del.onclick = () => { pendingDelete = p.id; $("#delete-text").textContent = `Видалити відправлення №${p.id}?`; $("#delete-dialog").showModal(); };
  actions.append(del); row.append(actions); $("#rows").append(row);
 }
}
function buildForm(p, id) {
 const form = $("#posting-form"); form.replaceChildren(); form.dataset.id = id || "";
 $("#editor-title").textContent = id ? "Редагування відправлення" : "Нове відправлення";
 for (const [name,title,type] of fields) {
  const wrap = document.createElement("div"); wrap.className = "field";
  const label = document.createElement("label"); label.htmlFor = name; label.textContent = title;
  const input = document.createElement(type === "select" ? "select" : "input"); input.id = name; input.name = name;
  if (type === "select") deliveryNames.forEach((v,i) => { const option = document.createElement("option"); option.value = i; option.textContent = v; input.append(option); });
  else { input.type = type === "readonly" ? "text" : type; if (type === "number") input.step = "any"; input.readOnly = type === "readonly"; }
  input.value = p?.[name] ?? (name === "deliveryType" ? 0 : name === "price" ? "Розраховує сервер" : name === "createdAt" ? "Після збереження" : "");
  const hint = document.createElement("span"); hint.id = `${name}-error`; hint.className = "error";
  input.setAttribute("aria-describedby",hint.id); wrap.append(label,input,hint); form.append(wrap);
 }
 const bar = document.createElement("div"); bar.className = "form-actions";
 const save = document.createElement("button"); save.type = "submit"; save.textContent = "Зберегти";
 bar.append(save,link("Скасувати","#list")); form.append(bar);
}
function showErrors(errors) {
 for (const [field,texts] of Object.entries(errors || {})) {
  const key = field.charAt(0).toLowerCase() + field.slice(1); const input = document.getElementById(key);
  if (input) { input.classList.add("invalid"); input.setAttribute("aria-invalid","true"); document.getElementById(`${key}-error`).textContent = "! " + texts.join(" "); }
 }
}
$("#posting-form").onsubmit = async e => {
 e.preventDefault(); const form = e.currentTarget, data = {}, errors = {};
 form.querySelectorAll(".error").forEach(x => x.textContent = ""); form.querySelectorAll(".invalid").forEach(x => { x.classList.remove("invalid"); x.removeAttribute("aria-invalid"); });
 for (const [name,title,type] of fields) {
  if (type === "readonly") continue; const value = form.elements[name].value.trim();
  data[name] = type === "number" ? (value === "" && name === "value" ? null : Number(value)) : type === "select" ? Number(value) : value;
  if (["from","to","content"].includes(name) && !value) errors[name] = [`Введіть ${title.toLowerCase()}.`];
  if (type === "number" && name !== "value" && (!value || !Number.isFinite(data[name]) || data[name] <= 0)) errors[name] = ["Введіть число більше нуля."];
  if (name === "value" && data[name] !== null && (!Number.isFinite(data[name]) || data[name] < 0)) errors[name] = ["Цінність не може бути відʼємною."];
 }
 if (Object.keys(errors).length) { showErrors(errors); return; }
 const save = form.querySelector("button[type=submit]"); save.disabled = true;
 try {
  const id = form.dataset.id; await api(id ? `/postings/${id}` : "/postings", {method:id ? "PUT":"POST",headers:{"Content-Type":"application/json"},body:JSON.stringify(data)});
  location.hash = "list"; message("Відправлення збережено.");
 } catch (error) { if (error.status === 400) showErrors(error.errors); else report(error); }
 finally { save.disabled = false; }
};
async function route() {
 const version = ++routeVersion; const [name,id] = (location.hash.slice(1) || "home").split("/");
 const section = name === "new" || name === "edit" ? "editor" : ["home","list","about","details"].includes(name) ? name : "home";
 document.querySelectorAll("main > section").forEach(x => x.hidden = x.id !== section);
 try {
  if (name === "list") await loadList(version);
  if (name === "new") buildForm(null,null);
  if (name === "edit" || name === "details") {
   const p = await api(`/postings/${encodeURIComponent(id)}`); if (version !== routeVersion) return;
   if (name === "edit") buildForm(p,id);
   else { const dl = $("#details-data"); dl.replaceChildren(); for (const [key,title] of [["id","Ідентифікатор"],...fields]) { const dt = document.createElement("dt"), dd = document.createElement("dd"); dt.textContent = title; dd.textContent = key === "deliveryType" ? deliveryNames[p[key]] : p[key] ?? "Не вказано"; dl.append(dt,dd); } }
  }
 } catch (error) { report(error); }
}
$("#refresh").onclick = () => loadList(routeVersion).catch(report);
$("#cancel-delete").onclick = () => $("#delete-dialog").close();
$("#confirm-delete").onclick = async () => {
 $("#delete-dialog").close(); try { await api(`/postings/${pendingDelete}`,{method:"DELETE"}); message("Відправлення видалено."); await loadList(routeVersion); }
 catch (error) { report(error); if (error.status === 404) await loadList(routeVersion).catch(report); }
};
window.addEventListener("hashchange",route); route();
