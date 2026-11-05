"use strict";
const $ = s => document.querySelector(s);
const groups = [
 {title:"Відправник", fields:[["senderName","ПІБ","text","Дмитро Коваль"],["senderPhone","Телефон","tel","+380 67 123 45 67"],["from","Місто","text","Київ"],["senderAddress","Адреса або відділення","text","Відділення №12"]]},
 {title:"Отримувач", fields:[["recipientName","ПІБ","text","Олена Бондар"],["recipientPhone","Телефон","tel","+380 93 123 45 67"],["to","Місто","text","Львів"],["recipientAddress","Адреса або відділення","text","Відділення №5 або вулиця, будинок"]]},
 {title:"Посилка та доставка", fields:[["content","Вміст посилки","text","Книги"],["deliveryType","Спосіб доставки","select"],["weight","Вага, кг","number","2"],["width","Ширина, см","number","30"],["height","Висота, см","number","20"],["depth","Глибина, см","number","10"],["value","Оголошена цінність, грн","number","500"],["description","Коментар до відправлення","text","Необов’язково"]]}
];
const fields = [...groups.flatMap(g=>g.fields),["price","Вартість, грн","readonly"],["createdAt","Дата створення","readonly"]];
const required = ["senderName","senderPhone","from","senderAddress","recipientName","recipientPhone","to","recipientAddress","content"];
let postings = [];
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
function cell(row, label, primary, secondary) {
 const td=document.createElement("td");td.dataset.label=label;
 const strong=document.createElement("strong");strong.textContent=primary || "Не вказано";td.append(strong);
 if(secondary){const small=document.createElement("small");small.textContent=secondary;td.append(small);}row.append(td);return td;
}
function renderRows(){
 const query=$("#search").value.trim().toLocaleLowerCase("uk");
 const data=postings.filter(p=>[p.senderName,p.recipientName,p.from,p.to,p.content,p.id].join(" ").toLocaleLowerCase("uk").includes(query));
 $("#rows").replaceChildren();$("#count").textContent=`Усього: ${postings.length} · Показано: ${data.length}`;$("#empty-list").hidden=data.length>0;
 for(const p of data){
  const row=document.createElement("tr");row.dataset.id=p.id;row.tabIndex=0;
  const select=()=>{document.querySelectorAll("tr.selected").forEach(r=>r.classList.remove("selected"));row.classList.add("selected");};row.onclick=select;row.onfocus=select;
  cell(row,"Відправник",p.senderName,p.from);cell(row,"Отримувач",p.recipientName,p.to);
  cell(row,"Посилка",p.content,`${p.weight} кг · ${p.width} × ${p.height} × ${p.depth} см`);
  const delivery=cell(row,"Доставка",deliveryNames[p.deliveryType],p.recipientAddress);delivery.firstChild.className="badge";
  cell(row,"Вартість",p.price.toFixed(2)+" грн");
  const actions=document.createElement("td");actions.className="actions";
  actions.append(link("Деталі",`#details/${p.id}`),link("Редагувати",`#edit/${p.id}`));
  const del=document.createElement("button");del.textContent="Видалити";del.onclick=()=>{pendingDelete=p.id;$("#delete-text").textContent=`№${p.id} · ${p.from} → ${p.to} · ${p.content}`;$("#delete-dialog").showModal();};
  actions.append(del);row.append(actions);$("#rows").append(row);
 }
}
async function loadList(version){$("#rows").replaceChildren();$("#count").textContent="Завантаження…";const data=await api("/postings");if(version!==routeVersion)return;postings=data;renderRows();}
function buildForm(p,id){
 const form=$("#posting-form");form.hidden=false;form.replaceChildren();form.dataset.id=id||"";$("#editor-title").textContent=id?"Редагування відправлення":"Нове відправлення";
 const summaryError=document.createElement("div");summaryError.id="form-error-summary";summaryError.className="error-summary";summaryError.setAttribute("role","alert");summaryError.hidden=true;form.append(summaryError);
 const layout=document.createElement("div");layout.className="form-layout";const sections=document.createElement("div");sections.className="form-sections";layout.append(sections);
 groups.forEach((group,index)=>{
  const section=document.createElement("div");section.className="form-section";
  const heading=document.createElement("div");heading.className="section-heading";const n=document.createElement("span");n.className="section-number";n.textContent=String(index+1).padStart(2,"0");const h=document.createElement("h2");h.textContent=group.title;heading.append(n,h);section.append(heading);
  for(const [name,title,type,placeholder] of group.fields){
   const wrap=document.createElement("div");wrap.className="field";const label=document.createElement("label");label.htmlFor=name;label.textContent=title+((required.includes(name)||["weight","width","height","depth"].includes(name))?" *":"");
   const input=document.createElement(type==="select"?"select":"input");input.id=name;input.name=name;
   if(type==="select")deliveryNames.forEach((v,i)=>{const o=document.createElement("option");o.value=i;o.textContent=v;input.append(o);});
   else{input.type=type;input.placeholder=placeholder||"";if(type==="number"){input.step="any";input.min=name==="value"?"0":"0.01";}if(type==="tel")input.autocomplete=index===0?"section-sender tel":"section-recipient tel";}
   input.value=p?.[name]??(name==="deliveryType"?0:"");
   input.addEventListener("input",()=>{input.classList.remove("invalid");input.removeAttribute("aria-invalid");const error=document.getElementById(name+"-error");if(error)error.textContent="";if(!form.querySelector(".invalid"))summaryError.hidden=true;});
   const hint=document.createElement("span");hint.id=name+"-error";hint.className="error";input.setAttribute("aria-describedby",hint.id);input.setAttribute("aria-required",String(required.includes(name)||["weight","width","height","depth"].includes(name)));wrap.append(label,input,hint);section.append(wrap);
  }sections.append(section);
 });
 const summary=document.createElement("aside");summary.className="card summary";const title=document.createElement("h2");title.textContent=id?`Відправлення №${id}`:"Підсумок";summary.append(title);
 const dl=document.createElement("dl");for(const [key,label] of [["price","Вартість доставки"],["createdAt","Дата створення"]]){const dt=document.createElement("dt"),dd=document.createElement("dd");dt.textContent=label;dd.id=key;dd.textContent=key==="price"?(p?p.price.toFixed(2)+" грн":"Після збереження"):(p?new Date(p.createdAt).toLocaleString("uk-UA"):"Після збереження");if(key==="price")dd.className="price-value";dl.append(dt,dd);}summary.append(dl);
 const note=document.createElement("p");note.className="tariff-note";note.textContent="Вартість розраховується після збереження за навчальними тарифами. Після зміни ваги або доставки її буде перераховано.";summary.append(note);
 const bar=document.createElement("div");bar.className="form-actions";const save=document.createElement("button");save.type="submit";save.textContent=id?"Зберегти зміни":"Створити відправлення";bar.append(save,link("Скасувати","#list"));bar.lastChild.className="button";summary.append(bar);layout.append(summary);form.append(layout);
}
function showErrors(errors) {
 $("#form-error-summary").textContent="Перевірте позначені поля. Введені дані збережено у формі.";$("#form-error-summary").hidden=false;
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
  if (required.includes(name) && !value) errors[name] = [`Введіть ${title.toLowerCase()}.`];
  if (type === "number" && name !== "value" && (!value || !Number.isFinite(data[name]) || data[name] <= 0)) errors[name] = ["Введіть число більше нуля."];
  if (name === "value" && data[name] !== null && (!Number.isFinite(data[name]) || data[name] < 0)) errors[name] = ["Цінність не може бути відʼємною."];
 }
 for(const name of ["senderPhone","recipientPhone"]){const digits=data[name].replace(/[^0-9]/g,"");if(!/^[+0-9 ()-]+$/.test(data[name])||!(/^0[0-9]{9}$/.test(digits)||/^380[0-9]{9}$/.test(digits)))errors[name]=["Номер у форматі 0XX XXX XX XX або +380 XX XXX XX XX."];}
 if (Object.keys(errors).length) { showErrors(errors); return; }
 $("#form-error-summary").hidden=true;
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
 document.querySelectorAll("nav a").forEach(a=>{if(a.getAttribute("href")==="#"+(section==="editor"||section==="details"?"list":section))a.setAttribute("aria-current","page");else a.removeAttribute("aria-current");});
 document.querySelectorAll("main > section").forEach(x => x.hidden = x.id !== section);
 try {
  if (name === "list") await loadList(version);
  if (name === "new") buildForm(null,null);
  if (name === "edit" || name === "details") {
   if(name==="edit") {$("#posting-form").replaceChildren();$("#posting-form").hidden=true;$("#editor-title").textContent="Завантаження відправлення…";}
   const p = await api(`/postings/${encodeURIComponent(id)}`); if (version !== routeVersion) return;
   if (name === "edit") buildForm(p,id);
   else { $("#details-edit").href="#edit/"+p.id;const dl = $("#details-data"); dl.replaceChildren(); for (const [key,title] of [["id","Номер відправлення"],...groups.flatMap((g)=>g.fields.map(f=>[f[0],(g.title==="Посилка та доставка"?"":g.title+" · ")+f[1]])),["price","Вартість, грн"],["createdAt","Дата створення"]]) { const dt = document.createElement("dt"), dd = document.createElement("dd"); dt.textContent = title; dd.textContent = key === "deliveryType" ? deliveryNames[p[key]] : p[key] ?? "Не вказано"; dl.append(dt,dd); } }
  }
 } catch (error) { report(error); }
}
$("#refresh").onclick = () => loadList(routeVersion).catch(report);
$("#cancel-delete").onclick = () => $("#delete-dialog").close();
$("#confirm-delete").onclick = async () => {
 $("#delete-dialog").close(); try { await api(`/postings/${pendingDelete}`,{method:"DELETE"}); message("Відправлення видалено."); await loadList(routeVersion); }
 catch (error) { report(error); if (error.status === 404) await loadList(routeVersion).catch(report); }
};
$("#search").oninput=renderRows;
window.addEventListener("hashchange",route); route();
