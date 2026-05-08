<template>
  <div class="studio-dashboard" :class="`theme-${theme}`">
    <aside class="studio-sidebar">
      <div class="studio-logo">
        <span class="logo-glyph">S</span>
        <strong>SiteForge</strong>
      </div>

      <button class="workspace-select" type="button">
        <span>Default Workspace</span>
        <span class="chevron">⌄</span>
      </button>

      <nav class="studio-nav" aria-label="Studio navigation">
        <p>Studio</p>
        <button class="active" type="button"><span class="nav-icon grid"></span>Projects</button>
        <button type="button"><span class="nav-icon domain"></span>Domains</button>
        <button type="button"><span class="nav-icon rocket"></span>Upgrade Plan</button>
        <button type="button"><span class="nav-icon users"></span>Community</button>
        <button type="button"><span class="nav-icon currency"></span>Refer & Earn</button>
      </nav>

      <div class="share-card">
        <span class="gift-icon"></span>
        <div>
          <strong>Share & Earn</strong>
          <span>0 / 3 rewards used</span>
        </div>
      </div>

      <div class="sidebar-footer">
        <button class="theme-toggle" @click="toggleTheme" type="button" :title="theme === 'dark' ? 'Switch to Light' : 'Switch to Dark'">
          {{ theme === 'dark' ? '☀️' : '🌙' }}
        </button>
        <div class="account-card">
          <span class="avatar">{{ initials }}</span>
          <div>
            <strong>{{ auth.user?.displayName || 'SiteForge User' }}</strong>
            <span>{{ auth.user?.email || 'signed in' }}</span>
          </div>
        </div>
      </div>
    </aside>

    <main class="projects-stage">
      <header class="projects-header">
        <h1>Projects</h1>
        <div class="header-controls">
          <span v-if="auth.user" class="signed-user">{{ auth.user.displayName || auth.user.email }}</span>
          <button @click="auth.logout" class="studio-button subtle" type="button">登出</button>
        </div>
      </header>

      <section class="projects-toolbar" aria-label="Project filters">
        <button class="filter-button" type="button">All projects <span>⌄</span></button>
        <button class="filter-button" type="button">All statuses <span>⌄</span></button>
        <button class="filter-button compact" type="button">Sort by <span>⌄</span></button>
        <label class="project-search">
          <span class="visually-hidden">搜尋專案</span>
          <input v-model="search" type="search" placeholder="Search..." />
        </label>
        <button @click="showAiCreate = true" class="studio-button ai-create" type="button">
          AI generate
        </button>
        <button @click="showCreate = true" class="studio-button primary create-project" type="button">
          <span class="plus-mark">+</span>
          Create new project
        </button>
      </section>

      <section class="status-strip" aria-label="Project status summary">
        <div>
          <span>全部專案</span>
          <strong>{{ sites.length }}</strong>
        </div>
        <div>
          <span>已發佈</span>
          <strong>{{ publishedCount }}</strong>
        </div>
        <div>
          <span>草稿</span>
          <strong>{{ draftCount }}</strong>
        </div>
        <div>
          <span>Builder</span>
          <strong>GrapesJS</strong>
        </div>
      </section>

      <div v-if="loading" class="studio-empty">載入專案中...</div>

      <section v-else-if="filteredSites.length > 0" class="project-grid">
        <article v-for="site in filteredSites" :key="site.id" class="project-card">
          <button class="project-preview" type="button" @click="openSite(site.id)" :aria-label="`開啟 ${site.name}`">
            <div class="preview-browser">
              <span></span>
              <span></span>
              <span></span>
            </div>
            <div class="preview-hero" :class="{ published: site.status === 'published' }">
              <small>{{ site.name }}</small>
              <strong>{{ previewHeadline(site) }}</strong>
              <em></em>
            </div>
          </button>

          <div class="project-meta">
            <div>
              <h2>{{ site.name }}</h2>
              <p>{{ relativeDate(site.updatedAt || site.createdAt) }}</p>
            </div>
            <span class="web-dot" :class="{ published: site.status === 'published' }" :title="site.status === 'published' ? 'Published' : 'Draft'"></span>
          </div>

          <p class="project-description">{{ site.description || '尚未填寫專案描述。' }}</p>

          <div class="project-actions">
            <button @click="openSite(site.id)" class="icon-button" type="button" aria-label="編輯專案">
              <span class="pencil-icon"></span>
            </button>
            <button @click="deleteSite(site.id)" class="icon-button" type="button" aria-label="刪除專案">
              <span class="trash-icon"></span>
            </button>
            <span class="project-status" :class="{ published: site.status === 'published' }">
              {{ statusLabel(site.status) }}
            </span>
          </div>
        </article>
      </section>

      <div v-else class="studio-empty">
        <p class="sf-kicker">No projects yet</p>
        <h2>建立第一個 SiteForge 專案</h2>
        <p>會自動建立 Home 頁，接著可進入 Studio 編輯器管理頁面、區塊、樣式與發佈。</p>
        <button @click="showCreate = true" class="studio-button primary" type="button">Create new project</button>
      </div>

      <button class="chat-fab" type="button" aria-label="Help chat"></button>
    </main>

    <div v-if="showCreate" class="sf-modal-overlay" @click.self="showCreate = false">
      <div class="sf-modal create-modal">
        <p class="sf-kicker">New project</p>
        <h3>建立新網站</h3>
        <form @submit.prevent="createSite">
          <label>
            網站名稱
            <input v-model="newSite.name" class="sf-input" placeholder="例如：Pebisnis Ulung" required />
          </label>
          <label>
            描述
            <input v-model="newSite.description" class="sf-input" placeholder="描述網站用途或客戶產業" />
          </label>
          <div class="modal-actions">
            <button type="button" @click="showCreate = false" class="sf-button">取消</button>
            <button type="submit" class="sf-button primary" :disabled="creating">
              {{ creating ? '建立中...' : '建立並進入工作區' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="showAiCreate" class="sf-modal-overlay" @click.self="showAiCreate = false">
      <div class="sf-modal ai-modal">
        <p class="sf-kicker">AI website generator</p>
        <h3>用 AI 產生完整網站</h3>
        <form @submit.prevent="generateSite">
          <label>
            網站名稱
            <input v-model="aiSite.siteName" class="sf-input" placeholder="例如：SynapseAI" required />
          </label>
          <label>
            網站需求
            <textarea
              v-model="aiSite.prompt"
              class="sf-input"
              placeholder="描述品牌、產業、目標客戶、想呈現的頁面內容與風格"
              required
            ></textarea>
          </label>
          <div class="ai-options">
            <label>
              風格
              <select v-model="aiSite.style" class="sf-input">
                <option value="studio">Studio</option>
                <option value="tech">Tech</option>
                <option value="premium">Premium</option>
                <option value="eco">Eco</option>
                <option value="fashion">Fashion</option>
              </select>
            </label>
            <label>
              內容長度
              <select v-model="aiSite.contentLength" class="sf-input">
                <option value="concise">精簡</option>
                <option value="medium">中等</option>
                <option value="long">詳細</option>
              </select>
            </label>
          </div>
          <div class="page-type-picks">
            <label v-for="type in aiPageTypes" :key="type.value">
              <input v-model="aiSite.pageTypes" type="checkbox" :value="type.value" />
              <span>{{ type.label }}</span>
            </label>
          </div>
          <div class="modal-actions">
            <button type="button" @click="showAiCreate = false" class="sf-button">取消</button>
            <button type="submit" class="sf-button primary" :disabled="generatingSite">
              {{ generatingSite ? 'AI 生成中...' : '生成網站並進入工作區' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useThemeStore } from '../stores/theme'
import api, { errorMessage, unwrap } from '../api/client'

const router = useRouter()
const auth = useAuthStore()
const themeStore = useThemeStore()
const theme = computed(() => themeStore.theme)
const sites = ref([])
const showCreate = ref(false)
const showAiCreate = ref(false)
const loading = ref(false)
const creating = ref(false)
const generatingSite = ref(false)
const search = ref('')
const newSite = ref({ name: '', description: '' })
const aiPageTypes = [
  { value: 'home', label: 'Home' },
  { value: 'about', label: 'About' },
  { value: 'services', label: 'Services' },
  { value: 'product', label: 'Products' },
  { value: 'contact', label: 'Contact' }
]
const aiSite = ref({
  siteName: '',
  prompt: '',
  style: 'studio',
  contentLength: 'medium',
  pageTypes: ['home', 'about', 'services', 'contact']
})

const publishedCount = computed(() => sites.value.filter((site) => site.status === 'published').length)
const draftCount = computed(() => sites.value.length - publishedCount.value)
const initials = computed(() => {
  const name = auth.user?.displayName || auth.user?.email || 'SF'
  return name.slice(0, 2).toUpperCase()
})
const filteredSites = computed(() => {
  const query = search.value.trim().toLowerCase()
  if (!query) return sites.value
  return sites.value.filter((site) => {
    return [site.name, site.slug, site.description].some((value) => value?.toLowerCase().includes(query))
  })
})

onMounted(loadSites)

function toggleTheme() {
  themeStore.toggle()
}

async function loadSites() {
  loading.value = true
  try {
    const response = await api.get('/Sites')
    const sitesData = unwrap(response) || []
    sites.value = Array.isArray(sitesData) ? sitesData : []
  } catch {
    sites.value = []
  } finally {
    loading.value = false
  }
}

async function createSite() {
  creating.value = true
  try {
    const response = await api.post('/Sites', newSite.value)
    const created = unwrap(response)
    newSite.value = { name: '', description: '' }
    showCreate.value = false
    await loadSites()
    if (created?.id) router.push(`/sites/${created.id}`)
  } catch (e) {
    alert(errorMessage(e, '建立失敗'))
  } finally {
    creating.value = false
  }
}

async function generateSite() {
  generatingSite.value = true
  try {
    const response = await api.post('/AiConversations/generate-site', aiSite.value)
    const generated = unwrap(response)
    showAiCreate.value = false
    aiSite.value = {
      siteName: '',
      prompt: '',
      style: 'studio',
      contentLength: 'medium',
      pageTypes: ['home', 'about', 'services', 'contact']
    }
    await loadSites()
    if (generated?.siteId) router.push(`/sites/${generated.siteId}`)
  } catch (e) {
    alert(errorMessage(e, 'AI 生成網站失敗'))
  } finally {
    generatingSite.value = false
  }
}

function openSite(siteId) {
  router.push(`/sites/${siteId}`)
}

async function deleteSite(siteId) {
  if (!confirm('確定要刪除這個網站嗎？')) return
  try {
    await api.delete(`/Sites/${siteId}`)
    await loadSites()
  } catch (e) {
    alert(errorMessage(e, '刪除失敗'))
  }
}

function statusLabel(status) {
  return status === 'published' ? '已發佈' : '草稿'
}

function previewHeadline(site) {
  if (site.description) return site.description.slice(0, 42)
  return site.status === 'published' ? 'Published landing page' : 'Draft website project'
}

function relativeDate(value) {
  if (!value) return '尚未更新'
  const date = new Date(value)
  const days = Math.max(0, Math.floor((Date.now() - date.getTime()) / 86400000))
  if (days === 0) return 'today'
  if (days === 1) return '1 day ago'
  if (days < 30) return `${days} days ago`
  return new Intl.DateTimeFormat('zh-TW', { month: 'short', day: 'numeric' }).format(date)
}
</script>

<style scoped>
/* ── Dashboard layout ── */
.studio-dashboard {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 330px minmax(0, 1fr);
  background: var(--sf-page);
  color: var(--sf-ink);
  transition: background 280ms ease, color 280ms ease;
}

/* ── Sidebar ── */
.studio-sidebar {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  gap: 24px;
  border-right: 1px solid var(--sf-sidebar-line);
  padding: 28px;
  background: var(--sf-sidebar-bg);
  transition: background 280ms ease, border-color 280ms ease;
}

.studio-logo {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 30px;
  line-height: 1;
}

.logo-glyph {
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  border: 3px solid var(--sf-ink);
  border-radius: 8px;
  color: var(--sf-ink);
  font-size: 20px;
  font-weight: 900;
  transition: border-color 280ms ease, color 280ms ease;
}

.workspace-select,
.filter-button,
.studio-button,
.icon-button {
  border: 1px solid var(--sf-line);
  border-radius: 7px;
  background: var(--sf-surface);
  color: var(--sf-ink);
  cursor: pointer;
  font-weight: 760;
  transition: border-color 160ms ease, background 160ms ease, color 160ms ease;
}

.workspace-select {
  min-height: 48px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  font-size: 16px;
}

.chevron {
  color: var(--sf-muted);
}

/* ── Nav ── */
.studio-nav {
  display: grid;
  gap: 7px;
}

.studio-nav p {
  margin-bottom: 3px;
  color: var(--sf-muted);
  font-size: 13px;
  font-weight: 850;
  text-transform: uppercase;
}

.studio-nav button {
  min-height: 42px;
  display: flex;
  align-items: center;
  gap: 12px;
  border: 0;
  border-radius: 7px;
  background: transparent;
  color: var(--sf-sidebar-text);
  cursor: pointer;
  font-size: 18px;
  font-weight: 820;
  text-align: left;
  transition: color 160ms ease, background 160ms ease;
}

.studio-nav button.active,
.studio-nav button:hover {
  color: var(--sf-sidebar-text-active);
  background: var(--sf-sidebar-highlight);
}

.nav-icon,
.gift-icon,
.pencil-icon,
.trash-icon {
  position: relative;
  width: 22px;
  height: 22px;
  display: inline-block;
  flex: 0 0 auto;
}

.nav-icon.grid {
  background:
    linear-gradient(var(--sf-sidebar-text-active) 0 0) left top / 9px 9px no-repeat,
    linear-gradient(var(--sf-sidebar-text-active) 0 0) right top / 9px 9px no-repeat,
    linear-gradient(var(--sf-sidebar-text-active) 0 0) left bottom / 9px 9px no-repeat,
    linear-gradient(var(--sf-sidebar-text-active) 0 0) right bottom / 9px 9px no-repeat;
  transition: background 280ms ease;
}

.nav-icon.domain {
  border: 2px solid currentColor;
  border-radius: 2px;
  box-shadow: 7px 7px 0 -3px currentColor;
}

.nav-icon.rocket::before,
.nav-icon.users::before,
.nav-icon.currency::before {
  content: "";
  position: absolute;
  inset: 4px;
  border: 2px solid currentColor;
  border-radius: 999px;
}

.nav-icon.currency::after {
  content: "$";
  position: absolute;
  inset: 0;
  display: grid;
  place-items: center;
  font-weight: 900;
}

.share-card {
  display: flex;
  align-items: center;
  gap: 14px;
  border: 1px solid var(--sf-line);
  border-radius: 7px;
  padding: 14px;
  background: var(--sf-surface);
  margin-top: auto;
  transition: border-color 280ms ease, background 280ms ease;
}

.share-card strong,
.share-card span {
  display: block;
}

.share-card strong {
  color: var(--sf-ink);
  font-size: 16px;
}

.share-card span {
  color: var(--sf-muted);
  font-size: 13px;
}

.gift-icon {
  width: 52px;
  height: 52px;
  border: 3px solid var(--sf-line);
  border-radius: 50%;
  position: relative;
  transition: border-color 280ms ease;
}

.gift-icon::before,
.gift-icon::after {
  content: "";
  position: absolute;
  background: var(--sf-muted);
  transition: background 280ms ease;
}

.gift-icon::before {
  width: 24px;
  height: 17px;
  left: 14px;
  top: 20px;
  border-radius: 3px;
}

.gift-icon::after {
  width: 4px;
  height: 27px;
  left: 24px;
  top: 13px;
}

/* ── Sidebar footer ── */
.sidebar-footer {
  display: flex;
  align-items: center;
  gap: 10px;
}

.account-card {
  display: flex;
  align-items: center;
  gap: 14px;
  flex: 1;
  border: 1px solid var(--sf-line);
  border-radius: 7px;
  padding: 14px;
  background: var(--sf-surface);
  min-width: 0;
  transition: border-color 280ms ease, background 280ms ease;
}

.account-card strong,
.account-card span {
  display: block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.account-card strong {
  color: var(--sf-ink);
  font-size: 16px;
}

.account-card span {
  color: var(--sf-muted);
  font-size: 13px;
}

.avatar {
  width: 38px;
  height: 38px;
  display: grid;
  flex-shrink: 0;
  place-items: center;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--sf-primary), var(--sf-accent));
  color: #fff;
  font-size: 13px;
  font-weight: 900;
}

/* ── Main stage ── */
.projects-stage {
  position: relative;
  min-width: 0;
  padding: 0 28px 60px;
}

.projects-header {
  min-height: 74px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--sf-line);
  transition: border-color 280ms ease;
}

.projects-header h1 {
  font-size: 26px;
  font-weight: 780;
}

.header-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

.signed-user {
  color: var(--sf-muted);
  font-weight: 700;
}

.projects-toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 30px 0 20px;
  flex-wrap: wrap;
}

