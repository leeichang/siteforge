<template>
  <div class="workspace-shell" :class="`theme-${theme}`">
    <header class="workspace-header">
      <div class="header-left">
        <button class="header-back" @click="router.push('/')" type="button">←</button>
        <div class="site-title">
          <p class="sf-kicker">Site workspace</p>
          <h1>{{ site?.name || '載入中' }}</h1>
        </div>
      </div>
      <div class="header-actions">
        <button class="theme-toggle" @click="themeStore.toggle()" type="button" :title="theme === 'dark' ? 'Switch to Light' : 'Switch to Dark'">
          {{ theme === 'dark' ? '☀️' : '🌙' }}
        </button>
        <a v-if="site?.publishedUrl" :href="site.publishedUrl" target="_blank" class="sf-button">查看公開頁</a>
        <button class="sf-button primary" @click="publishSite" :disabled="publishing">
          {{ publishing ? '發佈中...' : '發佈網站' }}
        </button>
      </div>
    </header>

    <main class="workspace-main">
      <aside class="workspace-nav">
        <div class="nav-card">
          <span class="sf-pill" :class="site?.status === 'published' ? 'success' : 'warning'">
            {{ site?.status === 'published' ? '已發佈' : '草稿' }}
          </span>
          <strong>{{ site?.slug ? `/${site.slug}` : '尚未載入' }}</strong>
          <small>{{ site?.description || '尚未填寫網站描述' }}</small>
        </div>
        <button class="nav-item active">頁面</button>
        <button class="nav-item">模板</button>
        <button class="nav-item">主題</button>
        <button class="nav-item">網域</button>
        <button class="nav-item" @click="showAiPage = true">AI 助手</button>
      </aside>

      <section class="workspace-content">
        <section class="workspace-hero">
          <div>
            <p class="sf-kicker">Build flow</p>
            <h2>頁面、模板、發佈都在同一個工作區完成。</h2>
            <p>先管理站點結構，再進入 GrapesJS 編輯器處理內容與樣式，最後產出靜態頁。</p>
          </div>
          <div class="health-grid">
            <div>
              <span>頁面</span>
              <strong>{{ pages.length }}</strong>
            </div>
            <div>
              <span>已發佈</span>
              <strong>{{ publishedPages }}</strong>
            </div>
            <div>
              <span>首頁</span>
              <strong>{{ homePageTitle }}</strong>
            </div>
          </div>
        </section>

        <section class="pages-panel">
          <div class="content-header">
            <div>
              <h2>頁面</h2>
              <p>管理網站導覽與每個頁面的 GrapesJS 內容。</p>
            </div>
            <div class="content-actions">
              <button class="sf-button" @click="showAiPage = true">AI 產生頁面</button>
              <button class="sf-button primary" @click="showCreatePage = true">新增頁面</button>
            </div>
          </div>

          <div v-if="loading" class="muted-panel">載入頁面中...</div>

          <div v-else class="page-table">
            <div class="page-row page-row-head">
              <span>標題</span>
              <span>路徑</span>
              <span>狀態</span>
              <span></span>
            </div>
            <div v-for="page in pages" :key="page.id" class="page-row">
              <div>
                <strong>{{ page.title }}</strong>
                <small v-if="page.isHome">Home page</small>
              </div>
              <span class="page-slug">/{{ page.slug }}</span>
              <span class="sf-pill" :class="page.isPublished ? 'success' : 'warning'">
                {{ page.isPublished ? '已發佈' : '草稿' }}
              </span>
              <div class="row-actions">
                <button class="sf-button primary" @click="openEditor(page.id)">編輯</button>
                <button class="sf-button danger" @click="deletePage(page.id)" :disabled="page.isHome">刪除</button>
              </div>
            </div>
          </div>
        </section>
      </section>
    </main>

    <div v-if="showCreatePage" class="sf-modal-overlay" @click.self="showCreatePage = false">
      <div class="sf-modal page-modal">
        <p class="sf-kicker">New page</p>
        <h3>新增頁面</h3>
        <form @submit.prevent="createPage">
          <label>
            頁面標題
            <input v-model="newPage.title" class="sf-input" required placeholder="例如：產品介紹" />
          </label>
          <label>
            URL Slug
            <input v-model="newPage.slug" class="sf-input" placeholder="例如：products" />
          </label>
          <div class="modal-actions">
            <button type="button" class="sf-button" @click="showCreatePage = false">取消</button>
            <button class="sf-button primary" :disabled="creatingPage">{{ creatingPage ? '建立中...' : '建立頁面' }}</button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="showAiPage" class="sf-modal-overlay" @click.self="showAiPage = false">
      <div class="sf-modal ai-page-modal">
        <p class="sf-kicker">AI page generator</p>
        <h3>用 AI 產生新頁面</h3>
        <form @submit.prevent="generatePage">
          <div class="ai-page-grid">
            <label>
              頁面名稱
              <input v-model="aiPage.pageName" class="sf-input" required placeholder="例如：服務方案" />
            </label>
            <label>
              頁面類型
              <select v-model="aiPage.pageType" class="sf-input">
                <option value="home">Home</option>
                <option value="about">About</option>
                <option value="services">Services</option>
                <option value="product">Products</option>
                <option value="portfolio">Portfolio</option>
                <option value="blog">Blog</option>
                <option value="contact">Contact</option>
              </select>
            </label>
          </div>
          <label>
            生成需求
            <textarea
              v-model="aiPage.prompt"
              class="sf-input"
              required
              placeholder="描述這頁要說什麼、面向誰、需要哪些區塊與 CTA"
            ></textarea>
          </label>
          <div class="ai-page-grid">
            <label>
              風格
              <select v-model="aiPage.style" class="sf-input">
                <option value="studio">Studio</option>
                <option value="tech">Tech</option>
                <option value="premium">Premium</option>
                <option value="eco">Eco</option>
                <option value="fashion">Fashion</option>
              </select>
            </label>
            <label>
              內容長度
              <select v-model="aiPage.contentLength" class="sf-input">
                <option value="concise">精簡</option>
                <option value="medium">中等</option>
                <option value="long">詳細</option>
              </select>
            </label>
          </div>
          <div class="modal-actions">
            <button type="button" class="sf-button" @click="showAiPage = false">取消</button>
            <button class="sf-button primary" :disabled="generatingPage">
              {{ generatingPage ? 'AI 生成中...' : '生成並打開編輯器' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useThemeStore } from '../stores/theme'
import api, { errorMessage, unwrap } from '../api/client'

const route = useRoute()
const router = useRouter()
const themeStore = useThemeStore()
const theme = computed(() => themeStore.theme)
const siteId = route.params.siteId

const site = ref(null)
const pages = ref([])
const loading = ref(false)
const publishing = ref(false)
const creatingPage = ref(false)
const generatingPage = ref(false)
const showCreatePage = ref(false)
const showAiPage = ref(false)
const newPage = ref({ title: '', slug: '' })
const aiPage = ref({
  pageName: '',
  pageType: 'services',
  prompt: '',
  style: 'studio',
  contentLength: 'medium'
})

const publishedPages = computed(() => pages.value.filter((page) => page.isPublished).length)
const homePageTitle = computed(() => pages.value.find((page) => page.isHome)?.title || '未設定')

onMounted(loadWorkspace)

async function loadWorkspace() {
  loading.value = true
  try {
    const [siteResponse, pagesResponse] = await Promise.all([
      api.get(`/Sites/${siteId}`),
      api.get(`/Pages/site/${siteId}`)
    ])
    site.value = unwrap(siteResponse)
    pages.value = (unwrap(pagesResponse) || []).sort((a, b) => a.displayOrder - b.displayOrder)
  } catch (e) {
    alert(errorMessage(e, '載入網站失敗'))
  } finally {
    loading.value = false
  }
}

async function createPage() {
  creatingPage.value = true
  try {
    const response = await api.post(`/Pages/site/${siteId}`, {
      title: newPage.value.title,
      slug: newPage.value.slug,
      pageType: 'custom',
      isHome: false
    })
    const page = unwrap(response)
    showCreatePage.value = false
    newPage.value = { title: '', slug: '' }
    await loadWorkspace()
    if (page?.id) openEditor(page.id)
  } catch (e) {
    alert(errorMessage(e, '建立頁面失敗'))
  } finally {
    creatingPage.value = false
  }
}

async function generatePage() {
  generatingPage.value = true
  try {
    const response = await api.post('/AiConversations/generate-page', {
      siteId,
      ...aiPage.value
    })
    const generated = unwrap(response)
    showAiPage.value = false
    aiPage.value = {
      pageName: '',
      pageType: 'services',
      prompt: '',
      style: 'studio',
      contentLength: 'medium'
    }
    await loadWorkspace()
    if (generated?.pageId) openEditor(generated.pageId)
  } catch (e) {
    alert(errorMessage(e, 'AI 生成頁面失敗'))
  } finally {
    generatingPage.value = false
  }
}

async function deletePage(pageId) {
  if (!confirm('確定要刪除這個頁面嗎？')) return
  try {
    await api.delete(`/Pages/${pageId}`)
    await loadWorkspace()
  } catch (e) {
    alert(errorMessage(e, '刪除頁面失敗'))
  }
}

async function publishSite() {
  publishing.value = true
  try {
    await api.post(`/Sites/${siteId}/publish`, { taskType: 'full_publish', targetUrl: '' })
    await loadWorkspace()
    alert('網站已發佈。')
  } catch (e) {
    alert(errorMessage(e, '發佈失敗'))
  } finally {
    publishing.value = false
  }
}

function openEditor(pageId) {
  router.push(`/editor/${siteId}/${pageId}`)
}
</script>

<style scoped>
.workspace-shell {
  min-height: 100vh;
  background: var(--sf-page);
  color: var(--sf-ink);
  transition: background 280ms ease, color 280ms ease;
}

/* ── Header ── */
.workspace-header {
  min-height: 78px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 16px 28px;
  border-bottom: 1px solid var(--sf-header-line);
  background: var(--sf-header-bg);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  transition: border-color 280ms ease, background 280ms ease;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}

.header-back {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border: 1px solid var(--sf-line);
  border-radius: var(--sf-radius-sm);
  background: var(--sf-surface);
  color: var(--sf-ink);
  cursor: pointer;
  font-size: 18px;
  transition: border-color 160ms ease, background 160ms ease;
}

.header-back:hover {
  border-color: var(--sf-line-strong);
  background: var(--sf-surface-hover);
}

.site-title h1 {
  margin-top: 3px;
  font-size: 25px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

/* ── Main layout ── */
.workspace-main {
  display: grid;
  grid-template-columns: 260px 1fr;
  min-height: calc(100vh - 78px);
}

/* ── Sidebar nav ── */
.workspace-nav {
  padding: 22px 16px;
  background: var(--sf-sidebar-bg);
  border-right: 1px solid var(--sf-sidebar-line);
  transition: background 280ms ease, border-color 280ms ease;
}

.nav-card {
  display: grid;
  gap: 10px;
  margin-bottom: 20px;
  border: 1px solid var(--sf-line);
  border-radius: 10px;
  padding: 15px;
  background: var(--sf-surface);
  transition: border-color 280ms ease, background 280ms ease;
}

.nav-card strong {
  overflow-wrap: anywhere;
  color: var(--sf-ink);
}

.nav-card small {
  color: var(--sf-muted);
  line-height: 1.45;
}

.nav-item {
  width: 100%;
  min-height: 42px;
  border: 0;
  border-radius: 8px;
  margin-bottom: 6px;
  padding: 0 12px;
  background: transparent;
  color: var(--sf-sidebar-text);
  cursor: pointer;
  font-weight: 750;
  text-align: left;
  transition: color 160ms ease, background 160ms ease;
}

.nav-item.active,
.nav-item:hover {
  background: var(--sf-sidebar-highlight);
  color: var(--sf-sidebar-text-active);
}

/* ── Content area ── */
.workspace-content {
  padding: 28px;
}

.workspace-hero,
.pages-panel {
  border: 1px solid var(--sf-card-border);
  border-radius: 10px;
  background: var(--sf-card-bg);
  box-shadow: var(--sf-shadow-soft);
  transition: border-color 280ms ease, background 280ms ease, box-shadow 280ms ease;
}

.workspace-hero {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(360px, 0.72fr);
  gap: 24px;
  margin-bottom: 22px;
  padding: 24px;
  background: var(--sf-surface);
  background-image: linear-gradient(135deg, var(--sf-primary-soft), var(--sf-accent-soft));
}

.workspace-hero h2 {
  max-width: 760px;
  margin: 7px 0 10px;
  font-size: clamp(24px, 3vw, 36px);
  line-height: 1.12;
  color: var(--sf-ink);
}

.workspace-hero p:not(.sf-kicker) {
  max-width: 680px;
  color: var(--sf-muted);
}

.health-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}

.health-grid div {
  min-width: 0;
  border: 1px solid var(--sf-line);
  border-radius: 8px;
  padding: 15px;
  background: var(--sf-surface);
  transition: border-color 280ms ease, background 280ms ease;
}

.health-grid span {
  display: block;
  color: var(--sf-muted);
  font-size: 13px;
  font-weight: 750;
}

.health-grid strong {
  display: block;
  margin-top: 5px;
  overflow-wrap: anywhere;
  font-size: 22px;
  line-height: 1.1;
  color: var(--sf-ink);
}

/* ── Pages table ── */
.pages-panel {
  padding: 22px;
}

.content-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 18px;
  margin-bottom: 18px;
}

