export interface CategoryDto {
  id: string
  name: string
  color: string
}

export function useCategories() {
  const categories = ref<CategoryDto[]>([])
  const loading = ref(false)

  async function fetchCategories() {
    loading.value = true
    try {
      categories.value = await apiFetch<CategoryDto[]>('/categories')
    } finally {
      loading.value = false
    }
  }

  async function createCategory(name: string, color: string): Promise<CategoryDto> {
    const created = await apiFetch<CategoryDto>('/categories', {
      method: 'POST',
      body: { name, color },
    })
    categories.value.push(created)
    categories.value.sort((a, b) => a.name.localeCompare(b.name))
    return created
  }

  async function updateCategory(id: string, name: string, color: string): Promise<void> {
    const updated = await apiFetch<CategoryDto>(`/categories/${id}`, {
      method: 'PUT',
      body: { name, color },
    })
    const idx = categories.value.findIndex(c => c.id === id)
    if (idx !== -1) categories.value[idx] = updated
    categories.value.sort((a, b) => a.name.localeCompare(b.name))
  }

  async function deleteCategory(id: string): Promise<void> {
    await apiFetch(`/categories/${id}`, { method: 'DELETE' })
    categories.value = categories.value.filter(c => c.id !== id)
  }

  return { categories, loading, fetchCategories, createCategory, updateCategory, deleteCategory }
}
