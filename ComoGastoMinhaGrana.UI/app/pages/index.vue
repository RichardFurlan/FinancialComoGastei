<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import { apiFetch, formatDate } from '~/composables/useApi'

definePageMeta({ middleware: 'auth' })

const auth = useAuthStore()

interface Statement {
  id: string
  fileName: string
  fileExtension: string
  uploadDate: string
  status: string
  transactionCount: number
  hasAnalysis: boolean
}

const statements = ref<Statement[]>([])
const uploading = ref(false)
const uploadError = ref('')
const showModal = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const pollingIds = ref(new Set<string>())

async function loadStatements() {
  statements.value = await apiFetch<Statement[]>('/statements')
}

async function handleUpload() {
  const file = fileInput.value?.files?.[0]
  if (!file) return

  uploadError.value = ''
  uploading.value = true
  try {
    const form = new FormData()
    form.append('file', file)
    const result = await apiFetch<{ id: string }>('/statements/upload', {
      method: 'POST',
      body: form,
    })
    showModal.value = false
    await loadStatements()
    startPolling(result.id)
  } catch (e: unknown) {
    uploadError.value = e instanceof Error ? e.message : 'Erro ao enviar arquivo.'
  } finally {
    uploading.value = false
    if (fileInput.value) fileInput.value.value = ''
  }
}

function startPolling(id: string) {
  if (pollingIds.value.has(id)) return
  pollingIds.value.add(id)

  const interval = setInterval(async () => {
    const updated = await apiFetch<Statement>(`/statements/${id}`).catch(() => null)
    if (!updated) { clearInterval(interval); pollingIds.value.delete(id); return }

    const idx = statements.value.findIndex(s => s.id === id)
    if (idx !== -1) statements.value[idx] = updated

    if (updated.status !== 'Pending' && updated.status !== 'Processing') {
      clearInterval(interval)
      pollingIds.value.delete(id)
    }
  }, 3000)
}

const statusBadge: Record<string, string> = {
  Pending: 'badge-warning',
  Processing: 'badge-info',
  Processed: 'badge-success',
  Error: 'badge-error',
}

const statusLabel: Record<string, string> = {
  Pending: 'Aguardando',
  Processing: 'Processando...',
  Processed: 'Pronto',
  Error: 'Erro',
}

onMounted(loadStatements)
</script>

<template>
  <div class="min-h-screen bg-base-200">
    <!-- Navbar -->
    <div class="navbar bg-base-100 shadow-sm px-4">
      <div class="flex-1">
        <span class="font-bold text-lg">Como Gasto Minha Grana</span>
      </div>
      <div class="flex-none gap-2">
        <NuxtLink to="/reports" class="btn btn-ghost btn-sm">Relatórios</NuxtLink>
        <NuxtLink to="/rules" class="btn btn-ghost btn-sm">Regras</NuxtLink>
        <NuxtLink to="/categories" class="btn btn-ghost btn-sm">Categorias</NuxtLink>
        <span class="text-sm opacity-70">{{ auth.user?.fullName }}</span>
        <button class="btn btn-ghost btn-sm" @click="auth.logout()">Sair</button>
      </div>
    </div>

    <!-- Conteúdo -->
    <div class="container mx-auto max-w-4xl p-6">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-xl font-semibold">Meus Extratos</h2>
        <button class="btn btn-primary btn-sm" @click="showModal = true">
          + Enviar Extrato
        </button>
      </div>

      <!-- Lista de extratos -->
      <div v-if="statements.length === 0" class="text-center py-16 opacity-50">
        <p class="text-4xl mb-2">📄</p>
        <p>Nenhum extrato enviado ainda.</p>
      </div>

      <div v-else class="flex flex-col gap-3">
        <NuxtLink
          v-for="s in statements"
          :key="s.id"
          :to="`/statements/${s.id}`"
          class="card bg-base-100 shadow hover:shadow-md transition-shadow cursor-pointer"
        >
          <div class="card-body py-4 px-5 flex-row items-center justify-between">
            <div class="flex flex-col gap-0.5">
              <span class="font-medium">{{ s.fileName }}</span>
              <span class="text-sm opacity-60">{{ formatDate(s.uploadDate) }} · {{ s.transactionCount }} transações</span>
            </div>
            <div class="flex items-center gap-2">
              <span v-if="s.hasAnalysis" class="badge badge-outline badge-sm">Análise IA</span>
              <span class="badge badge-sm" :class="statusBadge[s.status] ?? 'badge-neutral'">
                <span v-if="s.status === 'Processing'" class="loading loading-spinner loading-xs mr-1" />
                {{ statusLabel[s.status] ?? s.status }}
              </span>
            </div>
          </div>
        </NuxtLink>
      </div>
    </div>

    <!-- Modal de upload -->
    <dialog :open="showModal" class="modal modal-bottom sm:modal-middle">
      <div class="modal-box">
        <h3 class="font-bold text-lg mb-4">Enviar Extrato</h3>
        <p class="text-sm opacity-70 mb-4">
          Formatos aceitos: PDF, TXT, Excel (.xlsx/.xls), imagem (JPG/PNG)
        </p>

        <input
          ref="fileInput"
          type="file"
          class="file-input file-input-bordered w-full"
          accept=".pdf,.txt,.xlsx,.xls,.jpg,.jpeg,.png"
        />

        <div v-if="uploadError" class="alert alert-error mt-3 text-sm py-2">
          {{ uploadError }}
        </div>

        <div class="modal-action">
          <button class="btn btn-ghost" @click="showModal = false; uploadError = ''">Cancelar</button>
          <button class="btn btn-primary" :disabled="uploading" @click="handleUpload">
            <span v-if="uploading" class="loading loading-spinner loading-sm" />
            Enviar
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop" @click="showModal = false">
        <button>Fechar</button>
      </form>
    </dialog>
  </div>
</template>
