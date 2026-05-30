<script setup lang="ts">
import { marked } from 'marked'
import { apiFetch, formatCurrency, formatDate } from '~/composables/useApi'
import type { CategoryDto } from '~/composables/useCategories'

definePageMeta({ middleware: 'auth' })

const route = useRoute()
const id = route.params.id as string

interface Transaction {
  id: string
  date: string
  description: string
  amount: number
  currency: string
  categoryId: string | null
  categoryName: string | null
  categoryColor: string | null
}

interface StatementDetail {
  id: string
  fileName: string
  uploadDate: string
  status: string
  baseCurrency: string
  errorMessage: string | null
  transactions: Transaction[]
  hasAnalysis: boolean
}

const statement = ref<StatementDetail | null>(null)
const analysisMarkdown = ref<string | null>(null)
const loadingAnalysis = ref(false)
const analysisError = ref('')

const { categories, fetchCategories } = useCategories()
const categoryUpdating = ref<Set<string>>(new Set())

// --- Apply rules ---
const applyingRules = ref(false)
const applyRulesResult = ref<string | null>(null)

async function applyRules() {
  applyingRules.value = true
  applyRulesResult.value = null
  try {
    const res = await apiFetch<{ categorized: number }>(`/statements/${id}/apply-rules`, { method: 'POST' })
    applyRulesResult.value = res.categorized > 0
      ? `${res.categorized} transação(ões) categorizadas automaticamente.`
      : 'Nenhuma transação correspondeu às regras.'
    if (res.categorized > 0) {
      const detail = await apiFetch<StatementDetail>(`/statements/${id}`)
      statement.value = detail
      nextTick(() => renderChart())
    }
  } catch {
    applyRulesResult.value = 'Erro ao aplicar regras.'
  } finally {
    applyingRules.value = false
  }
}

// --- Export ---
const config = useRuntimeConfig()
const exporting = ref(false)

async function exportStatement(format: string) {
  exporting.value = true
  try {
    const base = `${config.public.apiBase}/api`
    const response = await fetch(`${base}/statements/${id}/export?format=${format}`, { credentials: 'include' })
    if (!response.ok) throw new Error('Erro ao exportar.')
    const blob = await response.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    const ext = format === 'xlsx' ? 'xlsx' : format
    a.download = `extrato-${id}.${ext}`
    a.click()
    URL.revokeObjectURL(url)
  } finally {
    exporting.value = false
  }
}

// --- Chart ---
const chartCanvas = ref<HTMLCanvasElement | null>(null)
let chartInstance: import('chart.js').Chart | null = null

async function load() {
  const [detail] = await Promise.all([
    apiFetch<StatementDetail>(`/statements/${id}`),
    fetchCategories(),
  ])
  statement.value = detail
  if (detail.hasAnalysis) loadAnalysis()
  nextTick(() => renderChart())
}

async function loadAnalysis() {
  loadingAnalysis.value = true
  analysisError.value = ''
  try {
    const res = await apiFetch<{ markdown: string }>(`/statements/${id}/analysis`)
    analysisMarkdown.value = res.markdown
  } catch {
    analysisError.value = 'Análise ainda não disponível.'
  } finally {
    loadingAnalysis.value = false
  }
}

async function assignCategory(transaction: Transaction, categoryId: string | null) {
  categoryUpdating.value = new Set([...categoryUpdating.value, transaction.id])
  try {
    await apiFetch(`/transactions/${transaction.id}/category`, {
      method: 'PATCH',
      body: { categoryId: categoryId || null },
    })
    const cat = categories.value.find((c: CategoryDto) => c.id === categoryId) ?? null
    transaction.categoryId = cat?.id ?? null
    transaction.categoryName = cat?.name ?? null
    transaction.categoryColor = cat?.color ?? null
    nextTick(() => renderChart())
  } catch {
    // mantém o estado anterior — o select reflete o valor do objeto reativo
  } finally {
    const next = new Set(categoryUpdating.value)
    next.delete(transaction.id)
    categoryUpdating.value = next
  }
}

