/* ---------------------------------------------------------------------------
   Modular Monolith Store — the UI logic.
   The browser only knows the HTTP API of the modules; every rule lives on the
   server side.
   --------------------------------------------------------------------------- */

const state = {
    token: localStorage.getItem('mm.token') || null,
    user: JSON.parse(localStorage.getItem('mm.user') || 'null'),
    cartId: localStorage.getItem('mm.cartId') || createCartId(),
    products: [],
    stock: new Map(),
    cart: null,
    view: 'catalog'
};

function createCartId() {
    const id = 'cart-' + Math.random().toString(36).slice(2, 10) + Date.now().toString(36);
    localStorage.setItem('mm.cartId', id);
    return id;
}

const $ = (id) => document.getElementById(id);
const money = (value) => 'R$ ' + Number(value || 0).toFixed(2).replace('.', ',').replace(/,(\d{3})(?=\d)/g, '.$1');

/* ----------------------------------------------------------------- api --- */

async function api(path, options = {}) {
    const headers = { 'Content-Type': 'application/json' };

    if (state.token)
        headers.Authorization = 'Bearer ' + state.token;

    const response = await fetch(path, {
        ...options,
        headers: { ...headers, ...(options.headers || {}) }
    });

    if (response.status === 204)
        return null;

    const text = await response.text();
    const body = text ? JSON.parse(text) : null;

    if (!response.ok) {
        const message = body && body.error ? body.error : 'Request failed (' + response.status + ').';
        throw new Error(message);
    }

    return body;
}

const get = (path) => api(path);
const post = (path, payload) => api(path, { method: 'POST', body: JSON.stringify(payload) });
const put = (path, payload) => api(path, { method: 'PUT', body: JSON.stringify(payload) });
const del = (path) => api(path, { method: 'DELETE' });

/* ---------------------------------------------------------------- toast -- */

let toastTimer;

function toast(message, kind = '') {
    const element = $('toast');
    element.textContent = message;
    element.className = 'toast ' + kind;
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => element.classList.add('hidden'), 4000);
}

/* --------------------------------------------------------------- login --- */

$('login-form').addEventListener('submit', async (event) => {
    event.preventDefault();
    $('login-error').classList.add('hidden');

    try {
        const result = await post('/api/auth/login', {
            email: $('login-email').value.trim(),
            password: $('login-password').value
        });

        state.token = result.accessToken;
        state.user = result.user;
        localStorage.setItem('mm.token', state.token);
        localStorage.setItem('mm.user', JSON.stringify(state.user));

        await start();
    } catch (error) {
        $('login-error').textContent = error.message;
        $('login-error').classList.remove('hidden');
    }
});

document.querySelectorAll('[data-demo-email]').forEach((button) => {
    button.addEventListener('click', () => {
        $('login-email').value = button.dataset.demoEmail;
        $('login-password').value = button.dataset.demoPassword;
    });
});

$('logout').addEventListener('click', () => {
    state.token = null;
    state.user = null;
    localStorage.removeItem('mm.token');
    localStorage.removeItem('mm.user');
    $('app-view').classList.add('hidden');
    $('login-view').classList.remove('hidden');
});

/* ---------------------------------------------------------------- tabs --- */

document.querySelectorAll('.tab').forEach((tab) => {
    tab.addEventListener('click', () => show(tab.dataset.view));
});

async function show(view) {
    state.view = view;

    document.querySelectorAll('.tab').forEach(tab => tab.classList.toggle('active', tab.dataset.view === view));
    document.querySelectorAll('.view').forEach(section => section.classList.add('hidden'));
    $('view-' + view).classList.remove('hidden');

    // The tab is kept in the address bar, so a view can be bookmarked.
    const url = new URL(location.href);
    url.searchParams.set('view', view);
    history.replaceState({}, '', url);

    if (view === 'catalog') await loadCatalog();
    if (view === 'cart') await loadCart();
    if (view === 'orders') await loadOrders();
    if (view === 'inventory') await loadInventory();
    if (view === 'purchases') await loadPurchases();
}

/* -------------------------------------------------------------- catalog -- */