.content-header h2 {
  font-size: 24px;
  color: var(--sf-ink);
}

.content-header p,
.muted-panel {
  color: var(--sf-muted);
}

.content-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.page-table {
  border: 1px solid var(--sf-line);
  border-radius: 8px;
  overflow: hidden;
  background: var(--sf-surface);
  transition: border-color 280ms ease, background 280ms ease;
}

.page-row {
  display: grid;
  grid-template-columns: minmax(180px, 1.3fr) minmax(140px, 1fr) 120px 190px;
  gap: 16px;
  align-items: center;
  padding: 15px 18px;
  border-top: 1px solid var(--sf-line);
  transition: border-color 280ms ease;
}

.page-row:first-child {
  border-top: 0;
}

.page-row-head {
  background: var(--sf-surface-2);
  color: var(--sf-muted);
  font-size: 13px;
  font-weight: 850;
}

.page-row strong {
  display: block;
  color: var(--sf-ink);
}

.page-row small {
  display: inline-block;
  margin-top: 4px;
  color: var(--sf-primary);
  font-weight: 800;
}

.page-slug {
  color: var(--sf-muted);
  overflow-wrap: anywhere;
}

.row-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

/* ── Modal ── */
.page-modal h3,
.ai-page-modal h3 {
  margin: 6px 0 18px;
  font-size: 26px;
  color: var(--sf-ink);
}

