<template>
  <main class="sf-login">
    <section class="sf-login-preview">
      <div class="sf-login-brand">
        <span class="material-symbols-outlined material-symbols-filled" style="font-size: 40px; color: var(--sf-primary);">architecture</span>
        <div>
          <p style="font-size: 12px; font-weight: 500; letter-spacing: 0.5px; text-transform: uppercase; color: var(--sf-on-surface-variant);">SiteForge</p>
          <h1 style="font-size: 32px; font-weight: 700; line-height: 40px; color: var(--sf-on-surface); margin-top: 4px;">{{ t('login.headline') }}</h1>
        </div>
      </div>

      <div class="sf-login-visual">
        <div class="sf-login-browser">
          <div class="sf-browser-bar">
            <span></span><span></span><span></span>
          </div>
          <div class="sf-browser-body">
            <div class="sf-browser-sidebar">
              <span class="sf-browser-active"></span>
              <span></span><span></span><span></span>
            </div>
            <div class="sf-browser-canvas">
              <div class="sf-browser-hero">
                <small style="color: var(--sf-on-surface-variant);">{{ t('login.productLaunch') }}</small>
                <h3 style="font-size: 18px; font-weight: 600; margin: 8px 0; color: var(--sf-on-surface);">{{ t('login.previewTitle') }}</h3>
                <p style="font-size: 13px; color: var(--sf-on-surface-variant);">{{ t('login.previewBody') }}</p>
              </div>
              <div class="sf-browser-grid">
                <div></div><div></div><div></div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="sf-login-stats">
        <div>
          <strong>7</strong>
          <span>{{ t('login.systemBlocks') }}</span>
        </div>
        <div>
          <strong>12+</strong>
          <span>{{ t('login.templates') }}</span>
        </div>
        <div>
          <strong>GPT</strong>
          <span>{{ t('login.copywriting') }}</span>
        </div>
      </div>
    </section>

    <section class="sf-login-form-wrap">
      <div class="sf-login-card">
        <button class="sf-login-locale" type="button" @click="localeStore.toggleLocale()">
          {{ locale === 'en' ? '繁中' : 'English' }}
        </button>
        <div class="sf-login-tabs">
          <button 
            :class="['sf-login-tab', { active: mode === 'login' }]" 
            @click="mode = 'login'"
          >
            {{ t('login.login') }}
          </button>
          <button 
            :class="['sf-login-tab', { active: mode === 'register' }]" 
            @click="mode = 'register'"
          >
            {{ t('login.register') }}
          </button>
        </div>

        <form @submit.prevent="handleSubmit">
          <div class="sf-login-field">
            <label class="sf-label-sm" style="color: var(--sf-on-surface-variant); display: block; margin-bottom: 8px;">Email</label>
            <div class="sf-login-input-wrap">
              <span class="material-symbols-outlined sf-login-input-icon">mail</span>
              <input 
                v-model="form.email" 
                type="email" 
                class="sf-input sf-login-input" 
                placeholder="you@example.com"
                required
              />
            </div>
          </div>

          <div class="sf-login-field" style="margin-top: 16px;">
            <label class="sf-label-sm" style="color: var(--sf-on-surface-variant); display: block; margin-bottom: 8px;">{{ t('login.password') }}</label>
            <div class="sf-login-input-wrap">
              <span class="material-symbols-outlined sf-login-input-icon">lock</span>
              <input 
                v-model="form.password" 
                :type="showPassword ? 'text' : 'password'" 
                class="sf-input sf-login-input" 
                :placeholder="t('login.passwordPlaceholder')"
                required
              />
              <button type="button" class="sf-login-eye" @click="showPassword = !showPassword">
                <span class="material-symbols-outlined">{{ showPassword ? 'visibility_off' : 'visibility' }}</span>
              </button>
            </div>
          </div>

          <div v-if="mode === 'register'" class="sf-login-field" style="margin-top: 16px;">
            <label class="sf-label-sm" style="color: var(--sf-on-surface-variant); display: block; margin-bottom: 8px;">{{ t('login.displayName') }}</label>
            <div class="sf-login-input-wrap">
              <span class="material-symbols-outlined sf-login-input-icon">person</span>
              <input 
                v-model="form.displayName" 
                type="text" 
                class="sf-input sf-login-input" 
                :placeholder="t('login.namePlaceholder')"
              />
            </div>
          </div>

          <button type="submit" class="sf-btn-primary sf-login-submit" :disabled="loading">
            <span v-if="loading" class="material-symbols-outlined" style="animation: spin 1s linear infinite;">progress_activity</span>
            <span>{{ mode === 'login' ? t('login.login') : t('login.register') }}</span>
          </button>
        </form>

        <p v-if="error" class="sf-login-error">{{ error }}</p>

        <p class="sf-login-hint">
          {{ mode === 'login' ? t('login.noAccount') : t('login.hasAccount') }}
          <a href="#" @click.prevent="mode = mode === 'login' ? 'register' : 'login'">
            {{ mode === 'login' ? t('login.createAccount') : t('login.signInNow') }}
          </a>
        </p>
      </div>
    </section>
  </main>
</template>

<script setup>
import { computed, ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useLocaleStore } from '../stores/locale'

const router = useRouter()
const auth = useAuthStore()
const localeStore = useLocaleStore()
const locale = computed(() => localeStore.locale)
const t = localeStore.t

const mode = ref('login')
const loading = ref(false)
const error = ref('')
const showPassword = ref(false)

const form = reactive({
  email: '',
  password: '',
  displayName: ''
})

