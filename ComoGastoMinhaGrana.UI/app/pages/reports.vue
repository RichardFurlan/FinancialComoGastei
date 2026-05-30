<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import { formatCurrency, formatDate } from '~/composables/useApi'
import type { ReportDetailDto, ReportSummaryDto } from '~/composables/useReports'

definePageMeta({ middleware: 'auth' })

const auth = useAuthStore()
const { reports, loading, fetchReports, createReport, deleteReport, fetchReportDetail } = useReports()

// --- Statements disponíveis para selecionar ---
interface StatementOption { id: string; fileName: string; uploadDate: string; transactionCount: number }
const availableStatements = ref<StatementOption[]>([])
async function loadStatements() {
  availableStatements.value = await apiFetch<StatementOption[]>('/statements')
}

// --- Criar relatório (modal 2 passos) ---
const modalStep = ref<1 | 2>(1)
const modalOpen = ref(false)
const form = ref({ name: '', selectedIds: new Set<string>() })
const saving = ref(false)
const createError = ref('')

function openCreate() {
  modalStep.value = 1
  form.value = { name: '', selectedIds: new Set() }
  createError.value = ''
  modalOpen.value = true
}

function toggleStatement(id: string) {
  if (form.value.selectedIds.has(id)) {
    form.value.selectedIds.delete(id)
  } else if (form.value.selectedIds.size < 6) {
    form.value.selectedIds.add(id)
  }
  form.value.selectedIds = new Set(form.value.selectedIds) // trigger reactivity
}

async function saveReport() {
  if (!form.value.name.trim()) { createError.value = 'Nome é obrigatório.'; return }
  if (form.value.selectedIds.size === 0) { createError.value = 'Selecione pelo menos 1 extrato.'; return }
  saving.value = true
  createError.value = ''
  try {
    await createReport(form.value.name, [...form.value.selectedIds])
    modalOpen.value = false
  } catch (e: unknown) {
    createError.value = e instanceof Error ? e.message : 'Erro ao criar relatório.'
  } finally {
    saving.value = false
  }
}

// --- Excluir ---
const deleteTarget = ref<ReportSummaryDto | null>(null)
const deleting = ref(false)
async function confirmDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await deleteReport(deleteTarget.value.id)
    if (activeReport.value?.id === deleteTarget.value.id) activeReport.value = null
    deleteTarget.value = null
  } finally {
    deleting.value = false
  }
}

// --- Detalhe ---
const activeReport = ref<ReportDetailDto | null>(null)
const loadingDetail = ref(false)
const pieCanvas = ref<HTMLCanvasElement | null>(null)
const barCanvas = ref<HTMLCanvasElement | null>(null)
let pieChart: import('chart.js').Chart | null = null
let barChart: import('chart.js').Chart | null = null

async function openReport(report: ReportSummaryDto) {
  loadingDetail.value = true
  activeReport.value = null
  try {
    activeReport.value = await fetchReportDetail(report.id)
    nextTick(() => renderCharts())
  } finally {
    loadingDetail.value = false
  }
}

