<script setup>
import { computed, onMounted, ref, watch } from 'vue'

const categories = ref([])
const products = ref([])
const total = ref(0)
const loading = ref(true)
const error = ref('')
const keyword = ref('')
const activeFilter = ref('all')
const activeCategory = ref(null)
const selectedProduct = ref(null)
const cartOpen = ref(false)
const cartLoading = ref(false)
const cartError = ref('')
const notice = ref('')
const auth = ref({ isAuthenticated: false })
const authOpen = ref(false)
const authMode = ref('login')
const authForm = ref({ displayName: '', email: 'demo@resellhub.local', phone: '', password: 'Demo123!' })
const authError = ref('')
const checkoutOpen = ref(false)
const checkoutForm = ref({ recipientName: '', phone: '', address: '' })
const cartData = ref({ items: [], itemCount: 0, subtotal: 0, shippingFee: 0, total: 0 })
const cartCount = computed(() => cartData.value.itemCount)

const filters = [
  ['all', '全部商品'], ['in-stock', '有庫存商品'],
  ['under-500', 'NT$500 以下'], ['500-1500', 'NT$500～1,500'], ['over-1500', 'NT$1,500 以上']
]

const money = value => new Intl.NumberFormat('zh-TW').format(value ?? 0)
const queryString = computed(() => {
  const p = new URLSearchParams({ filter: activeFilter.value })
  if (keyword.value.trim()) p.set('keyword', keyword.value.trim())
  if (activeCategory.value) p.set('categoryId', activeCategory.value)
  return p.toString()
})

