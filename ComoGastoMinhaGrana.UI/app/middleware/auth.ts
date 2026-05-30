import { useAuthStore } from '~/stores/auth'

export default defineNuxtRouteMiddleware(async (to) => {
  const auth = useAuthStore()

  if (!auth.loaded) {
    await auth.fetchMe()
  }

  if (!auth.isLoggedIn && to.path !== '/login') {
    return navigateTo('/login')
  }

  if (auth.isLoggedIn && to.path === '/login') {
    return navigateTo('/')
  }
})
