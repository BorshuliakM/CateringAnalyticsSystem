const state = {
    categories: [],
    dishes: [],
    diningTables: [],
    employees: [],
    orders: [],
    analytics: {}
};

const auth = JSON.parse(localStorage.getItem("auth") || "null");

if (!auth?.token) {
    window.location.href = "/login.html";
}

function authHeaders(extra = {}) {
    return {
        ...extra,
        Authorization: `Bearer ${auth.token}`
    };
}

function isAdmin() {
    return auth?.role === "Admin";
}

const tableStatuses = ["Free", "Occupied", "Reserved"];
const orderStatuses = ["New", "InProgress", "Completed", "Cancelled"];

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
        const response = await fetch(url, { method: "DELETE", headers: authHeaders() });
        if (!response.ok) {
            throw new Error("Не вдалося видалити запис.");
        }
    }
};

async function handleResponse(response) {
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
        throw new Error(data?.message || data?.title || "Помилка запиту до API.");
    }

    return data;
}

function qs(selector) {
    return document.querySelector(selector);
}

function qsa(selector) {
    return [...document.querySelectorAll(selector)];
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
    return `<span class="badge ${map[status] || ""}">${status}</span>`;
}

async function loadData() {
    try {
        const [categories, dishes, diningTables, orders] = await Promise.all([
            api.get("/api/categories"),
            api.get("/api/dishes"),
            api.get("/api/diningtables"),
            api.get("/api/orders")
        ]);

        const employees = isAdmin() ? await api.get("/api/employees") : [];
        let analytics = {};

        if (isAdmin()) {
            const analyticsQuery = getAnalyticsQuery();
            const [
                summary,
                popularDishes,
                salesByEmployee,
                ordersCountByDay,
                revenueByCategory,
                revenueByTable,
                tableOccupancy
            ] = await Promise.all([
                api.get(`/api/analytics/summary?${analyticsQuery}`),
                api.get(`/api/analytics/popular-dishes?${analyticsQuery}`),
                api.get(`/api/analytics/sales-by-employee?${analyticsQuery}`),
                api.get("/api/analytics/orders-count-by-day"),
                api.get(`/api/analytics/revenue-by-category?${analyticsQuery}`),
                api.get(`/api/analytics/revenue-by-table?${analyticsQuery}`),
                api.get(`/api/analytics/table-occupancy?${analyticsQuery}`)
            ]);

            analytics = { summary, popularDishes, salesByEmployee, ordersCountByDay, revenueByCategory, revenueByTable, tableOccupancy };
        }

        Object.assign(state, {
            categories,
            dishes,
            diningTables,
            employees,
            orders,
            analytics
        });

        renderAll();
    } catch (error) {
        showMessage(error.message, true);
    }
}

function getAnalyticsQuery() {
    const params = new URLSearchParams();
    addParam(params, "from", qs("#analyticsFrom")?.value);
    addParam(params, "to", qs("#analyticsTo")?.value);
    addParam(params, "employeeId", qs("#analyticsEmployee")?.value);
    addParam(params, "diningTableId", qs("#analyticsTable")?.value);
    return params.toString();
}

function renderAll() {
    renderSelects();
    renderAnalytics();
    renderOrders();
    renderDishes();
    renderDirectories();
    updateOrderTotal();
}