async function renderCharts() {
  if (!activeReport.value) return
  const { Chart, ArcElement, PieController, BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend } = await import('chart.js')
  Chart.register(ArcElement, PieController, BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend)

  // Pie — categorias agregadas
  if (pieCanvas.value && activeReport.value.categoryTotals.length > 0) {
    if (pieChart) { pieChart.destroy(); pieChart = null }
    pieChart = new Chart(pieCanvas.value, {
      type: 'pie',
      data: {
        labels: activeReport.value.categoryTotals.map(c => c.name),
        datasets: [{
          data: activeReport.value.categoryTotals.map(c => c.total),
          backgroundColor: activeReport.value.categoryTotals.map(c => c.color),
          borderWidth: 2, borderColor: 'white',
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

  // Bar — top 5 categorias por import
  if (barCanvas.value && activeReport.value.byImport.length > 1) {
    if (barChart) { barChart.destroy(); barChart = null }
    // Coletar todas as categorias únicas que aparecem em qualquer import
    const allCategories = [...new Set(
      activeReport.value.byImport.flatMap(imp => imp.topCategories.map(c => c.name))
    )].slice(0, 5)

    const datasets = activeReport.value.byImport.map((imp, i) => ({
      label: imp.fileName.replace(/\.[^.]+$/, '').slice(0, 20),
      data: allCategories.map(cat => imp.topCategories.find(c => c.name === cat)?.total ?? 0),
      backgroundColor: `hsl(${i * 60}, 65%, 55%)`,
    }))

    barChart = new Chart(barCanvas.value, {
      type: 'bar',
      data: { labels: allCategories, datasets },
      options: {
        responsive: true,
        plugins: { legend: { position: 'top' } },
        scales: { y: { beginAtZero: true } },
      },
    })
  }
}

onMounted(() => Promise.all([fetchReports(), loadStatements()]))
onUnmounted(() => { pieChart?.destroy(); barChart?.destroy() })
</script>

<template>
  <div class="min-h-screen bg-base-200">
    <!-- Navbar -->
    <div class="navbar bg-base-100 shadow-sm px-4">
      <div class="flex-1 gap-2">
        <NuxtLink to="/" class="btn btn-ghost btn-sm">← Extratos</NuxtLink>
        <span class="font-bold text-lg">Relatórios</span>
      </div>
      <div class="flex-none gap-2">
        <NuxtLink to="/rules" class="btn btn-ghost btn-sm">Regras</NuxtLink>
        <NuxtLink to="/categories" class="btn btn-ghost btn-sm">Categorias</NuxtLink>
        <span class="text-sm opacity-70">{{ auth.user?.fullName }}</span>
        <button class="btn btn-ghost btn-sm" @click="auth.logout()">Sair</button>
      </div>
    </div>

    <div class="container mx-auto max-w-6xl p-6 flex flex-col gap-6">
      <!-- Header + criar -->
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-xl font-semibold">Meus Relatórios</h2>
          <p class="text-sm opacity-60">Agregue até 6 extratos por relatório.</p>
        </div>
        <button class="btn btn-primary btn-sm" @click="openCreate">+ Novo Relatório</button>
      </div>

      <!-- Loading / empty -->
      <div v-if="loading" class="flex justify-center py-16">
        <span class="loading loading-spinner loading-lg" />
      </div>
      <div v-else-if="reports.length === 0 && !activeReport" class="text-center py-16 opacity-50">
        <p class="text-4xl mb-2">📊</p>
        <p>Nenhum relatório criado ainda.</p>
      </div>

      <div v-else class="flex flex-col lg:flex-row gap-6">
        <!-- Lista lateral -->
        <div class="w-full lg:w-72 flex flex-col gap-2">
          <div
            v-for="r in reports"
            :key="r.id"
            class="card bg-base-100 shadow cursor-pointer transition-shadow hover:shadow-md"
            :class="{ 'ring-2 ring-primary': activeReport?.id === r.id }"
            @click="openReport(r)"
          >
            <div class="card-body py-3 px-4">
              <div class="flex justify-between items-start">
                <div>
                  <p class="font-medium truncate max-w-[180px]">{{ r.name }}</p>
                  <p class="text-xs opacity-60">{{ formatDate(r.createdAt) }} · {{ r.statementCount }} extrato(s)</p>
                </div>
                <button
                  class="btn btn-ghost btn-xs text-error flex-shrink-0"
                  @click.stop="deleteTarget = r"
                >✕</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Detalhe -->
        <div class="flex-1">
          <div v-if="loadingDetail" class="flex justify-center py-16">
            <span class="loading loading-spinner loading-lg" />
          </div>

          <div v-else-if="activeReport" class="flex flex-col gap-6">
            <h3 class="text-lg font-bold">{{ activeReport.name }}</h3>

            <!-- Cards de resumo por moeda -->
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <template v-for="c in activeReport.currencies" :key="c.currency">
                <div class="stat bg-base-100 rounded-box shadow">
                  <div class="stat-title">Saídas ({{ c.currency }})</div>
                  <div class="stat-value text-lg text-error">{{ formatCurrency(c.debits, c.currency) }}</div>
                </div>
                <div class="stat bg-base-100 rounded-box shadow">
                  <div class="stat-title">Entradas ({{ c.currency }})</div>
                  <div class="stat-value text-lg text-success">{{ formatCurrency(c.credits, c.currency) }}</div>
                </div>
                <div class="stat bg-base-100 rounded-box shadow">
                  <div class="stat-title">Saldo ({{ c.currency }})</div>
                  <div class="stat-value text-lg" :class="c.balance >= 0 ? 'text-success' : 'text-error'">
                    {{ formatCurrency(c.balance, c.currency) }}
                  </div>
                </div>
              </template>
            </div>

            <!-- Gráficos -->
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <!-- Pizza: categorias agregadas -->
              <div v-if="activeReport.categoryTotals.length > 0" class="card bg-base-100 shadow">
                <div class="card-body">
                  <h4 class="font-semibold mb-2">Gastos por Categoria</h4>
                  <canvas ref="pieCanvas" class="max-h-56" />
                </div>
              </div>

              <!-- Barras: comparação entre imports -->
              <div v-if="activeReport.byImport.length > 1" class="card bg-base-100 shadow">
                <div class="card-body">
                  <h4 class="font-semibold mb-2">Top Categorias por Extrato</h4>
                  <canvas ref="barCanvas" class="max-h-56" />
                </div>
              </div>
            </div>

            <!-- Top 5 despesas -->
            <div v-if="activeReport.topExpenses.length > 0" class="card bg-base-100 shadow">
              <div class="card-body">
                <h4 class="font-semibold mb-3">Top 5 Maiores Despesas</h4>
                <table class="table table-sm">
                  <thead>
                    <tr><th>Data</th><th>Descrição</th><th>Categoria</th><th class="text-right">Valor</th></tr>
                  </thead>
                  <tbody>
                    <tr v-for="e in activeReport.topExpenses" :key="e.description + e.date">
                      <td class="whitespace-nowrap opacity-70">{{ formatDate(e.date) }}</td>
                      <td>{{ e.description }}</td>
                      <td class="opacity-60 text-sm">{{ e.categoryName ?? '—' }}</td>
                      <td class="text-right font-mono text-error">{{ formatCurrency(e.amount, e.currency) }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Extratos incluídos -->
            <div class="card bg-base-100 shadow">
              <div class="card-body">
                <h4 class="font-semibold mb-3">Extratos Incluídos ({{ activeReport.statements.length }})</h4>
                <div class="flex flex-col gap-1">
                  <div v-for="s in activeReport.statements" :key="s.id" class="flex justify-between text-sm py-1 border-b border-base-200">
                    <NuxtLink :to="`/statements/${s.id}`" class="link link-hover truncate max-w-xs">{{ s.fileName }}</NuxtLink>
                    <span class="opacity-60 flex-shrink-0 ml-2">{{ s.transactionCount }} transações · {{ formatDate(s.uploadDate) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="text-center py-16 opacity-40">
            <p>Selecione um relatório para visualizar.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal criar relatório (2 passos) -->
    <dialog :open="modalOpen" class="modal modal-bottom sm:modal-middle">
      <div class="modal-box max-w-lg">
        <!-- Passo 1: Nome -->
        <div v-if="modalStep === 1">
          <h3 class="font-bold text-lg mb-4">Novo Relatório — Nome</h3>
          <div class="form-control">
            <label class="label"><span class="label-text">Nome do relatório</span></label>
            <input
              v-model="form.name"
              type="text"
              placeholder="Ex: Janeiro a Junho 2026"
              class="input input-bordered w-full"
              maxlength="100"
              @keyup.enter="form.name.trim() && (modalStep = 2)"
            />
          </div>
          <div class="modal-action">
            <button class="btn btn-ghost" @click="modalOpen = false">Cancelar</button>
            <button class="btn btn-primary" :disabled="!form.name.trim()" @click="modalStep = 2">
              Próximo →
            </button>
          </div>
        </div>

        <!-- Passo 2: Selecionar extratos -->
        <div v-else>
          <h3 class="font-bold text-lg mb-1">Selecionar Extratos</h3>
          <p class="text-sm opacity-60 mb-4">
            Selecione de 1 a 6 extratos. {{ form.selectedIds.size }}/6 selecionados.
          </p>
          <div class="flex flex-col gap-2 max-h-64 overflow-y-auto">
            <label
              v-for="s in availableStatements.filter(s => s.status === 'Processed' || !s.status)"
              :key="s.id"
              class="flex items-center gap-3 p-2 rounded cursor-pointer hover:bg-base-200 transition-colors"
              :class="{ 'opacity-40 cursor-not-allowed': !form.selectedIds.has(s.id) && form.selectedIds.size >= 6 }"
            >
              <input
                type="checkbox"
                class="checkbox checkbox-sm"
                :checked="form.selectedIds.has(s.id)"
                :disabled="!form.selectedIds.has(s.id) && form.selectedIds.size >= 6"
                @change="toggleStatement(s.id)"
              />
              <div>
                <p class="text-sm font-medium">{{ s.fileName }}</p>
                <p class="text-xs opacity-60">{{ s.transactionCount }} transações · {{ formatDate(s.uploadDate) }}</p>
              </div>
            </label>
          </div>
          <div v-if="createError" class="alert alert-error mt-3 text-sm py-2">{{ createError }}</div>
          <div class="modal-action">
            <button class="btn btn-ghost" @click="modalStep = 1">← Voltar</button>
            <button class="btn btn-primary" :disabled="saving || form.selectedIds.size === 0" @click="saveReport">
              <span v-if="saving" class="loading loading-spinner loading-sm" />
              Criar Relatório
            </button>
          </div>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop" @click="modalOpen = false"><button>Fechar</button></form>
    </dialog>

    <!-- Modal exclusão -->
    <dialog :open="!!deleteTarget" class="modal modal-bottom sm:modal-middle">
      <div class="modal-box">
        <h3 class="font-bold text-lg">Excluir relatório?</h3>
        <p class="py-4 text-sm">
          O relatório <strong>"{{ deleteTarget?.name }}"</strong> será removido. Os extratos não são afetados.
        </p>
        <div class="modal-action">
          <button class="btn btn-ghost" @click="deleteTarget = null">Cancelar</button>
          <button class="btn btn-error" :disabled="deleting" @click="confirmDelete">
            <span v-if="deleting" class="loading loading-spinner loading-sm" />
            Excluir
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop" @click="deleteTarget = null"><button>Fechar</button></form>
    </dialog>
  </div>
</template>