// --- Pie chart ---
const spendingByCategory = computed(() => {
  if (!statement.value) return []
  const debits = statement.value.transactions.filter(t => t.amount < 0)
  const map = new Map<string, { name: string; color: string; total: number }>()
  for (const t of debits) {
    const key = t.categoryId ?? '__none__'
    const label = t.categoryName ?? 'Sem categoria'
    const color = t.categoryColor ?? '#94A3B8'
    const existing = map.get(key)
    if (existing) { existing.total += Math.abs(t.amount) }
    else { map.set(key, { name: label, color, total: Math.abs(t.amount) }) }
  }
  return [...map.values()].sort((a, b) => b.total - a.total)
})

async function renderChart() {
  if (!chartCanvas.value || spendingByCategory.value.length === 0) return
  const { Chart, ArcElement, PieController, Tooltip, Legend } = await import('chart.js')
  Chart.register(ArcElement, PieController, Tooltip, Legend)
  if (chartInstance) { chartInstance.destroy(); chartInstance = null }
  chartInstance = new Chart(chartCanvas.value, {
    type: 'pie',
    data: {
      labels: spendingByCategory.value.map(c => c.name),
      datasets: [{
        data: spendingByCategory.value.map(c => c.total),
        backgroundColor: spendingByCategory.value.map(c => c.color),
        borderWidth: 2,
        borderColor: 'white',
      }],
    },
    options: {
      responsive: true,
      plugins: {
        legend: { position: 'right' },
        tooltip: { callbacks: { label: ctx => ` ${formatCurrency(ctx.raw as number)}` } },
      },
    },
  })
}

const totalDebits = computed(() =>
  statement.value?.transactions.filter(t => t.amount < 0).reduce((s, t) => s + Math.abs(t.amount), 0) ?? 0
)
const totalCredits = computed(() =>
  statement.value?.transactions.filter(t => t.amount >= 0).reduce((s, t) => s + t.amount, 0) ?? 0
)
const renderedAnalysis = computed(() =>
  analysisMarkdown.value ? marked.parse(analysisMarkdown.value) : ''
)

onMounted(load)
onUnmounted(() => { if (chartInstance) chartInstance.destroy() })
</script>

