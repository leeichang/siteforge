import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../api/client'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const refreshToken = ref(localStorage.getItem('refreshToken') || '')
  const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))

  const isLoggedIn = computed(() => !!token.value)

  async function login(email, password) {
    const response = await api.post('/Auth/login', { email, password })
    const authData = response.data?.data || response.data
    setAuth(authData)
    return authData
  }

  async function register(email, password, displayName) {
    const response = await api.post('/Auth/register', { email, password, displayName })
    const authData = response.data?.data || response.data
    setAuth(authData)
    return authData
  }

  function setAuth(data) {
    token.value = data.token
    refreshToken.value = data.refreshToken
    user.value = data.user
    localStorage.setItem('token', data.token)
    localStorage.setItem('refreshToken', data.refreshToken)
    localStorage.setItem('user', JSON.stringify(data.user))
    api.defaults.headers.common['Authorization'] = `Bearer ${data.token}`
  }

  function logout() {
    token.value = ''
    refreshToken.value = ''
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
    delete api.defaults.headers.common['Authorization']
  }

  if (token.value) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
  }

  return { token, user, isLoggedIn, login, register, logout, setAuth }
})
