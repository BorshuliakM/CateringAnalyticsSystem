const state = {
    categories: [],
    dishes: [],
    diningTables: [],
    employees: [],
    orders: [],
    analytics: {},
    reportRows: [],
    activeView: "orders"
};

const tableStatuses = ["Free", "Occupied", "Reserved"];
const orderStatuses = ["New", "InProgress", "Completed", "Cancelled"];

const navItems = {
    admin: [
        { view: "dashboard", icon: "📊", label: "Аналітика" },
        { view: "reports", icon: "📄", label: "Звіти" },
        { view: "orders", icon: "🧾", label: "Замовлення" },
        { view: "menu", icon: "🍽", label: "Меню" },
        { view: "directories", icon: "👥", label: "Довідники" }
    ],
    waiter: [
        { view: "orders", icon: "🧾", label: "Замовлення" }
    ]
};

function getAuth() {
    try {
        return JSON.parse(localStorage.getItem("auth") || "null");
    } catch {
        return null;
    }
}

function getToken() {
    return getAuth()?.token || "";
}

function decodeJwt(token) {
    try {
        const payload = token.split(".")[1];
        const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
        return JSON.parse(decodeURIComponent(escape(atob(normalized))));
    } catch {
        return {};
    }
}

function getCurrentUser() {
    const auth = getAuth();
    if (!auth?.token) {
        return null;
    }

    const claims = decodeJwt(auth.token);
    return {
        token: auth.token,
        username: auth.username || claims.unique_name || claims.name || "",
        role: auth.role || claims.role || claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || "",
        employeeId: auth.employeeId ?? claims.employeeId ?? null,
        exp: claims.exp
    };
}

function getRole() {
    return getCurrentUser()?.role || "";
}

function getUsername() {
    return getCurrentUser()?.username || "";
}

function getEmployeeId() {
    return getCurrentUser()?.employeeId || null;
}

function isAdmin() {
    return getRole() === "Admin";
}

function isWaiter() {
    return getRole() === "Waiter";
}

function logout() {
    localStorage.removeItem("auth");
    window.location.href = "/login.html";
}

function requireAuth() {
    const user = getCurrentUser();
    if (!user?.token || (user.exp && Date.now() >= user.exp * 1000)) {
        logout();
        return false;
    }

    return true;
}

function requireAdmin() {
    if (!isAdmin()) {
        showMessage("Доступ заборонено", true);
        activateView("orders");
        return false;
    }

    return true;
}

if (!requireAuth()) {
    throw new Error("Authentication required.");
}

function authHeaders(extra = {}) {
    return {
        ...extra,
        Authorization: `Bearer ${getToken()}`
    };
}

const api = {
    async get(url) {
        return handleResponse(await fetch(url, { headers: authHeaders() }));
    },
    async send(url, method, body) {
        const response = await fetch(url, {
            method,
            headers: authHeaders({ "Content-Type": "application/json" }),
            body: JSON.stringify(body)
        });

        if (response.status === 204) {
            return null;
        }

        return handleResponse(response);
    },
    post(url, body) {
        return this.send(url, "POST", body);
    },
    put(url, body) {
        return this.send(url, "PUT", body);
    },
    patch(url, body) {
        return this.send(url, "PATCH", body);
    },
    async delete(url) {
        return handleResponse(await fetch(url, { method: "DELETE", headers: authHeaders() }));
    }
};

async function handleResponse(response) {
    if (response.status === 401) {
        logout();
        throw new Error("Потрібно увійти знову.");
    }

    const text = await response.text();
    let data = null;

    if (text) {
        try {
            data = JSON.parse(text);
        } catch {
            data = { message: text };
        }
    }

    if (!response.ok) {
        const error = new Error(data?.message || data?.title || (response.status === 403 ? "Доступ заборонено" : "Помилка запиту до API."));
        error.status = response.status;
        throw error;
    }

    return data;
}

function qs(selector) {
    return document.querySelector(selector);
}

function qsa(selector) {
    return [...document.querySelectorAll(selector)];
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "";
    }
}

function setValue(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.value = value ?? "";
    }
}

function setHtml(selector, value) {
    const element = qs(selector);
    if (element) {
        element.innerHTML = value ?? "";
    }
}

function showElement(id) {
    const element = document.getElementById(id);
    if (element) {
        element.hidden = false;
    }
}

function hideElement(id) {
    const element = document.getElementById(id);
    if (element) {
        element.hidden = true;
    }
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function money(value) {
    return `${Number(value || 0).toLocaleString("uk-UA", { maximumFractionDigits: 2 })} грн`;
}

function dateTime(value) {
    return new Date(value).toLocaleString("uk-UA", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
    });
}

