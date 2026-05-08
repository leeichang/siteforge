import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    const refreshToken = localStorage.getItem('refreshToken')
    const isAuthRequest = originalRequest?.url?.toLowerCase().includes('/auth/')

    if (error.response?.status === 401 && refreshToken && !originalRequest?._retry && !isAuthRequest) {
      originalRequest._retry = true
      try {
        const response = await api.post('/Auth/refresh', { refreshToken })
        const authData = response.data?.data || response.data
        localStorage.setItem('token', authData.token)
        localStorage.setItem('refreshToken', authData.refreshToken)
        localStorage.setItem('user', JSON.stringify(authData.user))
        originalRequest.headers.Authorization = `Bearer ${authData.token}`
        return api(originalRequest)
      } catch {
        clearAuthAndRedirect()
      }
    }

    if (error.response?.status === 401 && !isAuthRequest) {
      clearAuthAndRedirect()
    }

    return Promise.reject(error)
  }
)

function clearAuthAndRedirect() {
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('user')
  if (window.location.pathname !== '/login') {
    window.location.href = '/login'
  }
}

export function unwrap(response) {
  return response.data?.data ?? response.data
}

export function errorMessage(error, fallback = '發生錯誤') {
  return error.response?.data?.message || error.response?.data?.errors?.join?.(', ') || error.message || fallback
}

export default api