function renderSelects() {
    fillSelect(qs("#orderTable"), state.diningTables.filter(table => table.status !== "Occupied"), "Оберіть столик", table => `Столик ${table.number} (${table.seatsCount} місць, ${table.status})`);
    if (isAdmin()) {
        fillSelect(qs("#orderEmployee"), state.employees, "Оберіть працівника", item => `${item.fullName} (${item.position || "працівник"})`);
    } else {
        const employeeSelect = qs("#orderEmployee");
        employeeSelect.innerHTML = `<option value="${auth.employeeId || ""}">${auth.username}</option>`;
        employeeSelect.value = auth.employeeId || "";
    }
    fillSelect(qs("#dishCategory"), state.categories, "Оберіть категорію", item => item.name);
    fillSelect(qs("#filterTable"), state.diningTables, "Усі", table => `Столик ${table.number}`, true);
    fillSelect(qs("#filterEmployee"), state.employees, "Усі", item => item.fullName, true);
    fillSelect(qs("#analyticsTable"), state.diningTables, "All", table => `Table ${table.number}`, true);
    fillSelect(qs("#analyticsEmployee"), state.employees, "All", item => item.fullName, true);

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
    select.insertAdjacentHTML("beforeend", `<option value="">${placeholder}</option>`);

    for (const item of items) {
        const option = document.createElement("option");
        option.value = item.id;
        option.textContent = labelFactory(item);
        select.appendChild(option);
    }

    if (!allowEmpty && items.length > 0 && !current) {
        select.value = items[0].id;
        return;
    }

    select.value = current;
}

function renderAnalytics() {
    const summary = state.analytics.summary || {};
    qs("#totalOrders").textContent = summary.totalOrders || 0;
    qs("#completedOrders").textContent = summary.completedOrders || 0;
    qs("#cancelledOrders").textContent = summary.cancelledOrders || 0;
    qs("#activeOrders").textContent = summary.activeOrders || 0;
    qs("#totalSales").textContent = money(summary.totalRevenue);
    qs("#averageCheck").textContent = money(summary.averageCheck);

    renderBars("#employeeSalesChart", state.analytics.salesByEmployee || [], item => item.employeeName, item => item.totalRevenue, money);
    renderBars("#categoryRevenueChart", state.analytics.revenueByCategory || [], item => item.categoryName, item => item.totalRevenue, money);
    renderBars("#ordersByDayChart", state.analytics.ordersCountByDay || [], item => new Date(item.date).toLocaleDateString("uk-UA"), item => item.ordersCount, value => `${value} зам.`);
    renderBars("#tableRevenueChart", state.analytics.revenueByTable || [], item => `Столик ${item.tableNumber}`, item => item.totalRevenue, money);
    renderTableOccupancy();

    const body = qs("#popularDishesBody");
    body.innerHTML = "";
    for (const dish of state.analytics.popularDishes || []) {
        body.insertAdjacentHTML("beforeend", `
            <tr>
                <td>${dish.dishName}</td>
                <td>${dish.quantitySold}</td>
                <td>${money(dish.totalRevenue)}</td>
            </tr>
        `);
    }

    if (!body.children.length) {
        body.innerHTML = `<tr><td colspan="3"><div class="empty">Дані відсутні</div></td></tr>`;
    }
}

function renderBars(selector, items, label, value, valueLabel) {
    const container = qs(selector);
    container.innerHTML = "";
    const max = Math.max(...items.map(value), 0);

    for (const item of items) {
        const rawValue = value(item);
        const width = max === 0 ? 0 : Math.max(6, Math.round(rawValue / max * 100));
        container.insertAdjacentHTML("beforeend", `
            <div class="bar-row">
                <strong>${label(item)}</strong>
                <div class="bar-track"><div class="bar-fill" style="width:${width}%"></div></div>
                <span class="bar-value">${valueLabel(rawValue)}</span>
            </div>
        `);
    }

    if (!items.length) {
        container.innerHTML = `<div class="empty">Дані відсутні</div>`;
    }
}

function renderOrders() {
    const body = qs("#ordersBody");
    body.innerHTML = "";

    for (const order of state.orders) {
        body.insertAdjacentHTML("beforeend", `
            <tr>
                <td>#${order.id}</td>
                <td>${dateTime(order.orderDate)}</td>
                <td>Столик ${order.diningTableNumber}</td>
                <td>${order.employeeName}</td>
                <td>${money(order.totalAmount)}</td>
                <td>${statusBadge(order.status)}</td>
                <td>
                    <div class="row-actions">
                        <select class="status-select" data-id="${order.id}">
                            ${orderStatuses.map(status => `<option ${status === order.status ? "selected" : ""}>${status}</option>`).join("")}
                        </select>
                        <button class="danger-button" data-delete-order="${order.id}" type="button">Видалити</button>
                    </div>
                </td>
            </tr>
            <tr>
                <td></td>
                <td colspan="6">${order.items.map(item => `${item.dishName} x${item.quantity}`).join(", ")}</td>
            </tr>
        `);
    }

    if (!body.children.length) {
        body.innerHTML = `<tr><td colspan="7"><div class="empty">Замовлення не знайдені</div></td></tr>`;
    }
}