function showMessage(text, isError = false) {
    const box = qs("#message");
    if (!box) {
        return;
    }

    box.textContent = text;
    box.classList.toggle("error", isError);
    box.hidden = false;
    setTimeout(() => {
        box.hidden = true;
    }, 4200);
}

function statusBadge(status) {
    const map = {
        Completed: "ok",
        InProgress: "warning",
        Cancelled: "danger",
        Free: "ok",
        Occupied: "warning",
        Reserved: ""
    };
    return `<span class="badge ${map[status] || ""}">${escapeHtml(status)}</span>`;
}

function addParam(params, key, value) {
    if (value !== undefined && value !== null && value !== "") {
        params.set(key, value);
    }
}

function buildSidebar() {
    const nav = qs("#mainNav");
    if (!nav) {
        return;
    }

    const items = isAdmin() ? navItems.admin : navItems.waiter;
    nav.innerHTML = "";
    for (const item of items) {
        const button = document.createElement("button");
        button.className = "nav-button";
        button.dataset.view = item.view;
        button.type = "button";
        button.title = item.label;
        button.innerHTML = `<span>${item.icon}</span> ${item.label}`;
        button.addEventListener("click", () => activateView(item.view));
        nav.appendChild(button);
    }
}

function setupAuthUi() {
    setText("currentUserInfo", `Увійшов як: ${getUsername()} (${getRole()})`);

    const actions = qs(".topbar-actions");
    if (actions && !document.getElementById("logoutBtn")) {
        const logoutButton = document.createElement("button");
        logoutButton.id = "logoutBtn";
        logoutButton.className = "secondary-button";
        logoutButton.type = "button";
        logoutButton.textContent = "Вийти";
        logoutButton.addEventListener("click", logout);
        actions.appendChild(logoutButton);
    }

    const employeeLabel = qs("#filterEmployee")?.closest("label");
    if (employeeLabel) {
        employeeLabel.hidden = isWaiter();
    }

    const orderEmployeeLabel = qs("#orderEmployee")?.closest("label");
    if (orderEmployeeLabel) {
        orderEmployeeLabel.hidden = isWaiter();
    }
}

function initialView() {
    const hashView = window.location.hash.replace("#", "");
    const allowed = isAdmin() ? navItems.admin.map(item => item.view) : ["orders"];
    return allowed.includes(hashView) ? hashView : (isAdmin() ? "dashboard" : "orders");
}

function activateView(viewId) {
    if (isWaiter() && viewId !== "orders") {
        showMessage("Доступ заборонено", true);
        viewId = "orders";
    }

    if (["dashboard", "reports", "menu", "directories"].includes(viewId) && !requireAdmin()) {
        return;
    }

    state.activeView = viewId;
    window.location.hash = viewId;
    qsa(".nav-button").forEach(item => item.classList.toggle("active", item.dataset.view === viewId));
    qsa(".view").forEach(item => item.classList.toggle("active", item.id === viewId));
    loadCurrentView();
}

async function loadCurrentView() {
    try {
        if (state.activeView === "orders") {
            await loadOrdersPage();
        } else if (state.activeView === "dashboard") {
            await loadAnalyticsPage();
        } else if (state.activeView === "reports") {
            await loadReportsPage();
        } else if (state.activeView === "menu") {
            await loadMenuPage();
        } else if (state.activeView === "directories") {
            await loadDirectoriesPage();
        }
    } catch (error) {
        handleUiError(error);
    }
}

function handleUiError(error) {
    if (error.status === 403) {
        showMessage("Доступ заборонено", true);
        if (isWaiter()) {
            activateView("orders");
        }
        return;
    }

    showMessage(error.message, true);
}

async function loadSharedOrderData() {
    const [categories, dishes, diningTables, orders] = await Promise.all([
        api.get("/api/categories"),
        api.get("/api/dishes"),
        api.get("/api/diningtables"),
        api.get("/api/orders")
    ]);

    state.categories = categories || [];
    state.dishes = dishes || [];
    state.diningTables = diningTables || [];
    state.orders = orders || [];

    if (isAdmin()) {
        state.employees = await api.get("/api/employees");
    } else {
        state.employees = [];
    }
}

async function loadOrdersPage() {
    await loadSharedOrderData();
    renderSelects();
    renderOrders();
    updateOrderTotal();
}

