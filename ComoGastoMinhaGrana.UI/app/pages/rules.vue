<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import type { CategoryRuleDto } from '~/composables/useCategoryRules'

definePageMeta({ middleware: 'auth' })

const auth = useAuthStore()
const { rules, loading, fetchRules, createRule, updateRule, deleteRule } = useCategoryRules()
const { categories, fetchCategories } = useCategories()

const modalOpen = ref(false)
const editingRule = ref<CategoryRuleDto | null>(null)
const form = ref({ searchTerm: '', ruleMatchType: 'Contains', categoryId: '' })
const saving = ref(false)
const error = ref('')

const deleteTarget = ref<CategoryRuleDto | null>(null)
const deleting = ref(false)

const matchTypeOptions = [
  { value: 'Contains', label: 'Contém', badge: 'badge-info' },
  { value: 'Exact', label: 'Exato', badge: 'badge-warning' },
  { value: 'StartsWith', label: 'Começa com', badge: 'badge-success' },
]

function badgeClass(type: string) {
  return matchTypeOptions.find(o => o.value === type)?.badge ?? 'badge-neutral'
}
function matchLabel(type: string) {
  return matchTypeOptions.find(o => o.value === type)?.label ?? type
}

function openCreate() {
  editingRule.value = null
  form.value = { searchTerm: '', ruleMatchType: 'Contains', categoryId: categories.value[0]?.id ?? '' }
  error.value = ''
  modalOpen.value = true
}

function openEdit(rule: CategoryRuleDto) {
  editingRule.value = rule
  form.value = { searchTerm: rule.searchTerm, ruleMatchType: rule.ruleMatchType, categoryId: rule.categoryId }
  error.value = ''
  modalOpen.value = true
}

async function saveRule() {
  if (!form.value.searchTerm.trim()) { error.value = 'Termo de busca é obrigatório.'; return }
  if (!form.value.categoryId) { error.value = 'Selecione uma categoria.'; return }
  saving.value = true
  error.value = ''
  try {
    if (editingRule.value) {
      await updateRule(editingRule.value.id, form.value.searchTerm, form.value.ruleMatchType, form.value.categoryId)
    } else {
      await createRule(form.value.searchTerm, form.value.ruleMatchType, form.value.categoryId)
    }
    modalOpen.value = false
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Erro ao salvar.'
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await deleteRule(deleteTarget.value.id)
    deleteTarget.value = null
  } finally {
    deleting.value = false
  }
}

onMounted(() => Promise.all([fetchRules(), fetchCategories()]))
</script>