function renderDishes() {
    const body = qs("#dishesBody");
    body.innerHTML = "";

    for (const dish of state.dishes) {
        const category = state.categories.find(item => item.id === dish.categoryId);
        body.insertAdjacentHTML("beforeend", `
            <tr>
                <td>
                    <strong>${dish.name}</strong>
                    <span class="muted-line">${dish.description || ""}</span>
                </td>
                <td>${money(dish.price)}</td>
                <td>${category?.name || "-"}</td>
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
    list.innerHTML = "";

    for (const table of state.diningTables) {
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
        list.innerHTML = `<div class="empty">Дані відсутні</div>`;
    }
}

function renderEntityList(selector, items, title, subtitle, type) {
    const list = qs(selector);
    list.innerHTML = "";

    for (const item of items) {
        list.insertAdjacentHTML("beforeend", `
            <article class="entity-card">
                <div>
                    <strong>${title(item)}</strong>
                    <span>${subtitle(item) || "Без додаткових даних"}</span>
                </div>
                <button class="danger-button" data-delete-${type}="${item.id}" type="button">×</button>
            </article>
        `);
    }

    if (!items.length) {
        list.innerHTML = `<div class="empty">Записи відсутні</div>`;
    }
}

function addOrderItemRow(dishId = "", quantity = 1) {
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
    qs("#orderItems").appendChild(row);
    const select = row.querySelector(".dish-select");
    fillSelect(select, state.dishes.filter(dish => dish.isAvailable), "Оберіть страву", dish => `${dish.name} - ${money(dish.price)}`);
    select.value = dishId;
    updateOrderTotal();
}

function updateOrderTotal() {
    let total = 0;
    qsa(".order-item-row").forEach(row => {
        const dishId = Number(row.querySelector(".dish-select").value);
        const quantity = Number(row.querySelector(".quantity-input").value || 0);
        const dish = state.dishes.find(item => item.id === dishId);
        if (dish && quantity > 0) {
            total += dish.price * quantity;
        }
    });
    qs("#orderPreviewTotal").textContent = money(total);
}

function resetDishForm() {
    qs("#dishForm").reset();
    qs("#dishId").value = "";
    qs("#dishAvailable").checked = true;
    qs("#dishFormTitle").textContent = "Нова страва";
}

function setupEvents() {
    qsa(".nav-button").forEach(button => {
        button.addEventListener("click", () => activateView(button.dataset.view));
    });

    qs("#refreshBtn").addEventListener("click", loadData);
    qs("#addOrderItemBtn").addEventListener("click", () => addOrderItemRow());
    qs("#cancelDishEdit").addEventListener("click", resetDishForm);

    document.addEventListener("input", event => {
        if (event.target.matches(".dish-select, .quantity-input")) {
            updateOrderTotal();
        }
    });

    document.addEventListener("click", async event => {
        const target = event.target;

        if (target.matches(".remove-item")) {
            target.closest(".order-item-row").remove();
            updateOrderTotal();
        }

        if (target.dataset.editDish) {
            const dish = state.dishes.find(item => item.id === Number(target.dataset.editDish));
            if (dish) {
                qs("#dishId").value = dish.id;
                qs("#dishName").value = dish.name;
                qs("#dishDescription").value = dish.description || "";
                qs("#dishPrice").value = dish.price;
                qs("#dishCategory").value = dish.categoryId;
                qs("#dishAvailable").checked = dish.isAvailable;
                qs("#dishFormTitle").textContent = "Редагування страви";
            }
        }

        await handleDeleteClick(target);
    });

    document.addEventListener("change", async event => {
        if (event.target.matches(".status-select")) {
            try {
                await api.patch(`/api/orders/${event.target.dataset.id}/status`, { status: event.target.value });
                showMessage("Статус замовлення змінено.");
                await refreshOrdersAndAnalytics();
            } catch (error) {
                showMessage(error.message, true);
                await loadData();
            }
        }

        if (event.target.matches(".table-status-select")) {
            try {
                await api.patch(`/api/diningtables/${event.target.dataset.id}/status`, { status: event.target.value });
                showMessage("Статус столика змінено.");
                await refreshTablesAndAnalytics();
            } catch (error) {
                showMessage(error.message, true);
                await loadData();
            }
        }
    });

    qs("#orderForm").addEventListener("submit", submitOrder);
    qs("#dishForm").addEventListener("submit", submitDish);
    qs("#analyticsFilters")?.addEventListener("submit", event => {
        event.preventDefault();
        loadData();
    });
    qs("#resetAnalyticsFilters")?.addEventListener("click", () => {
        qs("#analyticsFilters").reset();
        loadData();
    });
    qs("#downloadOrdersCsv")?.addEventListener("click", downloadOrdersCsv);
    qs("#tableForm").addEventListener("submit", event => submitDirectory(event, "/api/diningtables", {
        number: Number(qs("#tableNumber").value),
        seatsCount: Number(qs("#tableSeats").value),
        status: qs("#tableStatus").value
    }));
    qs("#employeeForm").addEventListener("submit", event => submitDirectory(event, "/api/employees", {
        fullName: qs("#employeeName").value,
        position: qs("#employeePosition").value,
        phone: qs("#employeePhone").value,
        email: qs("#employeeEmail").value
    }));
    qs("#categoryForm").addEventListener("submit", event => submitDirectory(event, "/api/categories", {
        name: qs("#categoryName").value,
        description: qs("#categoryDescription").value
    }));
    qs("#orderFilters").addEventListener("submit", submitFilters);
}

async function downloadOrdersCsv() {
    const from = qs("#analyticsFrom").value;
    const to = qs("#analyticsTo").value;
    if (!from || !to) {
        showMessage("Select From and To dates before downloading CSV.", true);
        return;
    }

    const params = new URLSearchParams(getAnalyticsQuery());
    params.set("format", "csv");
    const response = await fetch(`/api/reports/orders/export?${params.toString()}`, {
        headers: authHeaders()
    });

    if (!response.ok) {
        showMessage("CSV export failed.", true);
        return;
    }

    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `orders_report_${from}_${to}.csv`;
    link.click();
    URL.revokeObjectURL(url);
}

function activateView(viewId) {
    if (!isAdmin() && viewId !== "orders") {
        viewId = "orders";
    }

    qsa(".nav-button").forEach(item => item.classList.toggle("active", item.dataset.view === viewId));
    qsa(".view").forEach(item => item.classList.toggle("active", item.id === viewId));
}

async function submitOrder(event) {
    event.preventDefault();
    const items = qsa(".order-item-row").map(row => ({
        dishId: Number(row.querySelector(".dish-select").value),
        quantity: Number(row.querySelector(".quantity-input").value)
    })).filter(item => item.dishId && item.quantity > 0);

    try {
        await api.post("/api/orders", {
            diningTableId: Number(qs("#orderTable").value),
            employeeId: isAdmin() ? Number(qs("#orderEmployee").value) : Number(auth.employeeId),
            items
        });
        qs("#orderForm").reset();
        qs("#orderItems").innerHTML = "";
        addOrderItemRow();
        showMessage("Замовлення створено.");
        await refreshOrdersAndAnalytics();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function submitDish(event) {
    event.preventDefault();
    const id = qs("#dishId").value;
    const payload = {
        name: qs("#dishName").value,
        description: qs("#dishDescription").value,
        price: Number(qs("#dishPrice").value),
        categoryId: Number(qs("#dishCategory").value),
        isAvailable: qs("#dishAvailable").checked
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
        state.dishes = await api.get("/api/dishes");
        renderSelects();
        renderDishes();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function submitDirectory(event, url, payload) {
    event.preventDefault();

    try {
        await api.post(url, payload);
        event.target.reset();
        showMessage("Запис додано.");
        await loadData();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function submitFilters(event) {
    event.preventDefault();
    const params = new URLSearchParams();
    addParam(params, "from", qs("#filterFrom").value);
    addParam(params, "to", qs("#filterTo").value);
    addParam(params, "diningTableId", qs("#filterTable").value);
    addParam(params, "employeeId", qs("#filterEmployee").value);
    addParam(params, "status", qs("#filterStatus").value);

    try {
        state.orders = await api.get(`/api/orders?${params.toString()}`);
        renderOrders();
    } catch (error) {
        showMessage(error.message, true);
    }
}

function addParam(params, key, value) {
    if (value) {
        params.set(key, value);
    }
}

async function handleDeleteClick(target) {
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
                await loadData();
            } catch (error) {
                showMessage(error.message, true);
            }
        }
    }
}

async function refreshOrdersAndAnalytics() {
    if (!isAdmin()) {
        const [orders, tables] = await Promise.all([
            api.get("/api/orders"),
            api.get("/api/diningtables")
        ]);

        state.orders = orders;
        state.diningTables = tables;
        renderSelects();
        renderOrders();
        renderDirectories();
        return;
    }

    const [orders, tables, summary, popularDishes, salesByEmployee, ordersCountByDay, revenueByCategory, revenueByTable, tableOccupancy] = await Promise.all([
        api.get("/api/orders"),
        api.get("/api/diningtables"),
        api.get("/api/analytics/summary"),
        api.get("/api/analytics/popular-dishes"),
        api.get("/api/analytics/sales-by-employee"),
        api.get("/api/analytics/orders-count-by-day"),
        api.get("/api/analytics/revenue-by-category"),
        api.get("/api/analytics/revenue-by-table"),
        api.get("/api/analytics/table-occupancy")
    ]);

    state.orders = orders;
    state.diningTables = tables;
    state.analytics = { summary, popularDishes, salesByEmployee, ordersCountByDay, revenueByCategory, revenueByTable, tableOccupancy };
    renderSelects();
    renderOrders();
    renderAnalytics();
    renderDirectories();
}

async function refreshTablesAndAnalytics() {
    if (!isAdmin()) {
        state.diningTables = await api.get("/api/diningtables");
        renderSelects();
        renderDirectories();
        return;
    }

    const [tables, summary, revenueByTable, tableOccupancy] = await Promise.all([
        api.get("/api/diningtables"),
        api.get("/api/analytics/summary"),
        api.get("/api/analytics/revenue-by-table"),
        api.get("/api/analytics/table-occupancy")
    ]);

    state.diningTables = tables;
    state.analytics = {
        ...state.analytics,
        summary,
        revenueByTable,
        tableOccupancy
    };

    renderSelects();
    renderAnalytics();
    renderDirectories();
}

function setupAuthUi() {
    const logout = document.createElement("button");
    logout.className = "secondary-button";
    logout.type = "button";
    logout.textContent = "Logout";
    logout.addEventListener("click", () => {
        localStorage.removeItem("auth");
        window.location.href = "/login.html";
    });
    qs(".topbar").appendChild(logout);

    if (!isAdmin()) {
        qsa('[data-view="dashboard"], [data-view="menu"], [data-view="directories"]').forEach(item => item.hidden = true);
        qs("#filterEmployee")?.closest("label")?.setAttribute("hidden", "hidden");
    }
}

setupAuthUi();
setupEvents();
activateView(isAdmin() ? "dashboard" : "orders");
addOrderItemRow();
loadData();