async function loadAnalyticsPage() {
    if (!requireAdmin()) {
        return;
    }

    await loadSharedOrderData();
    renderSelects();

    const analyticsQuery = getAnalyticsQuery();
    const [summary, popularDishes, salesByEmployee, ordersCountByDay, revenueByCategory, revenueByTable, tableOccupancy] = await Promise.all([
        api.get(`/api/analytics/summary?${analyticsQuery}`),
        api.get(`/api/analytics/popular-dishes?${analyticsQuery}`),
        api.get(`/api/analytics/sales-by-employee?${analyticsQuery}`),
        api.get("/api/analytics/orders-count-by-day"),
        api.get(`/api/analytics/revenue-by-category?${analyticsQuery}`),
        api.get(`/api/analytics/revenue-by-table?${analyticsQuery}`),
        api.get(`/api/analytics/table-occupancy?${analyticsQuery}`)
    ]);

    state.analytics = { summary, popularDishes, salesByEmployee, ordersCountByDay, revenueByCategory, revenueByTable, tableOccupancy };
    renderAnalytics();
}

async function loadReportsPage() {
    if (!requireAdmin()) {
        return;
    }

    await loadSharedOrderData();
    renderSelects();

    if (!qs("#reportFrom")?.value || !qs("#reportTo")?.value) {
        setDefaultReportDates();
    }

    await loadReportResults();
}

async function loadMenuPage() {
    if (!requireAdmin()) {
        return;
    }

    const [categories, dishes] = await Promise.all([
        api.get("/api/categories"),
        api.get("/api/dishes")
    ]);

    state.categories = categories || [];
    state.dishes = dishes || [];
    renderSelects();
    renderDishes();
}

async function loadDirectoriesPage() {
    if (!requireAdmin()) {
        return;
    }

    const [diningTables, employees, categories] = await Promise.all([
        api.get("/api/diningtables"),
        api.get("/api/employees"),
        api.get("/api/categories")
    ]);

    state.diningTables = diningTables || [];
    state.employees = employees || [];
    state.categories = categories || [];
    renderSelects();
    renderDirectories();
}

function getAnalyticsQuery() {
    const params = new URLSearchParams();
    addParam(params, "from", qs("#analyticsFrom")?.value);
    addParam(params, "to", qs("#analyticsTo")?.value);
    addParam(params, "employeeId", qs("#analyticsEmployee")?.value);
    addParam(params, "diningTableId", qs("#analyticsTable")?.value);
    return params.toString();
}

function buildReportQuery() {
    const params = new URLSearchParams();
    addParam(params, "from", qs("#reportFrom")?.value);
    addParam(params, "to", qs("#reportTo")?.value);
    addParam(params, "employeeId", qs("#reportEmployee")?.value);
    addParam(params, "diningTableId", qs("#reportTable")?.value);
    addParam(params, "categoryId", qs("#reportCategory")?.value);
    addParam(params, "dishId", qs("#reportDish")?.value);
    addParam(params, "status", qs("#reportStatus")?.value);
    addParam(params, "minTotal", qs("#reportMinTotal")?.value);
    addParam(params, "maxTotal", qs("#reportMaxTotal")?.value);
    return params;
}

function setDefaultReportDates() {
    const to = new Date();
    const from = new Date();
    from.setDate(to.getDate() - 30);
    setValue("reportFrom", from.toISOString().slice(0, 10));
    setValue("reportTo", to.toISOString().slice(0, 10));
}

function renderSelects() {
    fillSelect(qs("#orderTable"), state.diningTables.filter(table => table.status !== "Occupied"), "Оберіть столик", table => `Столик ${table.number} (${table.seatsCount} місць, ${table.status})`);

    if (isAdmin()) {
        fillSelect(qs("#orderEmployee"), state.employees, "Оберіть працівника", item => `${item.fullName} (${item.position || "працівник"})`);
    } else {
        const employeeSelect = qs("#orderEmployee");
        if (employeeSelect) {
            employeeSelect.innerHTML = `<option value="${escapeHtml(getEmployeeId() || "")}">${escapeHtml(getUsername())}</option>`;
            employeeSelect.value = getEmployeeId() || "";
        }
    }

    fillSelect(qs("#dishCategory"), state.categories, "Оберіть категорію", item => item.name);
    fillSelect(qs("#filterTable"), state.diningTables, "Усі", table => `Столик ${table.number}`, true);
    fillSelect(qs("#filterEmployee"), state.employees, "Усі", item => item.fullName, true);
    fillSelect(qs("#analyticsTable"), state.diningTables, "Усі", table => `Столик ${table.number}`, true);
    fillSelect(qs("#analyticsEmployee"), state.employees, "Усі", item => item.fullName, true);
    fillSelect(qs("#reportTable"), state.diningTables, "Усі", table => `Столик ${table.number}`, true);
    fillSelect(qs("#reportEmployee"), state.employees, "Усі", item => item.fullName, true);
    fillSelect(qs("#reportCategory"), state.categories, "Усі", item => item.name, true);
    fillSelect(qs("#reportDish"), state.dishes, "Усі", item => item.name, true);

    qsa(".dish-select").forEach(select => {
        const current = select.value;
        fillSelect(select, state.dishes.filter(dish => dish.isAvailable), "Оберіть страву", dish => `${dish.name} - ${money(dish.price)}`);
        select.value = current;
    });
}

