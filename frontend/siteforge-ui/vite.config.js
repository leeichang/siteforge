import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

const apiTarget = process.env.VITE_API_PROXY_TARGET || 'http://127.0.0.1:8000'
const uiPort = Number(process.env.SITEFORGE_UI_PORT || 5010)

export default defineConfig({
  plugins: [vue()],
  server: {
    port: uiPort,
    host: true,
    allowedHosts: ['.tunn.dev', '.tunnelto.dev'],
    proxy: {
      '/api': {
        target: apiTarget,
        changeOrigin: true,
      }
    }
  }
})
