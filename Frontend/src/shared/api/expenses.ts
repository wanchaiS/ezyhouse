import { api } from './client'
import type { Expense, ExpenseInput, ExpenseTag, NewExpenseInput } from './types'

/** Newest first; every tag when `tag` is omitted. */
export async function getExpenses(tag?: ExpenseTag) {
  return (await api.get<Expense[]>('/expenses', { params: { tag } })).data
}

export async function getExpense(id: number) {
  return (await api.get<Expense>(`/expenses/${id}`)).data
}

export async function createExpense(input: NewExpenseInput) {
  return (await api.post<Expense>('/expenses', input)).data
}

export async function updateExpense(id: number, input: ExpenseInput) {
  return (await api.put<Expense>(`/expenses/${id}`, input)).data
}

export async function deleteExpense(id: number) {
  await api.delete(`/expenses/${id}`)
}