function fillSelect(select, items, placeholder, labelFactory, allowEmpty = false) {
    if (!select) {
        return;
    }

    const current = select.value;
    select.innerHTML = "";
    select.insertAdjacentHTML("beforeend", `<option value="">${escapeHtml(placeholder)}</option>`);

    for (const item of items || []) {
        const option = document.createElement("option");
        option.value = item.id;
        option.textContent = labelFactory(item);
        select.appendChild(option);
    }

    if (!allowEmpty && items?.length > 0 && !current) {
        select.value = items[0].id;
        return;
    }

    select.value = current;
}

function renderAnalytics() {
    const summary = state.analytics.summary || {};
    setText("totalOrders", summary.totalOrders || 0);
    setText("completedOrders", summary.completedOrders || 0);
    setText("cancelledOrders", summary.cancelledOrders || 0);
    setText("activeOrders", summary.activeOrders || 0);
    setText("totalSales", money(summary.totalRevenue));
    setText("averageCheck", money(summary.averageCheck));

    renderBars("#employeeSalesChart", state.analytics.salesByEmployee || [], item => item.employeeName, item => item.totalRevenue, money);
    renderBars("#categoryRevenueChart", state.analytics.revenueByCategory || [], item => item.categoryName, item => item.totalRevenue, money);
    renderBars("#ordersByDayChart", state.analytics.ordersCountByDay || [], item => new Date(item.date).toLocaleDateString("uk-UA"), item => item.ordersCount, value => `${value} зам.`);
    renderBars("#tableRevenueChart", state.analytics.revenueByTable || [], item => `Столик ${item.tableNumber}`, item => item.totalRevenue, money);
    renderTableOccupancy();

    const body = qs("#popularDishesBody");
    if (!body) {
        return;
    }

    body.innerHTML = "";
    for (const dish of state.analytics.popularDishes || []) {
        body.insertAdjacentHTML("beforeend", `
            <tr>
                <td>${escapeHtml(dish.dishName)}</td>
                <td>${dish.quantitySold}</td>
                <td>${money(dish.totalRevenue)}</td>
            </tr>
        `);
    }

    if (!body.children.length) {
        body.innerHTML = `<tr><td colspan="3"><div class="empty">Немає даних для відображення</div></td></tr>`;
    }
}

function renderBars(selector, items, label, value, valueLabel) {
    const container = qs(selector);
    if (!container) {
        return;
    }

    container.innerHTML = "";
    const max = Math.max(...(items || []).map(value), 0);

    for (const item of items || []) {
        const rawValue = value(item);
        const width = max === 0 ? 0 : Math.max(6, Math.round(rawValue / max * 100));
        container.insertAdjacentHTML("beforeend", `
            <div class="bar-row">
                <strong>${escapeHtml(label(item))}</strong>
                <div class="bar-track"><div class="bar-fill" style="width:${width}%"></div></div>
                <span class="bar-value">${escapeHtml(valueLabel(rawValue))}</span>
            </div>
        `);
    }

    if (!items?.length) {
        container.innerHTML = `<div class="empty">Немає даних для відображення</div>`;
    }
}

function renderOrders() {
    const body = qs("#ordersBody");
    if (!body) {
        return;
    }

    body.innerHTML = "";

    for (const order of state.orders || []) {
        body.insertAdjacentHTML("beforeend", `
            <tr>
                <td>#${order.id}</td>
                <td>${dateTime(order.orderDate)}</td>
                <td>Столик ${order.diningTableNumber}</td>
                <td>${escapeHtml(order.employeeName)}</td>
                <td>${money(order.totalAmount)}</td>
                <td>${statusBadge(order.status)}</td>
                <td>
                    <div class="row-actions">
                        <select class="status-select" data-id="${order.id}">
                            ${orderStatuses.map(status => `<option ${status === order.status ? "selected" : ""}>${status}</option>`).join("")}
                        </select>
                        ${isAdmin() ? `<button class="danger-button" data-delete-order="${order.id}" type="button">Видалити</button>` : ""}
                    </div>
                </td>
            </tr>
            <tr>
                <td></td>
                <td colspan="6">${(order.items || []).map(item => `${escapeHtml(item.dishName)} x${item.quantity}`).join(", ")}</td>
            </tr>
        `);
    }

    if (!body.children.length) {
        body.innerHTML = `<tr><td colspan="7"><div class="empty">Замовлення не знайдені</div></td></tr>`;
    }
}