.filter-button {
  min-width: 150px;
  min-height: 48px;
  display: inline-flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 0 18px;
  font-size: 16px;
}

.filter-button.compact {
  min-width: 120px;
}

.project-search input {
  width: 264px;
  min-height: 48px;
  border: 1px solid var(--sf-line);
  border-radius: 7px;
  background: var(--sf-surface);
  color: var(--sf-ink);
  padding: 0 18px;
  font-size: 16px;
  transition: border-color 160ms ease, background 160ms ease, color 160ms ease;
}

.project-search input::placeholder {
  color: var(--sf-soft);
}

.studio-button {
  min-height: 40px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 9px;
  padding: 0 15px;
  text-decoration: none;
}

.studio-button.primary {
  border-color: var(--sf-primary);
  background: var(--sf-primary);
  color: white;
  transition: background 280ms ease, border-color 280ms ease;
}

.theme-dark .studio-button.primary {
  color: #101828;
}

.studio-button.ai-create {
  border-color: rgba(252, 84, 158, 0.6);
  background: rgba(252, 84, 158, 0.12);
  color: var(--sf-accent);
}

.studio-button.subtle {
  color: var(--sf-muted);
}

.create-project {
  min-height: 52px;
  margin-left: auto;
  padding: 0 24px;
  font-size: 16px;
}

