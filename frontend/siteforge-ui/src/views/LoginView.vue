<template>
  <main class="login-page">
    <section class="product-preview" aria-label="SiteForge preview">
      <div class="brand-lockup">
        <div class="brand-mark">SF</div>
        <div>
          <p class="sf-kicker">SiteForge</p>
          <h1>Template First, AI Assist</h1>
        </div>
      </div>

      <div class="preview-frame">
        <div class="preview-toolbar">
          <span></span>
          <span></span>
          <span></span>
          <strong>Editor workspace</strong>
        </div>
        <div class="preview-body">
          <aside class="preview-rail">
            <span class="active"></span>
            <span></span>
            <span></span>
            <span></span>
          </aside>
          <div class="preview-canvas">
            <div class="preview-hero">
              <small>Product launch</small>
              <h2>專業網站從模板開始</h2>
              <p>Hero、Features、FAQ、Contact 直接拖曳，內容與樣式可即時保存。</p>
            </div>
            <div class="preview-grid">
              <div></div>
              <div></div>
              <div></div>
            </div>
          </div>
          <aside class="preview-props">
            <p>Style Manager</p>
            <span></span>
            <span class="short"></span>
            <span></span>
          </aside>
        </div>
      </div>

      <div class="preview-stats">
        <div>
          <strong>7</strong>
          <span>系統區塊</span>
        </div>
        <div>
          <strong>3</strong>
          <span>裝置預覽</span>
        </div>
        <div>
          <strong>1</strong>
          <span>鍵發佈</span>
        </div>
      </div>
    </section>

    <section class="auth-panel" aria-label="Account form">
      <div class="auth-header">
        <p class="sf-kicker">{{ isLogin ? 'Welcome back' : 'Create account' }}</p>
        <h2>{{ isLogin ? '登入工作台' : '建立 SiteForge 帳號' }}</h2>
        <p>{{ isLogin ? '繼續編輯你的網站與發佈內容。' : '建立帳號後會直接進入網站管理。' }}</p>
      </div>

      <form @submit.prevent="handleSubmit" class="auth-form">
        <label>
          Email
          <input v-model="form.email" class="sf-input" type="email" autocomplete="email" required placeholder="you@example.com" />
        </label>
        <label>
          Password
          <input v-model="form.password" class="sf-input" type="password" autocomplete="current-password" required placeholder="輸入密碼" />
        </label>
        <label v-if="!isLogin">
          Display name
          <input v-model="form.displayName" class="sf-input" type="text" autocomplete="name" placeholder="你的名稱" />
        </label>

        <button type="submit" class="sf-button primary submit-button">
          {{ isLogin ? '登入' : '註冊' }}
        </button>
      </form>

      <p class="toggle">
        {{ isLogin ? '還沒有帳號？' : '已有帳號？' }}
        <a href="#" @click.prevent="isLogin = !isLogin">
          {{ isLogin ? '建立帳號' : '回到登入' }}
        </a>
      </p>

      <p v-if="error" class="error" role="alert">{{ error }}</p>
    </section>
  </main>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()
const isLogin = ref(true)
const error = ref('')
const form = ref({ email: '', password: '', displayName: '' })

async function handleSubmit() {
  error.value = ''
  try {
    if (isLogin.value) {
      await auth.login(form.value.email, form.value.password)
    } else {
      await auth.register(form.value.email, form.value.password, form.value.displayName)
    }
    router.push('/')
  } catch (e) {
    error.value = e.response?.data?.message || e.message || '發生錯誤'
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: minmax(0, 1.15fr) minmax(380px, 0.85fr);
  gap: 0;
}

.product-preview {
  position: relative;
  display: flex;
  min-height: 100vh;
  flex-direction: column;
  justify-content: space-between;
  padding: 46px;
  overflow: hidden;
  background:
    linear-gradient(120deg, rgba(19, 111, 99, 0.94), rgba(24, 92, 118, 0.88)),
    linear-gradient(rgba(255, 255, 255, 0.08) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.08) 1px, transparent 1px);
  background-size: auto, 34px 34px, 34px 34px;
  color: white;
}

.brand-lockup {
  display: flex;
  align-items: center;
  gap: 14px;
  max-width: 640px;
}

.brand-mark {
  width: 48px;
  height: 48px;
  display: grid;
  place-items: center;
  border: 1px solid rgba(255, 255, 255, 0.38);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.12);
  font-weight: 900;
}

