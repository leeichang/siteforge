<template>
  <div id="app" :data-theme="theme">
    <!-- Login page has no sidebar -->
    <template v-if="isLoginPage">
      <router-view />
    </template>
    
    <!-- Main app with sidebar -->
    <template v-else>
      <!-- Side Navigation -->
      <aside class="sf-sidebar">
        <!-- Brand -->
        <div class="sf-brand">
          <span class="material-symbols-outlined material-symbols-filled sf-brand-icon">architecture</span>
          <div>
            <h1 class="sf-headline-sm" style="font-weight: 700; color: var(--sf-on-surface);">SiteForge AI</h1>
            <p class="sf-label-sm" style="color: var(--sf-on-surface-variant);">Pro Plan</p>
          </div>
        </div>
        
        <!-- Navigation -->
        <nav class="sf-nav">
          <router-link 
            to="/" 
            class="sf-nav-item" 
            :class="{ active: $route.path === '/' }"
          >
            <span class="material-symbols-outlined material-symbols-filled">folder</span>
            <span class="sf-label-lg">Project</span>
          </router-link>
          <a href="#" class="sf-nav-item">
            <span class="material-symbols-outlined">layers</span>
            <span class="sf-label-lg">Layers</span>
          </a>
          <a href="#" class="sf-nav-item">
            <span class="material-symbols-outlined">widgets</span>
            <span class="sf-label-lg">Blocks</span>
          </a>
          <a href="#" class="sf-nav-item">
            <span class="material-symbols-outlined">palette</span>
            <span class="sf-label-lg">Styles</span>
          </a>
          <a href="#" class="sf-nav-item">
            <span class="material-symbols-outlined">image</span>
            <span class="sf-label-lg">Assets</span>
          </a>
          <a href="#" class="sf-nav-item">
            <span class="material-symbols-outlined" style="color: var(--sf-primary-fixed-dim);">auto_awesome</span>
            <span class="sf-label-lg">AI Assistant</span>
          </a>
        </nav>
        
        <!-- Footer -->
        <div class="sf-sidebar-footer">
          <button class="sf-btn-primary" @click="$router.push('/workspace')" style="width: 100%; justify-content: center;">
            <span class="material-symbols-outlined material-symbols-filled">add</span>
            New Project
          </button>
          <router-link to="/" class="sf-nav-item" style="margin-top: 8px;">
            <span class="material-symbols-outlined">settings</span>
            <span class="sf-label-lg">Settings</span>
          </router-link>
          <div class="sf-user-card">
            <div class="sf-avatar">{{ initials }}</div>
            <div class="sf-user-info">
              <p class="sf-label-lg" style="color: var(--sf-on-surface);">{{ auth.user?.displayName || 'User' }}</p>
              <p class="sf-label-sm" style="color: var(--sf-on-surface-variant);">{{ auth.user?.email || '' }}</p>
            </div>
            <button @click="toggleTheme" class="sf-theme-btn" :title="theme === 'dark' ? 'Light mode' : 'Dark mode'">
              <span class="material-symbols-outlined">{{ theme === 'dark' ? 'light_mode' : 'dark_mode' }}</span>
            </button>
            <button @click="auth.logout" class="sf-theme-btn" title="Logout">
              <span class="material-symbols-outlined">logout</span>
            </button>
          </div>
        </div>
      </aside>
      
      <!-- Main Content -->
      <main class="sf-main">
        <router-view />
      </main>
    </template>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from './stores/auth'
import { useThemeStore } from './stores/theme'

const route = useRoute()
const auth = useAuthStore()
const themeStore = useThemeStore()

const isLoginPage = computed(() => route.path === '/login')
const theme = computed(() => themeStore.theme)

const initials = computed(() => {
  const name = auth.user?.displayName || auth.user?.email || 'U'
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

const toggleTheme = () => {
  themeStore.toggleTheme()
}
</script>

<style scoped>
#app {
  min-height: 100vh;
  display: flex;
}

.sf-sidebar {
  width: 280px;
  height: 100vh;
  position: fixed;
  left: 0;
  top: 0;
  background: var(--sf-surface-container-low);
  border-right: 1px solid var(--sf-outline-variant);
  display: flex;
  flex-direction: column;
  z-index: 50;
  padding: 16px 0;
}

.sf-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0 24px;
  margin-bottom: 24px;
}

.sf-brand-icon {
  font-size: 32px;
  color: var(--sf-primary);
}

.sf-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 0 8px;
}

.sf-sidebar-footer {
  padding: 0 16px;
  margin-top: auto;
}

.sf-user-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  margin-top: 8px;
  border-radius: 12px;
  background: var(--sf-surface-variant);
  border: 1px solid var(--sf-outline-variant);
}

.sf-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--sf-primary-container);
  color: var(--sf-on-primary-container);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  flex-shrink: 0;
}

.sf-user-info {
  flex: 1;
  min-width: 0;
  overflow: hidden;
}

.sf-user-info p {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sf-theme-btn {
  background: none;
  border: none;
  color: var(--sf-on-surface-variant);
  cursor: pointer;
  padding: 4px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s;
}

.sf-theme-btn:hover {
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface);
}

.sf-main {
  margin-left: 280px;
  flex: 1;
  min-height: 100vh;
  background: var(--sf-bg);
}

/* Responsive */
@media (max-width: 768px) {
  .sf-sidebar {
    width: 64px;
    padding: 12px 0;
  }
  .sf-brand h1, .sf-brand p,
  .sf-nav-item span:not(.material-symbols-outlined),
  .sf-user-info,
  .sf-btn-primary span:last-child {
    display: none;
  }
  .sf-main {
    margin-left: 64px;
  }
  .sf-brand {
    justify-content: center;
    padding: 0;
  }
  .sf-nav-item {
    justify-content: center;
    padding: 12px;
    margin: 0 4px;
    width: calc(100% - 8px);
  }
  .sf-sidebar-footer {
    padding: 0 4px;
  }
  .sf-user-card {
    flex-direction: column;
    gap: 8px;
    padding: 8px;
  }
  .sf-avatar {
    width: 36px;
    height: 36px;
  }
  .sf-btn-primary {
    padding: 10px;
  }
  .sf-btn-primary span:first-child {
    margin: 0;
  }
}
</style>