.plus-mark {
  font-size: 26px;
  line-height: 1;
}

.status-strip {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
  margin-bottom: 20px;
}

.status-strip div {
  border: 1px solid var(--sf-line);
  border-radius: 7px;
  padding: 14px 16px;
  background: var(--sf-surface);
  transition: border-color 280ms ease, background 280ms ease;
}

.status-strip span,
.status-strip strong {
  display: block;
}

.status-strip span {
  color: var(--sf-muted);
  font-size: 12px;
  font-weight: 820;
}

.status-strip strong {
  margin-top: 4px;
  color: var(--sf-ink);
  font-size: 22px;
  line-height: 1.1;
}

/* ── Project cards ── */
.project-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(315px, 1fr));
  gap: 14px;
  align-items: start;
}

.project-card {
  overflow: hidden;
  border: 1px solid var(--sf-card-border);
  border-radius: 7px;
  background: var(--sf-card-bg);
  transition: border-color 280ms ease, background 280ms ease;
}

.project-preview {
  width: 100%;
  min-height: 168px;
  display: block;
  border: 0;
  border-bottom: 1px solid var(--sf-card-border);
  padding: 14px;
  background: var(--sf-stage-bg);
  cursor: pointer;
  text-align: left;
  transition: border-color 280ms ease, background 280ms ease;
}