function renderDishes() {
    const body = qs("#dishesBody");
    if (!body) {
        return;
    }

    body.innerHTML = "";

    for (const dish of state.dishes || []) {
        const category = state.categories.find(item => item.id === dish.categoryId);
        body.insertAdjacentHTML("beforeend", `
            <tr>
                <td>
                    <strong>${escapeHtml(dish.name)}</strong>
                    <span class="muted-line">${escapeHtml(dish.description || "")}</span>
                </td>
                <td>${money(dish.price)}</td>
                <td>${escapeHtml(category?.name || "-")}</td>
                <td>${dish.isAvailable ? '<span class="badge ok">Доступна</span>' : '<span class="badge danger">Недоступна</span>'}</td>
                <td>
                    <div class="row-actions">
                        <button class="secondary-button" data-edit-dish="${dish.id}" type="button">Редагувати</button>
                        <button class="danger-button" data-delete-dish="${dish.id}" type="button">Видалити</button>
                    </div>
                </td>
            </tr>
        `);
    }

    if (!body.children.length) {
        body.innerHTML = `<tr><td colspan="5"><div class="empty">Страви не знайдені</div></td></tr>`;
    }
}

function renderDirectories() {
    renderDiningTablesList();
    renderEntityList("#employeesList", state.employees, item => item.fullName, item => [item.position, item.phone, item.email].filter(Boolean).join(" | "), "employee");
    renderEntityList("#categoriesList", state.categories, item => item.name, item => item.description, "category");
}

function renderDiningTablesList() {
    const list = qs("#tablesList");
    if (!list) {
        return;
    }

    list.innerHTML = "";

    for (const table of state.diningTables || []) {
        list.insertAdjacentHTML("beforeend", `
            <article class="entity-card">
                <div>
                    <strong>Столик ${table.number}</strong>
                    <span>${table.seatsCount} місць</span>
                </div>
                <div class="row-actions">
                    <select class="table-status-select" data-id="${table.id}" aria-label="Статус столика ${table.number}">
                        ${tableStatuses.map(status => `<option ${status === table.status ? "selected" : ""}>${status}</option>`).join("")}
                    </select>
                    <button class="danger-button" data-delete-table="${table.id}" type="button">×</button>
                </div>
            </article>
        `);
    }

    if (!list.children.length) {
        list.innerHTML = `<div class="empty">Записи відсутні</div>`;
    }
}

function renderTableOccupancy() {
    const list = qs("#tableOccupancyList");
    if (!list) {
        return;
    }

    list.innerHTML = "";

    for (const item of state.analytics.tableOccupancy || []) {
        list.insertAdjacentHTML("beforeend", `
            <article class="entity-card">
                <div>
                    <strong>Столик ${item.tableNumber}</strong>
                    <span>${item.ordersCount} замовлень</span>
                </div>
                ${statusBadge(item.currentStatus)}
            </article>
        `);
    }

    if (!list.children.length) {
        list.innerHTML = `<div class="empty">Немає даних для відображення</div>`;
    }
}

function renderEntityList(selector, items, title, subtitle, type) {
    const list = qs(selector);
    if (!list) {
        return;
    }

    list.innerHTML = "";

    for (const item of items || []) {
        list.insertAdjacentHTML("beforeend", `
            <article class="entity-card">
                <div>
                    <strong>${escapeHtml(title(item))}</strong>
                    <span>${escapeHtml(subtitle(item) || "Без додаткових даних")}</span>
                </div>
                <button class="danger-button" data-delete-${type}="${item.id}" type="button">×</button>
            </article>
        `);
    }

    if (!items?.length) {
        list.innerHTML = `<div class="empty">Записи відсутні</div>`;
    }
}

function addOrderItemRow(dishId = "", quantity = 1) {
    const container = qs("#orderItems");
    if (!container) {
        return;
    }

    const row = document.createElement("div");
    row.className = "order-item-row";
    row.innerHTML = `
        <label>
            Страва
            <select class="dish-select" required></select>
        </label>
        <label>
            К-сть
            <input class="quantity-input" type="number" min="1" value="${quantity}" required>
        </label>
        <button class="icon-button remove-item" type="button" title="Видалити позицію">×</button>
    `;
    container.appendChild(row);
    const select = row.querySelector(".dish-select");
    fillSelect(select, state.dishes.filter(dish => dish.isAvailable), "Оберіть страву", dish => `${dish.name} - ${money(dish.price)}`);
    select.value = dishId;
    updateOrderTotal();
}

