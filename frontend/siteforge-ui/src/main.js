import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import './styles.css'
import './styles/theme.css'
import { useThemeStore } from './stores/theme'
import { useLocaleStore } from './stores/locale'

const app = createApp(App)
app.use(createPinia())
app.use(router)
// Initialize theme from localStorage before mounting
useThemeStore()
useLocaleStore()
app.mount('#app')
