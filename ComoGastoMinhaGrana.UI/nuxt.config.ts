export default defineNuxtConfig({
  srcDir: 'app/',
  compatibilityDate: '2025-05-30',
  devtools: { enabled: true },
  modules: ['@nuxtjs/tailwindcss', '@pinia/nuxt'],
  tailwindcss: { exposeConfig: true },
  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5209'
    }
  }
})
