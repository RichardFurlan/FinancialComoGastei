type Options = {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
}

export async function apiFetch<T>(path: string, options: Options = {}): Promise<T> {
  const config = useRuntimeConfig()
  const base = `${config.public.apiBase}/api`

  const response = await fetch(`${base}${path}`, {
    method: options.method ?? 'GET',
    credentials: 'include',
    headers: options.body instanceof FormData
      ? undefined
      : { 'Content-Type': 'application/json' },
    body: options.body instanceof FormData
      ? options.body
      : options.body !== undefined
        ? JSON.stringify(options.body)
        : undefined,
  })

  if (!response.ok) {
    const msg = await response.text().catch(() => `Erro ${response.status}`)
    throw new Error(msg || `Erro ${response.status}`)
  }

  if (response.status === 204 || response.status === 202) {
    return response.json().catch(() => undefined) as T
  }

  return response.json() as T
}

export const formatCurrency = (value: number, currency = 'BRL') =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency }).format(value || 0)

export const formatDate = (date: string | Date) =>
  new Date(date).toLocaleDateString('pt-BR')