async function loadCatalog() {
    const search = $('search').value.trim();
    const category = $('category').value;

    const query = new URLSearchParams();
    if (search) query.set('q', search);
    if (category) query.set('category', category);

    const products = await get('/api/catalog/products?' + query.toString());
    const inventory = await get('/api/inventory');

    state.products = products;
    state.stock = new Map(inventory.map(item => [item.productId, item.quantity]));

    const grid = $('product-grid');
    grid.innerHTML = '';

    if (products.length === 0) {
        grid.innerHTML = '<div class="empty">No product matches this search.</div>';
        return;
    }

    products.forEach((product) => {
        const quantity = state.stock.get(product.id) ?? 0;

        const card = document.createElement('div');
        card.className = 'product';
        card.innerHTML = `
            <div class="product-emoji">${escapeHtml(product.emoji)}</div>
            <h3>${escapeHtml(product.name)}</h3>
            <div class="sku">${escapeHtml(product.sku)} &middot; ${escapeHtml(product.category)}</div>
            <div class="price">${money(product.price)}</div>
            <div class="stock ${quantity === 0 ? 'stock-out' : ''}">${quantity === 0 ? 'Out of stock' : quantity + ' in stock'}</div>
        `;

        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'primary';
        button.textContent = quantity === 0 ? 'Unavailable' : 'Add to the cart';
        button.disabled = quantity === 0;
        button.addEventListener('click', () => addToCart(product));

        card.appendChild(button);
        grid.appendChild(card);
    });
}

async function loadCategories() {
    const categories = await get('/api/catalog/categories');
    const select = $('category');
    const list = $('category-list');

    select.innerHTML = '<option value="">All categories</option>';
    list.innerHTML = '';

    categories.forEach(category => {
        const option = document.createElement('option');
        option.value = category;
        option.textContent = category;
        select.appendChild(option);

        const item = document.createElement('option');
        item.value = category;
        list.appendChild(item);
    });
}

let searchTimer;

$('search').addEventListener('input', () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => show('catalog'), 250);
});

$('category').addEventListener('change', () => show('catalog'));

async function addToCart(product) {
    try {
        state.cart = await post(`/api/cart/${state.cartId}/items`, { productId: product.id, quantity: 1 });
        renderCart();
        toast(product.name + ' added to the cart.', 'ok');
    } catch (error) {
        toast(error.message, 'error');
    }
}

/* ----------------------------------------------------------------- cart -- */

async function loadCart() {
    state.cart = await get(`/api/cart/${state.cartId}`);
    renderCart();
}

function renderCart() {
    const cart = state.cart;
    const container = $('cart-lines');
    container.innerHTML = '';

    if (!cart || cart.items.length === 0) {
        container.innerHTML = '<div class="empty">Your cart is empty.</div>';
        $('cart-total').textContent = money(0);
        $('cart-badge').classList.add('hidden');
        return;
    }

    cart.items.forEach(line => {
        const row = document.createElement('div');
        row.className = 'cart-line';
        row.innerHTML = `
            <div class="emoji">${escapeHtml(line.emoji)}</div>
            <div>
                <div class="name">${escapeHtml(line.name)}</div>
                <div class="sub">${escapeHtml(line.sku)} &middot; ${money(line.price)} each</div>
            </div>
            <div class="qty">
                <button type="button" data-action="minus">&minus;</button>
                <span>${line.quantity}</span>
                <button type="button" data-action="plus">+</button>
            </div>
            <strong>${money(line.subtotal)}</strong>
            <button type="button" class="ghost small-button" data-action="remove">Remove</button>
        `;

        row.querySelector('[data-action="minus"]').addEventListener('click', () => changeQuantity(line, line.quantity - 1));
        row.querySelector('[data-action="plus"]').addEventListener('click', () => changeQuantity(line, line.quantity + 1));
        row.querySelector('[data-action="remove"]').addEventListener('click', () => removeLine(line));

        container.appendChild(row);
    });

    $('cart-total').textContent = money(cart.subtotal);
    $('cart-badge').textContent = cart.totalItems;
    $('cart-badge').classList.remove('hidden');
}

async function changeQuantity(line, quantity) {
    if (quantity < 1) {
        await removeLine(line);
        return;
    }

    try {
        state.cart = await put(`/api/cart/${state.cartId}/items/${line.productId}`, { quantity });
        renderCart();
    } catch (error) {
        toast(error.message, 'error');
    }
}

async function removeLine(line) {
    state.cart = await del(`/api/cart/${state.cartId}/items/${line.productId}`);
    renderCart();
}

$('clear-cart').addEventListener('click', async () => {
    state.cart = await del(`/api/cart/${state.cartId}`);
    renderCart();
});

/* ------------------------------------------------------------- checkout -- */