async function loadProducts() {
  loading.value = true
  error.value = ''
  try {
    const response = await fetch(`/api/store/products?${queryString.value}`)
    if (!response.ok) throw new Error('商品資料讀取失敗')
    const data = await response.json()
    products.value = data.items
    total.value = data.total
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

function selectFilter(filter) { activeFilter.value = filter; activeCategory.value = null }
function selectCategory(id) { activeCategory.value = id; activeFilter.value = 'all' }
function search() { loadProducts() }

async function cartRequest(url, options) {
  cartLoading.value = true
  cartError.value = ''
  try {
    const response = await fetch(url, { headers: { 'Content-Type': 'application/json' }, ...options })
    if (response.status === 401) { authOpen.value = true; throw new Error('請先登入會員。') }
    const data = await response.json()
    if (!response.ok) throw new Error(data.message || '購物車操作失敗')
    cartData.value = data
    return true
  } catch (e) {
    cartError.value = e.message
    return false
  } finally {
    cartLoading.value = false
  }
}

async function loadCart() { await cartRequest('/api/cart') }
async function addCart(product) {
  const success = await cartRequest('/api/cart/items', { method: 'POST', body: JSON.stringify({ productId: product.id, quantity: 1 }) })
  if (success) {
    notice.value = `已將「${product.name}」加入購物車`
    setTimeout(() => notice.value = '', 2200)
  }
}
async function updateQuantity(item, quantity) {
  if (quantity < 1 || quantity > item.stockQuantity) return
  await cartRequest(`/api/cart/items/${item.id}`, { method: 'PUT', body: JSON.stringify({ quantity }) })
}
async function removeCartItem(item) {
  await cartRequest(`/api/cart/items/${item.id}`, { method: 'DELETE' })
}
async function openCart() { cartOpen.value = true; await loadCart() }
async function loadAuth() { auth.value = await (await fetch('/api/auth/me')).json() }
async function submitAuth() {
  authError.value = ''
  const url = authMode.value === 'login' ? '/api/auth/login' : '/api/auth/register'
  const response = await fetch(url, { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(authForm.value) })
  const data = await response.json()
  if (!response.ok) { authError.value = data.message || '會員操作失敗'; return }
  auth.value = data; authOpen.value = false; await loadCart()
}
async function logout() {
  await fetch('/api/auth/logout', { method:'POST' }); auth.value={isAuthenticated:false}; cartData.value={items:[],itemCount:0,subtotal:0,shippingFee:0,total:0}
}
async function checkout() {
  const response = await fetch('/api/checkout', { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(checkoutForm.value) })
  const data = await response.json()
  if (!response.ok) { cartError.value=data.message||'結帳失敗'; return }
  checkoutOpen.value=false; cartOpen.value=false; await Promise.all([loadCart(),loadProducts()]); notice.value=`訂單 ${data.orderNumber} 建立成功`; setTimeout(()=>notice.value='',3500)
}
watch([activeFilter, activeCategory], loadProducts)

onMounted(async () => {
  try {
    categories.value = await (await fetch('/api/store/categories')).json()
  } catch { error.value = '分類資料讀取失敗' }
  await loadAuth()
  await Promise.all([loadProducts(), auth.value.isAuthenticated ? loadCart() : Promise.resolve()])
})
</script>

<template>
  <header class="header">
    <div class="utility"><span>讓好物找到新主人</span><div><button v-if="!auth.isAuthenticated" class="utility-button" @click="authOpen=true">登入／註冊</button><template v-else><span>Hi，{{ auth.name }}</span><button class="utility-button" @click="logout">登出</button></template><a href="/ProductManagement">賣家管理</a></div></div>
    <div class="nav">
      <a class="brand" href="/app/index.html"><b>R</b> ReSellHub</a>
      <form class="search" @submit.prevent="search"><input v-model="keyword" placeholder="搜尋二手商品"><button>搜尋</button></form>
      <button class="cart" @click="openCart">🛒<i v-if="cartCount">{{ cartCount }}</i></button>
    </div>
  </header>

  <main>
    <section class="hero"><div><small>ReSell 精選</small><h1>好物不浪費，輕鬆找到下一位主人</h1><p>探索價格實惠的二手商品，讓每一次購買都更有價值。</p></div><strong>♻️<span>二手新生活</span></strong></section>
    <div class="layout">
      <aside>
        <h3>☰ 商品篩選</h3>
        <nav>
          <button v-for="f in filters.slice(0,2)" :key="f[0]" :class="{active:activeFilter===f[0]&&!activeCategory}" @click="selectFilter(f[0])">{{ f[1] }}</button>
          <p>商品分類</p>
          <button v-for="c in categories" :key="c.id" :class="{active:activeCategory===c.id}" @click="selectCategory(c.id)">{{ c.icon }} {{ c.name }}</button>
          <p>價格範圍</p>
          <button v-for="f in filters.slice(2)" :key="f[0]" :class="{active:activeFilter===f[0]&&!activeCategory}" @click="selectFilter(f[0])">{{ f[1] }}</button>
        </nav>
      </aside>

      <section class="catalog">
        <div class="toolbar"><b>商城商品</b><span>找到 {{ total }} 件商品</span></div>
        <div v-if="loading" class="state">正在載入商品…</div>
        <div v-else-if="error" class="state error">{{ error }}<button @click="loadProducts">重新讀取</button></div>
        <div v-else-if="!products.length" class="state">🔎<h2>找不到符合條件的商品</h2></div>
        <div v-else class="grid">
          <article v-for="p in products" :key="p.id" @click="selectedProduct=p">
            <div class="image" :style="p.coverImageUrl?{backgroundImage:`url(${p.coverImageUrl})`}:{}"><span v-if="!p.coverImageUrl">{{ p.category?.icon || '📦' }}</span><em>{{ p.condition }}</em><i v-if="p.stockQuantity<=0">已售完</i></div>
            <div class="info"><div class="name">{{ p.name }}</div><small>{{ p.category?.name || '其他好物' }}</small><div class="price"><sup>NT$</sup>{{ money(p.sellingPrice) }}</div><footer><span>庫存 {{ p.stockQuantity }}</span><span>♡ 收藏</span></footer></div>
          </article>
        </div>
      </section>
    </div>
  </main>

  <div v-if="selectedProduct" class="modal" @click.self="selectedProduct=null">
    <section class="detail"><button class="close" @click="selectedProduct=null">×</button><div class="detail-image" :style="{backgroundImage:`url(${selectedProduct.coverImageUrl||'/images/product-placeholder.svg'})`}"></div><div><small class="tag">{{ selectedProduct.category?.name }}</small><h2>{{ selectedProduct.name }}</h2><div class="stars">5.0 ★★★★★</div><div class="detail-price">NT$ {{ money(selectedProduct.sellingPrice) }}</div><p>{{ selectedProduct.description || '賣家尚未提供詳細說明。' }}</p><p>商品狀況：<b>{{ selectedProduct.condition }}</b>　庫存：<b>{{ selectedProduct.stockQuantity }}</b></p><button class="add" :disabled="selectedProduct.stockQuantity<=0" @click="addCart(selectedProduct)">🛒 加入購物車</button></div></section>
  </div>

  <div v-if="cartOpen" class="drawer-mask" @click.self="cartOpen=false">
    <aside class="cart-drawer">
      <header><div><small>SHOPPING CART</small><h2>我的購物車</h2></div><button @click="cartOpen=false">×</button></header>
      <div v-if="cartLoading && !cartData.items.length" class="cart-state">正在讀取購物車…</div>
      <div v-else-if="!cartData.items.length" class="cart-state"><span>🛒</span><h3>購物車是空的</h3><p>去商城挑選喜歡的二手好物吧！</p><button @click="cartOpen=false">繼續購物</button></div>
      <div v-else class="cart-content">
        <div class="cart-items">
          <section v-for="item in cartData.items" :key="item.id" class="cart-item">
            <img :src="item.imageUrl || '/images/product-placeholder.svg'" :alt="item.productName">
            <div class="cart-item-info"><b>{{ item.productName }}</b><small>單價 NT$ {{ money(item.unitPrice) }} · 庫存 {{ item.stockQuantity }}</small><div class="quantity"><button @click="updateQuantity(item,item.quantity-1)" :disabled="item.quantity<=1">−</button><span>{{ item.quantity }}</span><button @click="updateQuantity(item,item.quantity+1)" :disabled="item.quantity>=item.stockQuantity">＋</button><button class="remove" @click="removeCartItem(item)">移除</button></div></div>
            <strong>NT$ {{ money(item.lineTotal) }}</strong>
          </section>
        </div>
        <div v-if="cartError" class="cart-error">{{ cartError }}</div>
        <footer class="summary"><p><span>商品小計</span><b>NT$ {{ money(cartData.subtotal) }}</b></p><p><span>運費</span><b>NT$ {{ money(cartData.shippingFee) }}</b></p><p class="total"><span>合計</span><b>NT$ {{ money(cartData.total) }}</b></p><button @click="checkoutOpen=true">前往結帳</button></footer>
      </div>
    </aside>
  </div>

  <div v-if="notice" class="toast">✓ {{ notice }}</div>

  <div v-if="authOpen" class="modal" @click.self="authOpen=false"><form class="account-card" @submit.prevent="submitAuth"><button type="button" class="close" @click="authOpen=false">×</button><h2>{{ authMode==='login'?'會員登入':'註冊會員' }}</h2><p class="demo-tip">展示帳號：demo@resellhub.local／Demo123!</p><input v-if="authMode==='register'" v-model="authForm.displayName" placeholder="姓名" required><input v-model="authForm.email" type="email" placeholder="Email" required><input v-if="authMode==='register'" v-model="authForm.phone" placeholder="手機"><input v-model="authForm.password" type="password" placeholder="密碼（至少 8 碼）" required><p v-if="authError" class="cart-error">{{ authError }}</p><button class="account-submit">{{ authMode==='login'?'登入':'建立帳號' }}</button><button type="button" class="switch-auth" @click="authMode=authMode==='login'?'register':'login'">{{ authMode==='login'?'還沒有帳號？立即註冊':'已有帳號？返回登入' }}</button></form></div>

  <div v-if="checkoutOpen" class="modal" @click.self="checkoutOpen=false"><form class="account-card" @submit.prevent="checkout"><button type="button" class="close" @click="checkoutOpen=false">×</button><h2>填寫收件資料</h2><input v-model="checkoutForm.recipientName" placeholder="收件人姓名" required><input v-model="checkoutForm.phone" placeholder="聯絡電話" required><textarea v-model="checkoutForm.address" placeholder="完整收件地址" required></textarea><p>應付金額：<b class="checkout-total">NT$ {{ money(cartData.total) }}</b></p><button class="account-submit">確認建立訂單</button></form></div>
</template>