function updateOrderTotal() {
    let total = 0;
    qsa(".order-item-row").forEach(row => {
        const dishId = Number(row.querySelector(".dish-select")?.value);
        const quantity = Number(row.querySelector(".quantity-input")?.value || 0);
        const dish = state.dishes.find(item => item.id === dishId);
        if (dish && quantity > 0) {
            total += dish.price * quantity;
        }
    });
    setText("orderPreviewTotal", money(total));
}

function resetDishForm() {
    qs("#dishForm")?.reset();
    setValue("dishId", "");
    const available = qs("#dishAvailable");
    if (available) {
        available.checked = true;
    }
    setText("dishFormTitle", "Нова страва");
}

function setupEvents() {
    qs("#refreshBtn")?.addEventListener("click", loadCurrentView);
    qs("#addOrderItemBtn")?.addEventListener("click", () => addOrderItemRow());
    qs("#cancelDishEdit")?.addEventListener("click", resetDishForm);

    document.addEventListener("input", event => {
        if (event.target.matches(".dish-select, .quantity-input")) {
            updateOrderTotal();
        }
    });

    document.addEventListener("click", async event => {
        const target = event.target;

        if (target.matches(".remove-item")) {
            target.closest(".order-item-row")?.remove();
            updateOrderTotal();
        }

        if (target.dataset.editDish) {
            const dish = state.dishes.find(item => item.id === Number(target.dataset.editDish));
            if (dish) {
                setValue("dishId", dish.id);
                setValue("dishName", dish.name);
                setValue("dishDescription", dish.description || "");
                setValue("dishPrice", dish.price);
                setValue("dishCategory", dish.categoryId);
                const available = qs("#dishAvailable");
                if (available) {
                    available.checked = dish.isAvailable;
                }
                setText("dishFormTitle", "Редагування страви");
            }
        }

        await handleDeleteClick(target);
    });

    document.addEventListener("change", async event => {
        if (event.target.matches(".status-select")) {
            try {
                await api.patch(`/api/orders/${event.target.dataset.id}/status`, { status: event.target.value });
                showMessage("Статус замовлення змінено.");
                await loadOrdersPage();
            } catch (error) {
                handleUiError(error);
                await loadCurrentView();
            }
        }

        if (event.target.matches(".table-status-select")) {
            try {
                await api.patch(`/api/diningtables/${event.target.dataset.id}/status`, { status: event.target.value });
                showMessage("Статус столика змінено.");
                await loadDirectoriesPage();
            } catch (error) {
                handleUiError(error);
                await loadCurrentView();
            }
        }
    });

    qs("#orderForm")?.addEventListener("submit", submitOrder);
    qs("#dishForm")?.addEventListener("submit", submitDish);
    qs("#analyticsFilters")?.addEventListener("submit", event => {
        event.preventDefault();
        loadAnalyticsPage();
    });
    qs("#resetAnalyticsFilters")?.addEventListener("click", () => {
        qs("#analyticsFilters")?.reset();
        loadAnalyticsPage();
    });
    qs("#downloadOrdersCsv")?.addEventListener("click", () => downloadReportCsv("orders", getAnalyticsQuery()));
    qs("#reportsFilters")?.addEventListener("submit", event => {
        event.preventDefault();
        loadReportResults();
    });
    qs("#resetReportsFilters")?.addEventListener("click", () => {
        qs("#reportsFilters")?.reset();
        setDefaultReportDates();
        loadReportResults();
    });
    qs("#downloadReportCsv")?.addEventListener("click", downloadCurrentReportCsv);
    qs("#tableForm")?.addEventListener("submit", event => submitDirectory(event, "/api/diningtables", {
        number: Number(qs("#tableNumber")?.value),
        seatsCount: Number(qs("#tableSeats")?.value),
        status: qs("#tableStatus")?.value
    }));
    qs("#employeeForm")?.addEventListener("submit", event => submitDirectory(event, "/api/employees", {
        fullName: qs("#employeeName")?.value,
        position: qs("#employeePosition")?.value,
        phone: qs("#employeePhone")?.value,
        email: qs("#employeeEmail")?.value
    }));
    qs("#categoryForm")?.addEventListener("submit", event => submitDirectory(event, "/api/categories", {
        name: qs("#categoryName")?.value,
        description: qs("#categoryDescription")?.value
    }));
    qs("#orderFilters")?.addEventListener("submit", submitFilters);
}

