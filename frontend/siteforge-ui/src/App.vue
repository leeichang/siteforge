<template>
  <div id="app" :data-theme="theme">
    <!-- Login is full-screen, no app chrome -->
    <template v-if="route.path === '/login'">
      <router-view />
    </template>

    <!-- Editor / SiteWorkspace — own their layout (handled inside each view) -->
    <template v-else-if="isEditorOrWorkspace">
      <router-view />
    </template>

    <!-- Dashboard shell — TopAppBar + side nav + content -->
    <template v-else>
      <!-- TopAppBar -->
      <header class="sf-topbar">
        <div class="sf-topbar-left">
          <button class="sf-topbar-menu-btn" @click="sidebarOpen = !sidebarOpen" aria-label="Toggle menu">
            <span class="material-symbols-outlined">menu</span>
          </button>
          <router-link to="/" class="sf-topbar-brand">
            <span class="material-symbols-outlined material-symbols-filled sf-topbar-brand-icon">architecture</span>
            <span class="sf-topbar-brand-text">SiteForge AI</span>
          </router-link>
        </div>

        <nav class="sf-topbar-nav">
          <router-link to="/" class="sf-topbar-nav-item" :class="{ active: route.path === '/' }">
            <span class="material-symbols-outlined">dashboard</span>
            <span>{{ t('app.dashboard') }}</span>
          </router-link>
          <router-link to="/sites" class="sf-topbar-nav-item" :class="{ active: route.path.startsWith('/sites') }">
            <span class="material-symbols-outlined">dns</span>
            <span>{{ t('app.sites') }}</span>
          </router-link>
          <router-link to="/templates" class="sf-topbar-nav-item" :class="{ active: route.path.startsWith('/templates') }">
            <span class="material-symbols-outlined">template</span>
            <span>{{ t('app.templates') }}</span>
          </router-link>
          <router-link to="/marketplace" class="sf-topbar-nav-item" :class="{ active: route.path.startsWith('/marketplace') }">
            <span class="material-symbols-outlined">store</span>
            <span>{{ t('app.marketplace') }}</span>
          </router-link>
        </nav>

        <div class="sf-topbar-right">
          <button class="sf-topbar-icon-btn" @click="toggleLocale" :title="t('common.language')">
            <span class="sf-locale-badge">{{ locale === 'en' ? '繁' : 'EN' }}</span>
          </button>
          <button class="sf-topbar-icon-btn" @click="toggleTheme" :title="theme === 'dark' ? t('common.switchToLight') : t('common.switchToDark')">
            <span class="material-symbols-outlined">{{ theme === 'dark' ? 'light_mode' : 'dark_mode' }}</span>
          </button>
          <button class="sf-topbar-icon-btn" title="Notifications">
            <span class="material-symbols-outlined">notifications</span>
          </button>
          <button class="sf-topbar-icon-btn" title="Help">
            <span class="material-symbols-outlined">help</span>
          </button>
          <div class="sf-topbar-user">
            <div class="sf-topbar-avatar">{{ initials }}</div>
            <span class="sf-topbar-user-name">{{ auth.user?.displayName || auth.user?.email || 'User' }}</span>
          </div>
        </div>
      </header>

      <!-- Sidebar (collapsible on mobile) -->
      <aside class="sf-sidebar" :class="{ open: sidebarOpen }">
        <div class="sf-sidebar-section">
          <p class="sf-sidebar-label">{{ t('app.mainMenu') }}</p>
          <router-link to="/" class="sf-sidebar-item" :class="{ active: route.path === '/' }">
            <span class="material-symbols-outlined">dashboard</span>
            <span>{{ t('app.dashboard') }}</span>
          </router-link>
          <router-link to="/sites" class="sf-sidebar-item" :class="{ active: route.path.startsWith('/sites') }">
            <span class="material-symbols-outlined">dns</span>
            <span>{{ t('app.sites') }}</span>
          </router-link>
        </div>
        <div class="sf-sidebar-section">
          <p class="sf-sidebar-label">{{ t('app.manage') }}</p>
          <router-link to="/templates" class="sf-sidebar-item" :class="{ active: route.path.startsWith('/templates') }">
            <span class="material-symbols-outlined">template</span>
            <span>{{ t('app.templates') }}</span>
          </router-link>
          <router-link to="/marketplace" class="sf-sidebar-item" :class="{ active: route.path.startsWith('/marketplace') }">
            <span class="material-symbols-outlined">store</span>
            <span>{{ t('app.marketplace') }}</span>
          </router-link>
        </div>
        <div class="sf-sidebar-spacer"></div>
        <div class="sf-sidebar-section">
          <router-link to="/settings" class="sf-sidebar-item">
            <span class="material-symbols-outlined">settings</span>
            <span>{{ t('app.settings') }}</span>
          </router-link>
          <button class="sf-sidebar-item" @click="auth.logout">
            <span class="material-symbols-outlined">logout</span>
            <span>{{ t('common.logout') }}</span>
          </button>
        </div>
      </aside>

      <!-- Backdrop for mobile sidebar -->
      <div v-if="sidebarOpen" class="sf-sidebar-backdrop" @click="sidebarOpen = false"></div>

      <!-- Main Content -->
      <main class="sf-main">
        <router-view />
      </main>
    </template>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from './stores/auth'
import { useThemeStore } from './stores/theme'
import { useLocaleStore } from './stores/locale'

const route = useRoute()
const auth = useAuthStore()
const themeStore = useThemeStore()
const localeStore = useLocaleStore()

const sidebarOpen = ref(false)