<template>
  <div class="min-h-screen bg-base-200">
    <!-- Navbar -->
    <div class="navbar bg-base-100 shadow-sm px-4">
      <div class="flex-1 gap-2">
        <NuxtLink to="/" class="btn btn-ghost btn-sm">← Extratos</NuxtLink>
        <span class="font-bold text-lg">Regras de Ouro</span>
      </div>
      <div class="flex-none gap-2">
        <NuxtLink to="/reports" class="btn btn-ghost btn-sm">Relatórios</NuxtLink>
        <NuxtLink to="/categories" class="btn btn-ghost btn-sm">Categorias</NuxtLink>
        <span class="text-sm opacity-70">{{ auth.user?.fullName }}</span>
        <button class="btn btn-ghost btn-sm" @click="auth.logout()">Sair</button>
      </div>
    </div>

    <div class="container mx-auto max-w-3xl p-6">
      <div class="flex justify-between items-center mb-2">
        <h2 class="text-xl font-semibold">Regras de Auto-Categorização</h2>
        <button
          class="btn btn-primary btn-sm"
          :disabled="categories.length === 0"
          :title="categories.length === 0 ? 'Crie uma categoria primeiro' : undefined"
          @click="openCreate"
        >+ Nova Regra</button>
      </div>
      <p class="text-sm opacity-60 mb-6">
        As regras são aplicadas em ordem de prioridade ao processar extratos. A primeira que corresponder ganha.
      </p>

      <!-- Loading -->
      <div v-if="loading" class="flex justify-center py-16">
        <span class="loading loading-spinner loading-lg" />
      </div>

      <!-- Empty state -->
      <div v-else-if="rules.length === 0" class="text-center py-16 opacity-50">
        <p class="text-4xl mb-2">⚡</p>
        <p>Nenhuma regra criada ainda.</p>
        <p class="text-sm mt-1">Crie regras para categorizar transações automaticamente ao fazer upload.</p>
      </div>

      <!-- Lista -->
      <div v-else class="flex flex-col gap-2">
        <div
          v-for="rule in rules"
          :key="rule.id"
          class="card bg-base-100 shadow"
        >
          <div class="card-body py-3 px-5 flex-row items-center gap-4">
            <span class="text-xs font-mono opacity-40 w-6 text-center">{{ rule.priority }}</span>
            <span class="badge badge-sm" :class="badgeClass(rule.ruleMatchType)">
              {{ matchLabel(rule.ruleMatchType) }}
            </span>
            <span class="font-mono font-medium flex-1 truncate">"{{ rule.searchTerm }}"</span>
            <div class="flex items-center gap-1.5">
              <span
                class="w-3 h-3 rounded-full flex-shrink-0"
                :style="{ backgroundColor: rule.categoryColor }"
              />
              <span class="text-sm">{{ rule.categoryName }}</span>
            </div>
            <div class="flex gap-2 flex-shrink-0">
              <button class="btn btn-ghost btn-xs" @click="openEdit(rule)">Editar</button>
              <button class="btn btn-ghost btn-xs text-error" @click="deleteTarget = rule">Excluir</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal criar/editar -->
    <dialog :open="modalOpen" class="modal modal-bottom sm:modal-middle">
      <div class="modal-box">
        <h3 class="font-bold text-lg mb-4">
          {{ editingRule ? 'Editar Regra' : 'Nova Regra de Ouro' }}
        </h3>

        <div class="flex flex-col gap-4">
          <div class="form-control">
            <label class="label"><span class="label-text">Termo de busca</span></label>
            <input
              v-model="form.searchTerm"
              type="text"
              placeholder='Ex: "IFOOD", "UBER", "AMAZON"'
              class="input input-bordered w-full"
              maxlength="200"
            />
          </div>

          <div class="form-control">
            <label class="label"><span class="label-text">Tipo de correspondência</span></label>
            <select v-model="form.ruleMatchType" class="select select-bordered w-full">
              <option v-for="opt in matchTypeOptions" :key="opt.value" :value="opt.value">
                {{ opt.label }} — {{ opt.value === 'Contains' ? 'descrição contém o termo' : opt.value === 'Exact' ? 'descrição é exatamente o termo' : 'descrição começa com o termo' }}
              </option>
            </select>
          </div>

          <div class="form-control">
            <label class="label"><span class="label-text">Categoria</span></label>
            <select v-model="form.categoryId" class="select select-bordered w-full">
              <option value="" disabled>Selecione uma categoria</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
            <div v-if="categories.length === 0" class="label">
              <span class="label-text-alt text-warning">
                Nenhuma categoria criada.
                <NuxtLink to="/categories" class="link">Criar categorias</NuxtLink>
              </span>
            </div>
          </div>
        </div>

        <div v-if="error" class="alert alert-error mt-3 text-sm py-2">{{ error }}</div>

        <div class="modal-action">
          <button class="btn btn-ghost" @click="modalOpen = false">Cancelar</button>
          <button class="btn btn-primary" :disabled="saving" @click="saveRule">
            <span v-if="saving" class="loading loading-spinner loading-sm" />
            Salvar
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop" @click="modalOpen = false"><button>Fechar</button></form>
    </dialog>

    <!-- Modal exclusão -->
    <dialog :open="!!deleteTarget" class="modal modal-bottom sm:modal-middle">
      <div class="modal-box">
        <h3 class="font-bold text-lg">Excluir regra?</h3>
        <p class="py-4 text-sm">
          A regra <strong>"{{ deleteTarget?.searchTerm }}"</strong> será removida permanentemente.
          Transações já categorizadas por ela não serão afetadas.
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