async function submitOrder(event) {
    event.preventDefault();
    const items = qsa(".order-item-row").map(row => ({
        dishId: Number(row.querySelector(".dish-select")?.value),
        quantity: Number(row.querySelector(".quantity-input")?.value)
    })).filter(item => item.dishId && item.quantity > 0);

    const payload = {
        diningTableId: Number(qs("#orderTable")?.value),
        items
    };

    if (isAdmin()) {
        payload.employeeId = Number(qs("#orderEmployee")?.value);
    }

    try {
        await api.post("/api/orders", payload);
        qs("#orderForm")?.reset();
        setHtml("#orderItems", "");
        addOrderItemRow();
        showMessage("Замовлення створено.");
        await loadOrdersPage();
    } catch (error) {
        handleUiError(error);
    }
}

async function submitDish(event) {
    event.preventDefault();
    const id = qs("#dishId")?.value;
    const payload = {
        name: qs("#dishName")?.value,
        description: qs("#dishDescription")?.value,
        price: Number(qs("#dishPrice")?.value),
        categoryId: Number(qs("#dishCategory")?.value),
        isAvailable: qs("#dishAvailable")?.checked || false
    };

    try {
        if (id) {
            await api.put(`/api/dishes/${id}`, payload);
            showMessage("Страву оновлено.");
        } else {
            await api.post("/api/dishes", payload);
            showMessage("Страву додано.");
        }

        resetDishForm();
        await loadMenuPage();
    } catch (error) {
        handleUiError(error);
    }
}

async function submitDirectory(event, url, payload) {
    event.preventDefault();

    try {
        await api.post(url, payload);
        event.target.reset();
        showMessage("Запис додано.");
        await loadDirectoriesPage();
    } catch (error) {
        handleUiError(error);
    }
}

async function submitFilters(event) {
    event.preventDefault();
    const params = new URLSearchParams();
    addParam(params, "from", qs("#filterFrom")?.value);
    addParam(params, "to", qs("#filterTo")?.value);
    addParam(params, "diningTableId", qs("#filterTable")?.value);
    if (isAdmin()) {
        addParam(params, "employeeId", qs("#filterEmployee")?.value);
    }
    addParam(params, "status", qs("#filterStatus")?.value);

    try {
        state.orders = await api.get(`/api/orders?${params.toString()}`);
        renderOrders();
    } catch (error) {
        handleUiError(error);
    }
}

async function handleDeleteClick(target) {
    if (!isAdmin()) {
        return;
    }

    const deletes = [
        ["deleteDish", "/api/dishes/"],
        ["deleteOrder", "/api/orders/"],
        ["deleteTable", "/api/diningtables/"],
        ["deleteEmployee", "/api/employees/"],
        ["deleteCategory", "/api/categories/"]
    ];

    for (const [key, url] of deletes) {
        if (target.dataset[key]) {
            try {
                await api.delete(`${url}${target.dataset[key]}`);
                showMessage("Запис видалено.");
                await loadCurrentView();
            } catch (error) {
                handleUiError(error);
            }
        }
    }
}

async function loadReportResults() {
    if (!requireAdmin()) {
        return;
    }

    const type = qs("#reportType")?.value || "orders";
    const params = buildReportQuery();
    if (!params.get("from") || !params.get("to")) {
        showReportMessage("Оберіть період звіту.");
        return;
    }

    try {
        const data = await api.get(`/api/reports/${type}?${params.toString()}`);
        const rows = Array.isArray(data) ? data : [data];
        state.reportRows = rows;
        renderReportTable(type, rows);
    } catch (error) {
        handleUiError(error);
        showReportMessage(error.message || "Помилка завантаження звіту.");
    }
}

function renderReportTable(type, rows) {
    const columns = reportColumns(type);
    const head = qs("#reportHead");
    const body = qs("#reportBody");
    if (!head || !body) {
        return;
    }

    setText("reportResultTitle", reportTitle(type));
    head.innerHTML = `<tr>${columns.map(column => `<th>${escapeHtml(column.title)}</th>`).join("")}</tr>`;
    body.innerHTML = "";

    if (!rows?.length) {
        showReportMessage("Немає даних для відображення");
        return;
    }

    hideElement("reportMessage");
    for (const row of rows) {
        body.insertAdjacentHTML("beforeend", `<tr>${columns.map(column => `<td>${escapeHtml(column.value(row))}</td>`).join("")}</tr>`);
    }
}