.page-modal form,
.ai-page-modal form {
  display: grid;
  gap: 15px;
}

.page-modal label,
.ai-page-modal label {
  font-weight: 750;
  color: var(--sf-ink);
}

.page-modal input,
.ai-page-modal input,
.ai-page-modal select,
.ai-page-modal textarea {
  margin-top: 7px;
}

.ai-page-modal {
  width: min(680px, 100%);
}

.ai-page-modal textarea {
  min-height: 140px;
  resize: vertical;
}

.ai-page-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 4px;
}

/* ── Responsive ── */
@media (max-width: 1100px) {
  .workspace-hero {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 840px) {
  .workspace-header,
  .workspace-main {
    grid-template-columns: 1fr;
  }
  .workspace-nav {
    display: grid;
    grid-template-columns: repeat(5, minmax(96px, 1fr));
    gap: 8px;
    overflow-x: auto;
  }
  .nav-card {
    grid-column: 1 / -1;
  }
  .workspace-content {
    padding: 18px;
  }
  .health-grid {
    grid-template-columns: 1fr;
  }
  .page-row {
    grid-template-columns: 1fr;
  }
  .row-actions,
  .content-actions,
  .content-header,
  .header-actions,
  .modal-actions {
    align-items: stretch;
    flex-direction: column;
  }
  .ai-page-grid {
    grid-template-columns: 1fr;
  }
}
</style>
