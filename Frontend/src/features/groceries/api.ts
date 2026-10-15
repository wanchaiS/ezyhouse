import { api } from '../../shared/api/client'
import type { Settlement } from '../../shared/api/types'
import type { Expense, ExpenseInput } from './types'

// Only this slice reads expenses, so the key lives here rather than in shared/api/keys.
export const expensesKey = ['expenses'] as const

export async function getExpenses() {
  return (await api.get<Expense[]>('/expenses')).data
}

export async function createExpense(input: ExpenseInput) {
  return (await api.post<Expense>('/expenses', input)).data
}

export async function updateExpense(id: number, input: ExpenseInput) {
  return (await api.put<Expense>(`/expenses/${id}`, input)).data
}

export async function deleteExpense(id: number) {
  await api.delete(`/expenses/${id}`)
}

export async function getSettlements() {
  return (await api.get<Settlement[]>('/settlements')).data
}