function showReportMessage(message) {
    setText("reportMessage", message);
    showElement("reportMessage");
    setHtml("#reportHead", "");
    setHtml("#reportBody", "");
}

function reportTitle(type) {
    return {
        orders: "Звіт по замовленнях",
        sales: "Звіт продажів",
        dishes: "Звіт по стравах",
        employees: "Звіт по працівниках",
        tables: "Звіт по столиках",
        daily: "Денний звіт"
    }[type] || "Результати звіту";
}

function reportColumns(type) {
    const columns = {
        orders: [
            ["ID", row => row.orderId],
            ["Дата", row => dateTime(row.orderDate)],
            ["Столик", row => row.diningTableNumber],
            ["Працівник", row => row.employeeName],
            ["Статус", row => row.status],
            ["Сума", row => money(row.totalAmount)],
            ["Позиції", row => row.itemsCount]
        ],
        sales: [
            ["Усього", row => row.totalOrders],
            ["Completed", row => row.completedOrders],
            ["Cancelled", row => row.cancelledOrders],
            ["Active", row => row.activeOrders],
            ["Виручка", row => money(row.totalRevenue)],
            ["Середній чек", row => money(row.averageCheck)]
        ],
        dishes: [
            ["ID", row => row.dishId],
            ["Страва", row => row.dishName],
            ["Категорія", row => row.categoryName],
            ["Кількість", row => row.quantitySold],
            ["Виручка", row => money(row.totalRevenue)]
        ],
        employees: [
            ["ID", row => row.employeeId],
            ["Працівник", row => row.employeeName],
            ["Замовлення", row => row.ordersCount],
            ["Виручка", row => money(row.totalRevenue)],
            ["Середній чек", row => money(row.averageCheck)]
        ],
        tables: [
            ["ID", row => row.diningTableId],
            ["Столик", row => row.tableNumber],
            ["Замовлення", row => row.ordersCount],
            ["Виручка", row => money(row.totalRevenue)],
            ["Середній чек", row => money(row.averageCheck)],
            ["Статус", row => row.currentStatus]
        ],
        daily: [
            ["Дата", row => new Date(row.date).toLocaleDateString("uk-UA")],
            ["Замовлення", row => row.ordersCount],
            ["Completed", row => row.completedOrders],
            ["Cancelled", row => row.cancelledOrders],
            ["Виручка", row => money(row.totalRevenue)],
            ["Середній чек", row => money(row.averageCheck)]
        ]
    }[type] || [];

    return columns.map(([title, value]) => ({ title, value }));
}

async function downloadCurrentReportCsv() {
    const type = qs("#reportType")?.value || "orders";
    if (type === "daily") {
        showMessage("CSV export для денного звіту не підтримується API.", true);
        return;
    }

    const params = buildReportQuery();
    params.set("format", "csv");
    await downloadReportCsv(type, params.toString());
}

async function downloadReportCsv(type, queryString) {
    if (!requireAdmin()) {
        return;
    }

    const params = new URLSearchParams(queryString);
    if (!params.get("from") || !params.get("to")) {
        showMessage("Оберіть період перед завантаженням CSV.", true);
        return;
    }
    params.set("format", "csv");

    try {
        const response = await fetch(`/api/reports/${type}/export?${params.toString()}`, {
            headers: authHeaders()
        });

        if (response.status === 401) {
            logout();
            return;
        }

        if (response.status === 403) {
            showMessage("Доступ заборонено", true);
            return;
        }

        if (!response.ok) {
            throw new Error("Не вдалося завантажити CSV.");
        }

        const blob = await response.blob();
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = fileNameFromDisposition(response.headers.get("Content-Disposition")) || `${type}_report.csv`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
    } catch (error) {
        handleUiError(error);
    }
}

function fileNameFromDisposition(disposition) {
    if (!disposition) {
        return "";
    }

    const utfMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);
    if (utfMatch) {
        return decodeURIComponent(utfMatch[1]);
    }

    const match = disposition.match(/filename="?([^"]+)"?/i);
    return match?.[1] || "";
}

function bootstrap() {
    buildSidebar();
    setupAuthUi();
    setupEvents();
    setDefaultReportDates();
    window.addEventListener("hashchange", () => {
        const nextView = initialView();
        if (nextView !== state.activeView) {
            activateView(nextView);
        } else if (isWaiter() && window.location.hash !== "#orders") {
            window.location.hash = "orders";
        }
    });
    activateView(initialView());
    if (!qs("#orderItems")?.children.length) {
        addOrderItemRow();
    }
}

bootstrap();
