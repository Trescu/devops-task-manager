"use strict";
const $ = (id) => document.getElementById(id);
const state = { tasks: [], editing: null };
const statusLabels = { "todo":"Teendő", "in-progress":"Folyamatban", "done":"Elvégezve" };
const priorityLabels = { "low":"Alacsony", "medium":"Közepes", "high":"Magas" };
const toast = (text) => {
  const target = $("toast");
  target.textContent = text;
  target.classList.add("show");
  clearTimeout(toast.timer);
  toast.timer = setTimeout(() => target.classList.remove("show"), 3100);
};
async function request(url, options) {
  const response = await fetch(url, options);
  if (!response.ok) {
    let msg = "HTTP " + response.status;
    try { const error = await response.json(); msg = error.error || msg; } catch { /* no JSON body */ }
    throw new Error(msg);
  }
  return response.status === 204 ? null : response.json();
}
const render = () => {
  $("stat-total").textContent = state.tasks.length;
  $("stat-progress").textContent = state.tasks.filter(t => t.status === "in-progress").length;
  $("stat-done").textContent = state.tasks.filter(t => t.status === "done").length;
  $("task-count").textContent = state.tasks.length;
  const search = $("search").value.trim().toLocaleLowerCase("hu");
  const filter = $("filter").value;
  const tasks = state.tasks.filter(t =>
    (filter === "all" || t.status === filter) &&
    (t.title.toLocaleLowerCase("hu").includes(search) || t.description.toLocaleLowerCase("hu").includes(search))
  );
  $("footer-count").textContent = tasks.length + " feladat";
  const tbody = $("task-rows");
  tbody.replaceChildren();
  if (!tasks.length) {
    const tr = document.createElement("tr"), cell = document.createElement("td");
    cell.colSpan = 5; cell.className = "empty";
    cell.textContent = state.tasks.length ? "Nincs a szűrésnek megfelelő feladat." : "Még nincsenek feladatok. Hozz létre egyet az Új feladat gombbal!";
    tr.appendChild(cell); tbody.appendChild(tr); return;
  }
  for (const task of tasks) {
    const tr = document.createElement("tr");
    const title = document.createElement("td");
    const heading = document.createElement("strong");
    heading.className = "taskname"; heading.textContent = task.title;
    const desc = document.createElement("span");
    desc.className = "desc"; desc.textContent = task.description || "Nincs megadva leírás";
    title.append(heading, desc);
    const priority = document.createElement("td");
    const priorityTag = document.createElement("span");
    priorityTag.className = "tag " + task.priority; priorityTag.textContent = priorityLabels[task.priority] || task.priority;
    priority.appendChild(priorityTag);
    const status = document.createElement("td");
    const statusTag = document.createElement("span");
    statusTag.className = "tag " + task.status; statusTag.textContent = statusLabels[task.status] || task.status;
    status.appendChild(statusTag);
    const date = document.createElement("td");
    date.textContent = new Date(task.createdAt).toLocaleDateString("hu-HU");
    const actions = document.createElement("td"); actions.className = "last";
    const controls = document.createElement("div"); controls.className = "row-actions";
    const edit = document.createElement("button"); edit.type = "button"; edit.className = "iconbutton";
    edit.textContent = "Szerkesztés"; edit.addEventListener("click", () => openEditor(task));
    const del = document.createElement("button"); del.type = "button"; del.className = "iconbutton delete";
    del.textContent = "Törlés"; del.addEventListener("click", () => removeTask(task));
    controls.append(edit, del); actions.appendChild(controls);
    tr.append(title, priority, status, date, actions); tbody.appendChild(tr);
  }
};
async function load() {
  try {
    state.tasks = await request("/api/tasks");
    render();
  } catch (error) { toast("Betöltési hiba: " + error.message); }
}
async function health() {
  try {
    const result = await request("/health");
    if (result.status !== "healthy") throw new Error("Unhealthy");
    $("health-dot").classList.add("ok"); $("health-text").textContent = "Rendszer elérhető";
  } catch {
    $("health-dot").classList.remove("ok"); $("health-text").textContent = "Kapcsolati hiba";
  }
}
function openEditor(task = null) {
  state.editing = task?.id ?? null;
  const form = $("task-form");
  form.reset();
  form.elements.title.value = task?.title || "";
  form.elements.description.value = task?.description || "";
  form.elements.priority.value = task?.priority || "medium";
  form.elements.status.value = task?.status || "todo";
  $("dialog-title").textContent = task ? "Feladat szerkesztése" : "Új feladat";
  $("editor").showModal();
  form.elements.title.focus();
}
async function save(event) {
  event.preventDefault();
  const form = $("task-form");
  const body = {
    title: form.elements.title.value,
    description: form.elements.description.value,
    priority: form.elements.priority.value,
    status: form.elements.status.value
  };
  $("save-button").disabled = true;
  try {
    await request(state.editing ? "/api/tasks/" + state.editing : "/api/tasks", {
      method: state.editing ? "PUT" : "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body)
    });
    $("editor").close();
    toast(state.editing ? "Feladat módosítva" : "Új feladat létrehozva");
    await load();
  } catch (error) { toast("Mentési hiba: " + error.message); }
  finally { $("save-button").disabled = false; }
}
async function removeTask(task) {
  if (!confirm('Biztosan törlöd ezt a feladatot?\n\n' + task.title)) return;
  try {
    await request("/api/tasks/" + task.id, { method: "DELETE" });
    toast("Feladat törölve");
    await load();
  } catch (error) { toast("Törlési hiba: " + error.message); }
}
$("new-button").addEventListener("click", () => openEditor());
$("close-dialog").addEventListener("click", () => $("editor").close());
$("cancel-dialog").addEventListener("click", () => $("editor").close());
$("task-form").addEventListener("submit", save);
$("search").addEventListener("input", render);
$("filter").addEventListener("change", render);
$("refresh").addEventListener("click", () => { health(); load(); });
health(); load();