const theme = computed(() => themeStore.theme)
const locale = computed(() => localeStore.locale)
const t = localeStore.t

const isEditorOrWorkspace = computed(() =>
  route.path.startsWith('/editor/') || route.path.startsWith('/sites/')
)

const initials = computed(() => {
  const name = auth.user?.displayName || auth.user?.email || 'U'
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

const toggleTheme = () => themeStore.toggle()
const toggleLocale = () => localeStore.toggleLocale()
</script>

<style scoped>
#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

/* ═══════════════════════════════════════
   TopAppBar
   ═══════════════════════════════════════ */
.sf-topbar {
  position: sticky;
  top: 0;
  z-index: 60;
  display: flex;
  align-items: center;
  height: 56px;
  padding: 0 16px;
  background: var(--sf-surface-container-low);
  border-bottom: 1px solid var(--sf-line);
  gap: 8px;
}

.sf-topbar-left {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
}

.sf-topbar-menu-btn {
  display: none;
  background: none;
  border: none;
  color: var(--sf-ink-secondary);
  cursor: pointer;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  align-items: center;
  justify-content: center;
  transition: all 0.15s;
}
.sf-topbar-menu-btn:hover {
  background: var(--sf-chrome-btn-hover-bg);
  color: var(--sf-ink);
}

.sf-topbar-brand {
  display: flex;
  align-items: center;
  gap: 10px;
  text-decoration: none;
  color: var(--sf-ink);
  flex-shrink: 0;
}

.sf-topbar-brand-icon {
  font-size: 28px;
  color: var(--sf-accent);
}

.sf-topbar-brand-text {
  font-size: 18px;
  font-weight: 700;
  letter-spacing: -0.3px;
}

/* Nav in topbar */
.sf-topbar-nav {
  display: flex;
  align-items: center;
  gap: 2px;
  margin-left: 24px;
  flex: 1;
}

.sf-topbar-nav-item {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 8px;
  color: var(--sf-ink-secondary);
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.15s;
  white-space: nowrap;
}
.sf-topbar-nav-item:hover {
  background: var(--sf-chrome-btn-hover-bg);
  color: var(--sf-ink);
}
.sf-topbar-nav-item.active {
  background: var(--sf-accent-soft);
  color: var(--sf-accent);
}
.sf-topbar-nav-item .material-symbols-outlined {
  font-size: 18px;
}

/* Right side */
.sf-topbar-right {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}

.sf-topbar-icon-btn {
  width: 34px;
  height: 34px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: 8px;
  background: none;
  color: var(--sf-ink-secondary);
  cursor: pointer;
  transition: all 0.15s;
}
.sf-topbar-icon-btn:hover {
  background: var(--sf-chrome-btn-hover-bg);
  color: var(--sf-ink);
}
.sf-topbar-icon-btn .material-symbols-outlined {
  font-size: 20px;
}

.sf-locale-badge {
  font-size: 11px;
  font-weight: 700;
  line-height: 20px;
  min-width: 20px;
  text-align: center;
}

.sf-topbar-user {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-left: 8px;
  padding-left: 8px;
  border-left: 1px solid var(--sf-line);
}

.sf-topbar-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--sf-accent-soft);
  color: var(--sf-accent);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  flex-shrink: 0;
}

.sf-topbar-user-name {
  font-size: 13px;
  font-weight: 500;
  color: var(--sf-ink);
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* ═══════════════════════════════════════
   Sidebar
   ═══════════════════════════════════════ */
.sf-sidebar {
  position: fixed;
  left: 0;
  top: 56px;
  bottom: 0;
  width: 240px;
  background: var(--sf-surface-container-low);
  border-right: 1px solid var(--sf-line);
  display: flex;
  flex-direction: column;
  padding: 12px 0;
  z-index: 50;
  transition: transform 0.2s ease;
  overflow-y: auto;
}

.sf-sidebar-section {
  padding: 4px 12px;
}

.sf-sidebar-label {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.8px;
  color: var(--sf-muted);
  padding: 8px 12px 6px;
}

.sf-sidebar-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-radius: 8px;
  color: var(--sf-ink-secondary);
  text-decoration: none;
  cursor: pointer;
  border: none;
  background: none;
  width: 100%;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.15s;
  text-align: left;
}
.sf-sidebar-item:hover {
  background: var(--sf-chrome-btn-hover-bg);
  color: var(--sf-ink);
}
.sf-sidebar-item.active {
  background: var(--sf-accent-soft);
  color: var(--sf-accent);
}
.sf-sidebar-item .material-symbols-outlined {
  font-size: 20px;
}

.sf-sidebar-spacer {
  flex: 1;
}

.sf-sidebar-backdrop {
  display: none;
}

/* ═══════════════════════════════════════
   Main content area
   ═══════════════════════════════════════ */
.sf-main {
  margin-left: 240px;
  flex: 1;
  min-height: calc(100vh - 56px);
  background: var(--sf-bg);
}

/* ═══════════════════════════════════════
   Responsive
   ═══════════════════════════════════════ */
@media (max-width: 900px) {
  .sf-topbar-nav {
    display: none;
  }
  .sf-topbar-menu-btn {
    display: flex;
  }
  .sf-topbar-user-name {
    display: none;
  }
}

@media (max-width: 768px) {
  .sf-sidebar {
    transform: translateX(-100%);
  }
  .sf-sidebar.open {
    transform: translateX(0);
  }
  .sf-sidebar-backdrop {
    display: block;
    position: fixed;
    inset: 0;
    background: rgba(0,0,0,0.4);
    z-index: 49;
  }
  .sf-main {
    margin-left: 0;
  }
}
</style>
