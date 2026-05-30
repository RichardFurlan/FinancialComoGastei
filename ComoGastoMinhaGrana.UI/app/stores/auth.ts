import { defineStore } from 'pinia'
import { apiFetch } from '~/composables/useApi'

interface User {
  id: string
  email: string
  fullName: string
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as User | null,
    loaded: false,
  }),

  getters: {
    isLoggedIn: (state) => !!state.user,
  },

  actions: {
    async fetchMe() {
      try {
        this.user = await apiFetch<User>('/auth/me')
      } catch {
        this.user = null
      } finally {
        this.loaded = true
      }
    },

    async login(email: string, password: string) {
      this.user = await apiFetch<User>('/auth/login', {
        method: 'POST',
        body: { email, password },
      })
    },

    async register(email: string, password: string, fullName: string) {
      this.user = await apiFetch<User>('/auth/register', {
        method: 'POST',
        body: { email, password, fullName },
      })
    },

    async logout() {
      await apiFetch('/auth/logout', { method: 'POST' }).catch(() => null)
      this.user = null
      navigateTo('/login')
    },
  },
})