$('checkout').addEventListener('click', async () => {
    if (!state.cart || state.cart.items.length === 0) {
        toast('The cart is empty.', 'error');
        return;
    }

    if (!state.user) {
        toast('Sign in to finalize the order.', 'error');
        $('app-view').classList.add('hidden');
        $('login-view').classList.remove('hidden');
        return;
    }

    const methods = await get('/api/payments/methods');
    const select = $('c-method');
    select.innerHTML = '';
    methods.forEach(method => {
        const option = document.createElement('option');
        option.value = method;
        option.textContent = method.toUpperCase();
        select.appendChild(option);
    });

    $('c-name').value = state.user.displayName || '';
    $('c-email').value = state.user.email || '';
    $('checkout-error').classList.add('hidden');
    $('checkout-modal').classList.remove('hidden');
});

$('checkout-close').addEventListener('click', () => $('checkout-modal').classList.add('hidden'));

$('checkout-form').addEventListener('submit', async (event) => {
    event.preventDefault();
    $('checkout-error').classList.add('hidden');

    try {
        const order = await post('/api/orders/checkout', {
            cartId: state.cartId,
            customerName: $('c-name').value,
            email: $('c-email').value,
            address: $('c-address').value,
            city: $('c-city').value,
            zip: $('c-zip').value,
            paymentMethod: $('c-method').value,
            simulateFailure: $('c-fail').checked
        });

        $('checkout-modal').classList.add('hidden');
        $('checkout-form').reset();
        await loadCart();

        if (order.status === 'Cancelled') {
            // The payment was declined: the order exists but nothing left the
            // inventory and the cart is still there.
            const reason = order.payment && order.payment.reason
                ? order.payment.reason
                : 'The payment was declined.';

            toast(reason + ' The order was cancelled and the stock was kept.', 'error');
        } else {
            toast('Order ' + order.code + ' placed.', 'ok');
        }

        show('orders');
    } catch (error) {
        $('checkout-error').textContent = error.message;
        $('checkout-error').classList.remove('hidden');
    }
});

/* --------------------------------------------------------------- orders -- */

async function loadOrders() {
    const orders = await get('/api/orders/mine');
    renderOrders(orders, $('order-list'), false);
}

async function loadPurchases() {
    const orders = await get('/api/orders');

    const paid = orders.filter(order => order.status === 'Paid' || order.status === 'Shipped');
    const revenue = paid.reduce((sum, order) => sum + Number(order.total), 0);

    $('purchase-kpis').innerHTML = `
        <div class="kpi"><span>Orders</span><strong>${orders.length}</strong></div>
        <div class="kpi ok"><span>Paid revenue</span><strong>${money(revenue)}</strong></div>
        <div class="kpi"><span>Items sold</span><strong>${orders.reduce((sum, order) => sum + order.items.reduce((n, item) => n + item.quantity, 0), 0)}</strong></div>
    `;

    renderOrders(orders, $('purchase-list'), true);
}

function renderOrders(orders, container, showCustomer) {
    container.innerHTML = '';

    if (orders.length === 0) {
        container.innerHTML = '<div class="empty">No order yet.</div>';
        return;
    }

    orders.forEach(order => {
        const card = document.createElement('div');
        card.className = 'order';

        const payment = order.payment
            ? ` &middot; payment <span class="status ${order.payment.status.toLowerCase()}">${order.payment.status}</span>`
            : '';

        card.innerHTML = `
            <div class="order-head">
                <span class="order-code">${escapeHtml(order.code)}</span>
                <span class="status ${order.status.toLowerCase()}">${order.status}</span>
                ${payment}
                ${showCustomer ? `<span class="muted small">${escapeHtml(order.customerName)} &middot; ${escapeHtml(order.email)}</span>` : ''}
                <span class="order-total">${money(order.total)}</span>
            </div>
            <ul class="order-items">
                ${order.items.map(item => `<li>${escapeHtml(item.emoji)} ${escapeHtml(item.name)} &times; ${item.quantity}</li>`).join('')}
            </ul>
        `;

        container.appendChild(card);
    });
}

$('refresh-orders').addEventListener('click', loadOrders);
$('refresh-purchases').addEventListener('click', loadPurchases);

/* ------------------------------------------------------------ inventory -- */

