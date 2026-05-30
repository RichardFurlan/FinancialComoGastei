<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'

definePageMeta({ middleware: 'auth' })

const auth = useAuthStore()
const tab = ref<'login' | 'register'>('login')
const email = ref('')
const password = ref('')
const fullName = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  error.value = ''
  loading.value = true
  try {
    if (tab.value === 'login') {
      await auth.login(email.value, password.value)
    } else {
      await auth.register(email.value, password.value, fullName.value)
    }
    await navigateTo('/')
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Erro ao autenticar.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-base-200 flex items-center justify-center p-4">
    <div class="card bg-base-100 shadow-xl w-full max-w-sm">
      <div class="card-body">
        <h1 class="card-title text-2xl font-bold mb-2">Como Gasto Minha Grana</h1>

        <div role="tablist" class="tabs tabs-boxed mb-4">
          <button role="tab" class="tab" :class="{ 'tab-active': tab === 'login' }" @click="tab = 'login'">
            Entrar
          </button>
          <button role="tab" class="tab" :class="{ 'tab-active': tab === 'register' }" @click="tab = 'register'">
            Cadastrar
          </button>
        </div>

        <form @submit.prevent="submit" class="flex flex-col gap-3">
          <input
            v-if="tab === 'register'"
            v-model="fullName"
            type="text"
            placeholder="Seu nome"
            class="input input-bordered w-full"
            required
          />
          <input
            v-model="email"
            type="email"
            placeholder="Email"
            class="input input-bordered w-full"
            required
            autocomplete="email"
          />
          <input
            v-model="password"
            type="password"
            placeholder="Senha"
            class="input input-bordered w-full"
            required
            autocomplete="current-password"
          />

          <div v-if="error" class="alert alert-error text-sm py-2">
            {{ error }}
          </div>

          <button type="submit" class="btn btn-primary w-full" :disabled="loading">
            <span v-if="loading" class="loading loading-spinner loading-sm" />
            {{ tab === 'login' ? 'Entrar' : 'Cadastrar' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>