.brand-lockup .sf-kicker {
  color: #bff8e9;
}

.brand-lockup h1 {
  max-width: 620px;
  font-size: clamp(34px, 5vw, 66px);
  line-height: 1.02;
}

.preview-frame {
  width: min(920px, 100%);
  margin: 48px 0;
  border: 1px solid rgba(255, 255, 255, 0.34);
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.12);
  box-shadow: 0 28px 80px rgba(3, 18, 22, 0.38);
  overflow: hidden;
}

.preview-toolbar {
  height: 48px;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.22);
  background: rgba(7, 27, 31, 0.26);
}

.preview-toolbar span {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.55);
}

.preview-toolbar strong {
  margin-left: 8px;
  font-size: 13px;
}

.preview-body {
  display: grid;
  grid-template-columns: 56px minmax(0, 1fr) 190px;
  min-height: 370px;
}

.preview-rail,
.preview-props {
  background: rgba(8, 32, 37, 0.34);
  padding: 16px;
}

.preview-rail {
  display: grid;
  align-content: start;
  gap: 12px;
}

.preview-rail span {
  width: 24px;
  height: 24px;
  border-radius: 6px;
  background: rgba(255, 255, 255, 0.24);
}

.preview-rail span.active {
  background: #f6c177;
}

.preview-canvas {
  padding: 26px;
  background: #f4f7f6;
  color: var(--sf-ink);
}

.preview-hero {
  min-height: 190px;
  border: 1px solid #dbe7e3;
  border-radius: 10px;
  padding: 28px;
  background: linear-gradient(135deg, white, #eaf7f2);
}

.preview-hero small {
  color: var(--sf-primary);
  font-weight: 850;
}

.preview-hero h2 {
  max-width: 420px;
  margin: 12px 0;
  font-size: 34px;
  line-height: 1.05;
}

.preview-hero p {
  max-width: 460px;
  color: var(--sf-muted);
}

.preview-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
  margin-top: 16px;
}

.preview-grid div {
  min-height: 72px;
  border: 1px solid #dbe7e3;
  border-radius: 8px;
  background: white;
}

.preview-props p {
  margin-bottom: 16px;
  font-size: 13px;
  font-weight: 850;
}

.preview-props span {
  display: block;
  height: 10px;
  border-radius: 999px;
  margin-bottom: 12px;
  background: rgba(255, 255, 255, 0.34);
}

.preview-props .short {
  width: 68%;
}

.preview-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
  max-width: 640px;
}

.preview-stats div {
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 8px;
  padding: 14px;
  background: rgba(255, 255, 255, 0.10);
}

.preview-stats strong {
  display: block;
  font-size: 24px;
}

.preview-stats span {
  color: #d9fff3;
  font-size: 13px;
}

.auth-panel {
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 46px;
  background: rgba(255, 255, 255, 0.86);
  border-left: 1px solid rgba(217, 226, 236, 0.8);
}

.auth-header {
  max-width: 440px;
  margin-bottom: 28px;
}

.auth-header h2 {
  margin: 8px 0;
  font-size: 32px;
  line-height: 1.12;
}

.auth-header p:last-child {
  color: var(--sf-muted);
}

.auth-form {
  display: grid;
  gap: 16px;
  max-width: 440px;
}

.auth-form label {
  color: var(--sf-ink);
  font-weight: 750;
}

.auth-form input {
  margin-top: 7px;
}

.submit-button {
  width: 100%;
  min-height: 48px;
  margin-top: 4px;
}

.toggle {
  max-width: 440px;
  margin-top: 18px;
  color: var(--sf-muted);
}

.toggle a {
  color: var(--sf-primary);
  font-weight: 850;
  text-decoration: none;
}

.error {
  max-width: 440px;
  margin-top: 16px;
  border: 1px solid #f4b8b2;
  border-radius: 8px;
  padding: 12px;
  background: #fff4f2;
  color: var(--sf-danger);
}

@media (max-width: 940px) {
  .login-page {
    grid-template-columns: 1fr;
  }

  .product-preview {
    min-height: auto;
    padding: 28px;
  }

  .auth-panel {
    border-left: 0;
    padding: 28px;
  }
}

@media (max-width: 640px) {
  .preview-body {
    grid-template-columns: 42px minmax(0, 1fr);
  }

  .preview-props {
    display: none;
  }

  .preview-stats {
    grid-template-columns: 1fr;
  }
}
</style>