async function loadInventory() {
    const items = await get('/api/inventory');

    const low = items.filter(item => item.quantity <= item.reorderLevel);
    const out = items.filter(item => item.quantity === 0);
    const units = items.reduce((sum, item) => sum + item.quantity, 0);

    $('inventory-kpis').innerHTML = `
        <div class="kpi"><span>Products</span><strong>${items.length}</strong></div>
        <div class="kpi"><span>Units in stock</span><strong>${units}</strong></div>
        <div class="kpi warn"><span>Low stock</span><strong>${low.length}</strong></div>
        <div class="kpi danger"><span>Out of stock</span><strong>${out.length}</strong></div>
    `;

    const rows = $('inventory-rows');
    rows.innerHTML = '';

    items.forEach(item => {
        const state_ = item.quantity === 0 ? 'Out of stock' : item.quantity <= item.reorderLevel ? 'Low' : 'Healthy';
        const color = item.quantity === 0 ? 'cancelled' : item.quantity <= item.reorderLevel ? 'pending' : 'paid';

        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${escapeHtml(item.emoji)} ${escapeHtml(item.productName)}</td>
            <td class="muted">${escapeHtml(item.sku)}</td>
            <td class="right"><strong>${item.quantity}</strong></td>
            <td class="right muted">${item.reorderLevel}</td>
            <td><span class="status ${color}">${state_}</span></td>
            <td class="row-actions">
                <button type="button" data-action="minus">&minus;10</button>
                <button type="button" data-action="plus">+10</button>
                <button type="button" data-action="restock">Restock 50</button>
            </td>
        `;

        row.querySelector('[data-action="minus"]').addEventListener('click', () => adjust(item, item.quantity - 10));
        row.querySelector('[data-action="plus"]').addEventListener('click', () => adjust(item, item.quantity + 10));
        row.querySelector('[data-action="restock"]').addEventListener('click', () => restock(item, 50));

        rows.appendChild(row);
    });
}

async function adjust(item, quantity) {
    try {
        await put(`/api/inventory/${item.productId}/quantity`, { quantity, reorderLevel: item.reorderLevel });
        toast(item.productName + ': stock set to ' + quantity + '.', 'ok');
        await loadInventory();
    } catch (error) {
        toast(error.message, 'error');
    }
}

async function restock(item, delta) {
    try {
        await post(`/api/inventory/${item.productId}/restock`, { delta });
        toast(item.productName + ': +' + delta + ' units.', 'ok');
        await loadInventory();
    } catch (error) {
        toast(error.message, 'error');
    }
}

$('refresh-inventory').addEventListener('click', loadInventory);

/* ---------------------------------------------------------- new product -- */

$('product-form').addEventListener('submit', async (event) => {
    event.preventDefault();
    $('product-error').classList.add('hidden');

    try {
        await post('/api/catalog/products', {
            sku: $('p-sku').value.trim(),
            name: $('p-name').value.trim(),
            description: $('p-description').value.trim(),
            category: $('p-category').value.trim(),
            emoji: $('p-emoji').value.trim() || '📦',
            price: Number($('p-price').value),
            initialStock: Number($('p-stock').value),
            reorderLevel: Number($('p-reorder').value)
        });

        $('product-form').reset();
        toast('The product was added to the catalog.', 'ok');
        await loadCategories();
        await show('catalog');
    } catch (error) {
        $('product-error').textContent = error.message;
        $('product-error').classList.remove('hidden');
    }
});

/* ---------------------------------------------------------------- utils -- */

const AMP = '\u0026';

const HTML_ENTITIES = {
    '&': AMP + 'amp;',
    '<': AMP + 'lt;',
    '>': AMP + 'gt;',
    '"': AMP + 'quot;',
    "'": AMP + '#39;'
};

function escapeHtml(value) {
    return String(value ?? '').replace(/[&<>"']/g, character => HTML_ENTITIES[character]);
}

/* ---------------------------------------------------------------- start -- */

async function start() {
    $('login-view').classList.add('hidden');
    $('app-view').classList.remove('hidden');

    const isAdmin = state.user && state.user.role === 'Admin';

    $('session-user').textContent = state.user.displayName || state.user.email;
    $('session-role').textContent = state.user.role;
    $('session-role').className = 'role-badge' + (isAdmin ? ' admin' : '');

    // The administrator also satisfies the Buyer policy: an admin may buy too.
    document.querySelectorAll('.admin-only').forEach(element => element.classList.toggle('hidden', !isAdmin));
    document.querySelectorAll('.buyer-only').forEach(element => element.classList.remove('hidden'));

    await loadCategories();

    // A view can be requested by the address bar (?view=inventory), as long as
    // the signed in role is allowed to see it.
    const requested = new URLSearchParams(location.search).get('view');
    const tab = requested ? document.querySelector('.tab[data-view="' + requested + '"]') : null;

    await show(tab && !tab.classList.contains('hidden') ? requested : 'catalog');
}

if (state.token && state.user)
    start().catch(() => {
        localStorage.removeItem('mm.token');
        localStorage.removeItem('mm.user');
        state.token = null;
        state.user = null;
    });
