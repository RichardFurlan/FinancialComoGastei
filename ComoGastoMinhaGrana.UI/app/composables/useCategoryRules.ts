export interface CategoryRuleDto {
  id: string
  searchTerm: string
  ruleMatchType: 'Contains' | 'Exact' | 'StartsWith'
  categoryId: string
  categoryName: string
  categoryColor: string
  priority: number
}

export function useCategoryRules() {
  const rules = ref<CategoryRuleDto[]>([])
  const loading = ref(false)

  async function fetchRules() {
    loading.value = true
    try {
      rules.value = await apiFetch<CategoryRuleDto[]>('/category-rules')
    } finally {
      loading.value = false
    }
  }

  async function createRule(
    searchTerm: string,
    ruleMatchType: string,
    categoryId: string
  ): Promise<CategoryRuleDto> {
    const created = await apiFetch<CategoryRuleDto>('/category-rules', {
      method: 'POST',
      body: { searchTerm, ruleMatchType, categoryId },
    })
    rules.value.push(created)
    return created
  }

  async function updateRule(
    id: string,
    searchTerm: string,
    ruleMatchType: string,
    categoryId: string
  ): Promise<void> {
    const updated = await apiFetch<CategoryRuleDto>(`/category-rules/${id}`, {
      method: 'PUT',
      body: { searchTerm, ruleMatchType, categoryId },
    })
    const idx = rules.value.findIndex(r => r.id === id)
    if (idx !== -1) rules.value[idx] = updated
  }

  async function deleteRule(id: string): Promise<void> {
    await apiFetch(`/category-rules/${id}`, { method: 'DELETE' })
    rules.value = rules.value.filter(r => r.id !== id)
  }

  return { rules, loading, fetchRules, createRule, updateRule, deleteRule }
}