const handleSubmit = async () => {
  loading.value = true
  error.value = ''
  try {
    if (mode.value === 'login') {
      await auth.login(form.email, form.password)
    } else {
      await auth.register(form.email, form.password, form.displayName)
    }
    router.push('/')
  } catch (e) {
    error.value = e.response?.data?.message || e.message || t('common.operationFailed')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.sf-login {
  width: 100vw;
  min-width: 100%;
  box-sizing: border-box;
  min-height: 100vh;
  display: grid;
  grid-template-columns: auto auto;
  place-content: center;
  place-items: center;
  padding: 48px;
  background: var(--sf-bg);
  color: var(--sf-on-bg);
}

.sf-login::before {
  content: "";
  position: fixed;
  inset: 0;
  pointer-events: none;
  background:
    radial-gradient(circle at 24% 20%, rgba(208, 188, 255, 0.18), transparent 32%),
    radial-gradient(circle at 74% 72%, rgba(119, 183, 255, 0.12), transparent 34%);
}

.sf-login > section {
  position: relative;
  z-index: 1;
}

.sf-login-preview {
  width: min(50vw, 560px);
  min-height: 760px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 48px;
  background: linear-gradient(135deg, var(--sf-surface-container-low) 0%, var(--sf-bg) 100%);
  border: 1px solid var(--sf-outline-variant);
  border-right: 0;
  border-radius: 24px 0 0 24px;
}

.sf-login-brand {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 32px;
}

.sf-login-visual {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.sf-login-browser {
  width: 100%;
  max-width: 500px;
  background: var(--sf-surface-container);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 20px 60px var(--sf-shadow);
}

.sf-browser-bar {
  height: 36px;
  background: var(--sf-surface-container-high);
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 16px;
}

.sf-browser-bar span {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--sf-outline-variant);
}

.sf-browser-body {
  display: flex;
  height: 300px;
}

.sf-browser-sidebar {
  width: 48px;
  background: var(--sf-surface-container-low);
  border-right: 1px solid var(--sf-outline-variant);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 16px 0;
  gap: 16px;
}

.sf-browser-sidebar span {
  width: 24px;
  height: 24px;
  border-radius: 6px;
  background: var(--sf-surface-variant);
}

.sf-browser-sidebar span.sf-browser-active {
  background: var(--sf-primary-container);
}

.sf-browser-canvas {
  flex: 1;
  padding: 24px;
  overflow: hidden;
}

.sf-browser-hero {
  background: linear-gradient(135deg, var(--sf-primary-container), var(--sf-secondary-container));
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 16px;
}

.sf-browser-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.sf-browser-grid div {
  height: 80px;
  background: var(--sf-surface-variant);
  border-radius: 8px;
}

.sf-login-stats {
  display: flex;
  gap: 32px;
  margin-top: 32px;
}

.sf-login-stats div {
  text-align: center;
}

.sf-login-stats strong {
  display: block;
  font-size: 24px;
  font-weight: 700;
  color: var(--sf-primary);
}

.sf-login-stats span {
  font-size: 12px;
  color: var(--sf-on-surface-variant);
}

.sf-login-form-wrap {
  width: min(42vw, 480px);
  min-height: 760px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px;
  background: var(--sf-surface-container);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 0 24px 24px 0;
}

.sf-login-card {
  width: 100%;
  max-width: 380px;
}

.sf-login-locale {
  display: block;
  margin: 0 0 16px auto;
  border: 1px solid var(--sf-outline-variant);
  border-radius: 999px;
  background: transparent;
  color: var(--sf-on-surface-variant);
  cursor: pointer;
  font-size: 13px;
  font-weight: 700;
  padding: 7px 12px;
}

.sf-login-locale:hover {
  border-color: var(--sf-primary);
  color: var(--sf-primary);
}

.sf-login-tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 24px;
  border-bottom: 1px solid var(--sf-outline-variant);
}

.sf-login-tab {
  background: none;
  border: none;
  padding: 12px 20px;
  font-size: 14px;
  font-weight: 500;
  color: var(--sf-on-surface-variant);
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-bottom: -1px;
  transition: all 0.2s;
}

.sf-login-tab.active {
  color: var(--sf-primary);
  border-bottom-color: var(--sf-primary);
}

.sf-login-field {
  margin-bottom: 16px;
}

.sf-login-input-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

.sf-login-input-icon {
  position: absolute;
  left: 14px;
  color: var(--sf-on-surface-variant);
  font-size: 18px;
  z-index: 1;
}

.sf-login-input {
  padding-left: 44px;
  padding-right: 44px;
  border-radius: 12px;
  width: 100%;
}

.sf-login-eye {
  position: absolute;
  right: 12px;
  background: none;
  border: none;
  color: var(--sf-on-surface-variant);
  cursor: pointer;
  padding: 4px;
  display: flex;
  align-items: center;
}

.sf-login-submit {
  width: 100%;
  margin-top: 24px;
  justify-content: center;
  padding: 14px;
  font-size: 16px;
}

.sf-login-error {
  margin-top: 16px;
  padding: 12px;
  background: var(--sf-error-container);
  color: var(--sf-on-error-container);
  border-radius: 12px;
  font-size: 14px;
}

.sf-login-hint {
  margin-top: 24px;
  text-align: center;
  font-size: 14px;
  color: var(--sf-on-surface-variant);
}

.sf-login-hint a {
  color: var(--sf-primary);
  text-decoration: none;
  font-weight: 500;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@media (max-width: 768px) {
  .sf-login {
    grid-template-columns: 1fr;
    padding: 24px;
  }
  .sf-login-preview {
    display: none;
  }
  .sf-login-form-wrap {
    width: 100%;
    min-height: auto;
    border-left: none;
    border-radius: 20px;
    padding: 24px;
  }
}
</style>
