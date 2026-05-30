<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import type { CategoryDto } from '~/composables/useCategories'

definePageMeta({ middleware: 'auth' })

const auth = useAuthStore()
const { categories, loading, fetchCategories, createCategory, updateCategory, deleteCategory } = useCategories()

const modalOpen = ref(false)
const editingCategory = ref<CategoryDto | null>(null)
const form = ref({ name: '', color: '#3B82F6' })
const saving = ref(false)
const error = ref('')

const deleteTarget = ref<CategoryDto | null>(null)
const deleting = ref(false)

function openCreate() {
  editingCategory.value = null
  form.value = { name: '', color: '#3B82F6' }
  error.value = ''
  modalOpen.value = true
}

function openEdit(cat: CategoryDto) {
  editingCategory.value = cat
  form.value = { name: cat.name, color: cat.color }
  error.value = ''
  modalOpen.value = true
}

async function saveCategory() {
  if (!form.value.name.trim()) { error.value = 'Nome é obrigatório.'; return }
  saving.value = true
  error.value = ''
  try {
    if (editingCategory.value) {
      await updateCategory(editingCategory.value.id, form.value.name, form.value.color)
    } else {
      await createCategory(form.value.name, form.value.color)
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
    await deleteCategory(deleteTarget.value.id)
    deleteTarget.value = null
  } finally {
    deleting.value = false
  }
}

onMounted(fetchCategories)
</script>

<template>
  <div class="min-h-screen bg-base-200">
    <!-- Navbar -->
    <div class="navbar bg-base-100 shadow-sm px-4">
      <div class="flex-1 gap-2">
        <NuxtLink to="/" class="btn btn-ghost btn-sm">← Extratos</NuxtLink>
        <span class="font-bold text-lg">Categorias</span>
      </div>
      <div class="flex-none gap-2">
        <NuxtLink to="/reports" class="btn btn-ghost btn-sm">Relatórios</NuxtLink>
        <NuxtLink to="/rules" class="btn btn-ghost btn-sm">Regras</NuxtLink>
        <span class="text-sm opacity-70">{{ auth.user?.fullName }}</span>
        <button class="btn btn-ghost btn-sm" @click="auth.logout()">Sair</button>
      </div>
    </div>

    <div class="container mx-auto max-w-2xl p-6">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-xl font-semibold">Minhas Categorias</h2>
        <button class="btn btn-primary btn-sm" @click="openCreate">+ Nova Categoria</button>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="flex justify-center py-16">
        <span class="loading loading-spinner loading-lg" />
      </div>

      <!-- Empty state -->
      <div v-else-if="categories.length === 0" class="text-center py-16 opacity-50">
        <p class="text-4xl mb-2">🏷️</p>
        <p>Nenhuma categoria criada ainda.</p>
        <p class="text-sm mt-1">Crie categorias para organizar suas transações.</p>
      </div>

      <!-- Lista -->
      <div v-else class="flex flex-col gap-2">
        <div
          v-for="cat in categories"
          :key="cat.id"
          class="card bg-base-100 shadow"
        >
          <div class="card-body py-3 px-5 flex-row items-center justify-between">
            <div class="flex items-center gap-3">
              <span
                class="w-5 h-5 rounded-full flex-shrink-0 border border-base-300"
                :style="{ backgroundColor: cat.color }"
              />
              <span class="font-medium">{{ cat.name }}</span>
            </div>
            <div class="flex gap-2">
              <button class="btn btn-ghost btn-xs" @click="openEdit(cat)">Editar</button>
              <button class="btn btn-ghost btn-xs text-error" @click="deleteTarget = cat">Excluir</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal criar/editar -->
    <dialog :open="modalOpen" class="modal modal-bottom sm:modal-middle">
      <div class="modal-box">
        <h3 class="font-bold text-lg mb-4">
          {{ editingCategory ? 'Editar Categoria' : 'Nova Categoria' }}
        </h3>

        <div class="flex flex-col gap-4">
          <div class="form-control">
            <label class="label"><span class="label-text">Nome</span></label>
            <input
              v-model="form.name"
              type="text"
              placeholder="Ex: Alimentação"
              class="input input-bordered w-full"
              maxlength="100"
              @keyup.enter="saveCategory"
            />
          </div>

          <div class="form-control">
            <label class="label"><span class="label-text">Cor</span></label>
            <div class="flex items-center gap-3">
              <input
                v-model="form.color"
                type="color"
                class="w-12 h-10 rounded cursor-pointer border border-base-300 p-0.5"
              />
              <span class="font-mono text-sm opacity-70">{{ form.color }}</span>
            </div>
          </div>
        </div>

        <div v-if="error" class="alert alert-error mt-3 text-sm py-2">{{ error }}</div>

        <div class="modal-action">
          <button class="btn btn-ghost" @click="modalOpen = false">Cancelar</button>
          <button class="btn btn-primary" :disabled="saving" @click="saveCategory">
            <span v-if="saving" class="loading loading-spinner loading-sm" />
            Salvar
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop" @click="modalOpen = false">
        <button>Fechar</button>
      </form>
    </dialog>

    <!-- Modal confirmação de exclusão -->
    <dialog :open="!!deleteTarget" class="modal modal-bottom sm:modal-middle">
      <div class="modal-box">
        <h3 class="font-bold text-lg">Excluir categoria?</h3>
        <p class="py-4 text-sm">
          A categoria <strong>{{ deleteTarget?.name }}</strong> será removida.
          As transações vinculadas a ela ficarão sem categoria.
        </p>
        <div class="modal-action">
          <button class="btn btn-ghost" @click="deleteTarget = null">Cancelar</button>
          <button class="btn btn-error" :disabled="deleting" @click="confirmDelete">
            <span v-if="deleting" class="loading loading-spinner loading-sm" />
            Excluir
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop" @click="deleteTarget = null">
        <button>Fechar</button>
      </form>
    </dialog>
  </div>
</template>