.preview-browser {
  height: 30px;
  display: flex;
  gap: 6px;
  align-items: center;
  padding: 0 10px;
  border: 1px solid var(--sf-line);
  border-radius: 8px 8px 0 0;
  background: var(--sf-surface);
  transition: border-color 280ms ease, background 280ms ease;
}

.preview-browser span {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--sf-soft);
  transition: background 280ms ease;
}

.preview-hero {
  min-height: 126px;
  padding: 18px;
  border: 1px solid var(--sf-line);
  border-top: 0;
  border-radius: 0 0 8px 8px;
  background:
    radial-gradient(circle at 78% 34%, rgba(131, 88, 237, 0.2), transparent 28%),
    linear-gradient(135deg, #101713, #050706 68%);
  transition: border-color 280ms ease;
}

.preview-hero.published {
  background:
    radial-gradient(circle at 76% 36%, rgba(252, 84, 158, 0.18), transparent 30%),
    linear-gradient(135deg, #fff7fa, #ffffff 66%);
}

.theme-dark .preview-hero {
  background:
    radial-gradient(circle at 78% 34%, rgba(168, 136, 255, 0.25), transparent 28%),
    linear-gradient(135deg, #101713, #050706 68%);
}

.theme-dark .preview-hero.published {
  background:
    radial-gradient(circle at 76% 36%, rgba(252, 84, 158, 0.22), transparent 30%),
    linear-gradient(135deg, #1a0d14, #0d080b 66%);
}

.preview-hero small,
.preview-hero strong {
  display: block;
}

.preview-hero small {
  color: #54d7a0;
  font-size: 9px;
  font-weight: 900;
}

.preview-hero.published small {
  color: #fc549e;
}

.preview-hero strong {
  max-width: 230px;
  margin-top: 22px;
  color: #f5f6f8;
  font-size: 22px;
  line-height: 1.15;
}

.preview-hero.published strong {
  color: #15161a;
}

.theme-dark .preview-hero strong {
  color: #f5f6f8;
}

.preview-hero em {
  display: block;
  width: 90px;
  height: 7px;
  margin-top: 16px;
  border-radius: 999px;
  background: #42d996;
}

.preview-hero.published em {
  background: #fc668f;
}

.project-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 12px 0;
}

.project-meta h2 {
  color: var(--sf-ink);
  font-size: 18px;
  font-weight: 720;
}

.project-meta p,
.project-description {
  color: var(--sf-muted);
}

.project-description {
  min-height: 44px;
  padding: 7px 12px 0;
  font-size: 13px;
}

.web-dot {
  width: 18px;
  height: 18px;
  border: 2px solid var(--sf-soft);
  border-radius: 50%;
  transition: border-color 280ms ease;
}

.web-dot.published {
  border-color: var(--sf-success);
  background: var(--sf-success);
}

.project-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  justify-content: flex-end;
  padding: 10px 12px;
}

.icon-button {
  width: 38px;
  height: 38px;
  display: grid;
  place-items: center;
}

.pencil-icon::before {
  content: "";
  position: absolute;
  width: 16px;
  height: 4px;
  left: 3px;
  top: 9px;
  border-radius: 2px;
  background: var(--sf-muted);
  transform: rotate(-42deg);
  transition: background 280ms ease;
}

.trash-icon::before,
.trash-icon::after {
  content: "";
  position: absolute;
  left: 5px;
  background: var(--sf-muted);
  transition: background 280ms ease;
}

.trash-icon::before {
  width: 13px;
  height: 14px;
  top: 7px;
  border-radius: 2px;
}

.trash-icon::after {
  width: 17px;
  height: 3px;
  top: 3px;
  border-radius: 2px;
}

.project-status {
  margin-left: auto;
  color: var(--sf-warning);
  font-size: 12px;
  font-weight: 850;
}

.project-status.published {
  color: var(--sf-success);
}

.studio-empty {
  max-width: 520px;
  border: 1px solid var(--sf-card-border);
  border-radius: 7px;
  padding: 36px;
  background: var(--sf-card-bg);
  color: var(--sf-muted);
  transition: border-color 280ms ease, background 280ms ease;
}

.studio-empty h2 {
  margin: 8px 0;
  color: var(--sf-ink);
}

.studio-empty p:not(.sf-kicker) {
  margin-bottom: 18px;
}

.chat-fab {
  position: fixed;
  right: 30px;
  bottom: 28px;
  width: 70px;
  height: 70px;
  border: 0;
  border-radius: 50%;
  background: var(--sf-primary);
  box-shadow: var(--sf-shadow);
  cursor: pointer;
  transition: background 280ms ease, box-shadow 280ms ease;
}

.chat-fab::before,
.chat-fab::after {
  content: "";
  position: absolute;
  background: white;
}

.chat-fab::before {
  width: 31px;
  height: 24px;
  left: 20px;
  top: 22px;
  border-radius: 999px;
}

.chat-fab::after {
  width: 10px;
  height: 10px;
  left: 39px;
  top: 38px;
  transform: rotate(45deg);
}

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
}

/* ── Modal ── */
.create-modal h3,
.ai-modal h3 {
  margin: 6px 0 18px;
  font-size: 26px;
  color: var(--sf-ink);
}

.create-modal form,
.ai-modal form {
  display: grid;
  gap: 15px;
}

.create-modal label,
.ai-modal label {
  font-weight: 750;
  color: var(--sf-ink);
}

.create-modal input,
.ai-modal input,
.ai-modal textarea,
.ai-modal select {
  margin-top: 7px;
}

.ai-modal {
  width: min(680px, 100%);
}

.ai-modal textarea {
  min-height: 130px;
  resize: vertical;
}

.ai-options {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.page-type-picks {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(118px, 1fr));
  gap: 8px;
}

.page-type-picks label {
  min-height: 40px;
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid var(--sf-line);
  border-radius: 7px;
  padding: 0 10px;
  background: var(--sf-surface-2);
  color: var(--sf-ink);
  transition: border-color 280ms ease, background 280ms ease, color 280ms ease;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 4px;
}

/* ── Responsive ── */
@media (max-width: 1080px) {
  .studio-dashboard {
    grid-template-columns: 1fr;
  }
  .studio-sidebar {
    min-height: auto;
  }
  .share-card,
  .account-card {
    display: none;
  }
}

@media (max-width: 760px) {
  .studio-sidebar,
  .projects-stage {
    padding: 18px;
  }
  .projects-header,
  .projects-toolbar {
    align-items: stretch;
    flex-direction: column;
  }
  .project-search input,
  .filter-button,
  .create-project {
    width: 100%;
    margin-left: 0;
  }
  .status-strip {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
