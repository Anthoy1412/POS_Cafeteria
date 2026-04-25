let carrito = [];

const API_URL = window.location.origin + "/api";



const session = JSON.parse(localStorage.getItem('usuario'));

if (!session && !window.location.href.includes('login.html')) {

window.location.href = 'login.html';

}



function salir() {

localStorage.removeItem('usuario');

window.location.href = 'login.html';

}



async function cargarProductos() {

try {

const res = await fetch(`${API_URL}/products`);

const productos = await res.json();

const div = document.getElementById('lista-productos');


div.innerHTML = productos.map(p => `

<div class="col-12 col-sm-6 col-md-4 mb-3">

<div class="card h-100 shadow-sm border-0">

<div class="card-body text-center" onclick="agregarAlCarrito(${p.productId}, '${p.name}', ${p.price})" style="cursor:pointer">

<h6 class="fw-bold text-dark">${p.name}</h6>

<span class="badge bg-success mb-2">$${p.price.toFixed(2)}</span>

<p class="small text-muted mb-0">Stock: ${p.stock}</p>

</div>

<div class="card-footer bg-light border-0 d-flex justify-content-between p-2">

<button class="btn btn-sm btn-outline-secondary" onclick="restablecerStock(${p.productId})">📦 Stock</button>

<button class="btn btn-sm btn-outline-danger" onclick="eliminarProducto(${p.productId})">🗑️ Borrar</button>

</div>

</div>

</div>

`).join('');

} catch (e) { console.error("Error:", e); }

}



async function eliminarProducto(id) {

if (!confirm("¿Eliminar producto permanentemente?")) return;

const res = await fetch(`${API_URL}/products/${id}`, { method: 'DELETE' });

if (res.ok) { cargarProductos(); }

else { alert("No se puede borrar un producto con historial de ventas."); }

}



async function restablecerStock(id) {

const cant = prompt("Nueva cantidad en stock:");

if (!cant) return;

await fetch(`${API_URL}/products/update-stock/${id}`, {

method: 'PUT',

headers: { 'Content-Type': 'application/json' },

body: JSON.stringify(parseInt(cant))

});

cargarProductos();

}



async function crearProducto() {

const nombre = prompt("Nombre:");

const precio = prompt("Precio:");

const stock = prompt("Stock:");

if (!nombre || !precio) return;

await fetch(`${API_URL}/products`, {

method: 'POST',

headers: { 'Content-Type': 'application/json' },

body: JSON.stringify({ name: nombre, price: parseFloat(precio), stock: parseInt(stock), categoryId: 1 })

});

cargarProductos();

}



function agregarAlCarrito(id, nombre, precio) {

const item = carrito.find(p => p.productId === id);

if (item) item.quantity++;

else carrito.push({ productId: id, name: nombre, price: precio, quantity: 1 });

renderizar();

}



function renderizar() {

const body = document.getElementById('carrito-body');

let total = 0;

body.innerHTML = carrito.map((p, i) => {

total += p.price * p.quantity;

return `<tr><td>${p.name}</td><td>${p.quantity}</td><td>$${(p.price*p.quantity).toFixed(2)}</td>

<td><button class="btn btn-sm btn-danger py-0" onclick="carrito.splice(${i},1);renderizar()">x</button></td></tr>`;

}).join('');

document.getElementById('total-venta').innerText = `$${total.toFixed(2)}`;

}



async function finalizarVenta() {

if (carrito.length === 0) return;

const venta = {

userId: session ? session.userId : 1,

total: carrito.reduce((s, p) => s + (p.price * p.quantity), 0),

details: carrito.map(p => ({ productId: p.productId, quantity: p.quantity, price: p.price }))

};

const res = await fetch(`${API_URL}/sales`, {

method: 'POST',

headers: { 'Content-Type': 'application/json' },

body: JSON.stringify(venta)

});

if (res.ok) {

const data = await res.json();

mostrarFactura(data.saleId);

carrito = [];

renderizar();

cargarProductos();

}

}



async function verReportes() {

const resTop = await fetch(`${API_URL}/reports/top-products`);

const top = await resTop.json();

document.getElementById('lista-top-productos').innerHTML = top.map(i =>

`<li class="list-group-item d-flex justify-content-between">${i.productName || i.ProductName} <span>${i.totalSold || i.TotalSold}</span></li>`

).join('');



const resVentas = await fetch(`${API_URL}/reports/daily-sales`);

const daily = await resVentas.json();

document.getElementById('reporte-total-ventas').innerText = `$${(daily.totalAmount || daily.TotalAmount || 0).toFixed(2)}`;



const resStock = await fetch(`${API_URL}/products/low-stock`);

const low = await resStock.json();

document.getElementById('reporte-stock-bajo').innerText = low.length;

new bootstrap.Modal('#modalReportes').show();

}



async function mostrarFactura(id) {

const res = await fetch(`${API_URL}/invoice/${id}`);

const data = await res.json();

document.getElementById('ticket-print').innerHTML = `

<div class="text-center"><h6>☕ CAFETERÍA</h6><p>#${data.folio}<br>${data.fecha}</p></div>

<hr>${data.productos.map(p => `<div class="d-flex justify-content-between"><span>${p.nombre} x${p.cantidad}</span><span>$${p.subtotal}</span></div>`).join('')}

<hr><div class="d-flex justify-content-between fw-bold"><span>TOTAL</span><span>$${data.total}</span></div>`;

new bootstrap.Modal('#modalFactura').show();

}



cargarProductos();