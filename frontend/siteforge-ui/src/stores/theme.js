import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

export const useThemeStore = defineStore('theme', () => {
  const stored = localStorage.getItem('sf-theme') || 'dark'
  const theme = ref(stored)

  // Apply theme class to html element
  function apply(t) {
    document.documentElement.className = `theme-${t}`
    localStorage.setItem('sf-theme', t)
  }

  function toggle() {
    theme.value = theme.value === 'dark' ? 'light' : 'dark'
  }

  // Sync ref changes to DOM
  watch(theme, apply, { immediate: true })

  return { theme, toggle }
})