<template>
  <div class="min-h-screen bg-base-200">
    <div class="navbar bg-base-100 shadow-sm px-4">
      <NuxtLink to="/" class="btn btn-ghost btn-sm gap-1">← Voltar</NuxtLink>
      <span class="font-bold ml-2 truncate max-w-xs">{{ statement?.fileName }}</span>
      <div class="flex-1" />
      <div class="flex gap-2">
        <!-- Exportar dropdown -->
        <div class="dropdown dropdown-end">
          <button tabindex="0" class="btn btn-outline btn-sm" :disabled="exporting">
            <span v-if="exporting" class="loading loading-spinner loading-xs" />
            Exportar ▾
          </button>
          <ul tabindex="0" class="dropdown-content menu bg-base-100 rounded-box shadow z-10 w-36 p-1 mt-1">
            <li><button @click="exportStatement('csv')">CSV</button></li>
            <li><button @click="exportStatement('xlsx')">Excel</button></li>
            <li><button @click="exportStatement('pdf')">PDF</button></li>
          </ul>
        </div>
        <NuxtLink to="/rules" class="btn btn-ghost btn-sm">Regras</NuxtLink>
        <NuxtLink to="/categories" class="btn btn-ghost btn-sm">Categorias</NuxtLink>
      </div>
    </div>

    <div class="container mx-auto max-w-5xl p-6 flex flex-col gap-6">
      <!-- Cards de resumo -->
      <div v-if="statement" class="grid grid-cols-2 sm:grid-cols-3 gap-4">
        <div class="stat bg-base-100 rounded-box shadow">
          <div class="stat-title">Transações</div>
          <div class="stat-value text-xl">{{ statement.transactions.length }}</div>
        </div>
        <div class="stat bg-base-100 rounded-box shadow">
          <div class="stat-title">Saídas</div>
          <div class="stat-value text-xl text-error">{{ formatCurrency(totalDebits) }}</div>
        </div>
        <div class="stat bg-base-100 rounded-box shadow">
          <div class="stat-title">Entradas</div>
          <div class="stat-value text-xl text-success">{{ formatCurrency(totalCredits) }}</div>
        </div>
      </div>

      <!-- Gráfico de gastos por categoria -->
      <div v-if="spendingByCategory.length > 0" class="card bg-base-100 shadow">
        <div class="card-body">
          <h2 class="card-title">Gastos por Categoria</h2>
          <div class="flex flex-col sm:flex-row items-center gap-6">
            <canvas ref="chartCanvas" class="max-w-xs max-h-64" />
            <div class="flex flex-col gap-2 text-sm w-full">
              <div
                v-for="item in spendingByCategory"
                :key="item.name"
                class="flex items-center justify-between gap-2"
              >
                <div class="flex items-center gap-2">
                  <span
                    class="w-3 h-3 rounded-full flex-shrink-0"
                    :style="{ backgroundColor: item.color }"
                  />
                  <span>{{ item.name }}</span>
                </div>
                <span class="font-mono font-medium">{{ formatCurrency(item.total) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Análise IA -->
      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <h2 class="card-title">Análise Inteligente</h2>
          <div v-if="loadingAnalysis" class="flex justify-center py-8">
            <span class="loading loading-spinner loading-md" />
          </div>
          <div v-else-if="analysisError" class="alert alert-warning text-sm">{{ analysisError }}</div>
          <div v-else-if="renderedAnalysis" class="prose max-w-none" v-html="renderedAnalysis" />
          <div v-else class="text-sm opacity-60 py-4 text-center">
            <span v-if="statement?.status === 'Processing' || statement?.status === 'Pending'">
              Extrato ainda sendo processado...
            </span>
            <span v-else>Análise não disponível para este extrato.</span>
          </div>
        </div>
      </div>

      <!-- Tabela de transações -->
      <div class="card bg-base-100 shadow overflow-x-auto">
        <div class="card-body pb-0">
          <div class="flex items-center justify-between mb-4">
            <h2 class="card-title">Transações</h2>
            <button
              class="btn btn-outline btn-sm"
              :disabled="applyingRules"
              @click="applyRules"
            >
              <span v-if="applyingRules" class="loading loading-spinner loading-xs" />
              ⚡ Aplicar Regras
            </button>
          </div>
          <div v-if="applyRulesResult" class="alert alert-sm mb-3 py-2 text-sm"
            :class="applyRulesResult.includes('Nenhuma') ? 'alert-info' : 'alert-success'">
            {{ applyRulesResult }}
          </div>
        </div>
        <table class="table table-sm table-zebra">
          <thead>
            <tr>
              <th>Data</th>
              <th>Descrição</th>
              <th>Categoria</th>
              <th class="text-right">Valor</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in statement?.transactions" :key="t.id">
              <td class="whitespace-nowrap opacity-70">{{ formatDate(t.date) }}</td>
              <td>{{ t.description }}</td>
              <td>
                <div class="flex items-center gap-1.5">
                  <span
                    v-if="t.categoryColor"
                    class="w-2.5 h-2.5 rounded-full flex-shrink-0"
                    :style="{ backgroundColor: t.categoryColor }"
                  />
                  <select
                    class="select select-xs select-bordered max-w-[160px]"
                    :value="t.categoryId ?? ''"
                    :disabled="categoryUpdating.has(t.id)"
                    @change="assignCategory(t, ($event.target as HTMLSelectElement).value || null)"
                  >
                    <option value="">Sem categoria</option>
                    <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                      {{ cat.name }}
                    </option>
                  </select>
                  <span v-if="categoryUpdating.has(t.id)" class="loading loading-spinner loading-xs" />
                </div>
              </td>
              <td
                class="text-right font-mono whitespace-nowrap"
                :class="t.amount >= 0 ? 'text-success' : 'text-error'"
              >
                {{ t.amount >= 0 ? '+' : '' }}{{ formatCurrency(t.amount) }}
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="!statement?.transactions.length" class="text-center py-8 opacity-50 text-sm">
          Nenhuma transação encontrada.
        </div>
      </div>
    </div>
  </div>
</template>
