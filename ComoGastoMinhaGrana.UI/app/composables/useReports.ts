export interface ReportSummaryDto {
  id: string
  name: string
  createdAt: string
  statementCount: number
}

export interface CurrencySummaryDto {
  currency: string
  debits: number
  credits: number
  balance: number
}

export interface CategorySummaryDto {
  name: string
  color: string
  total: number
}

export interface ImportCategoryComparisonDto {
  statementId: string
  fileName: string
  topCategories: CategorySummaryDto[]
}

export interface TopExpenseDto {
  date: string
  description: string
  amount: number
  currency: string
  categoryName: string | null
}

export interface StatementSummaryInReport {
  id: string
  fileName: string
  uploadDate: string
  status: string
  transactionCount: number
  hasAnalysis: boolean
}

export interface ReportDetailDto {
  id: string
  name: string
  createdAt: string
  totalTransactions: number
  currencies: CurrencySummaryDto[]
  categoryTotals: CategorySummaryDto[]
  byImport: ImportCategoryComparisonDto[]
  topExpenses: TopExpenseDto[]
  statements: StatementSummaryInReport[]
}

export function useReports() {
  const reports = ref<ReportSummaryDto[]>([])
  const loading = ref(false)

  async function fetchReports() {
    loading.value = true
    try {
      reports.value = await apiFetch<ReportSummaryDto[]>('/reports')
    } finally {
      loading.value = false
    }
  }

  async function createReport(name: string, statementIds: string[]): Promise<ReportSummaryDto> {
    const created = await apiFetch<ReportSummaryDto>('/reports', {
      method: 'POST',
      body: { name, statementIds },
    })
    reports.value.unshift(created)
    return created
  }

  async function deleteReport(id: string): Promise<void> {
    await apiFetch(`/reports/${id}`, { method: 'DELETE' })
    reports.value = reports.value.filter(r => r.id !== id)
  }

  async function fetchReportDetail(id: string): Promise<ReportDetailDto> {
    return apiFetch<ReportDetailDto>(`/reports/${id}`)
  }

  return { reports, loading, fetchReports, createReport, deleteReport, fetchReportDetail }
}
