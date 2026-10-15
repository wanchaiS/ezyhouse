import type { ExpenseTag } from './types'

// Query keys shared by two or more slices. Invalidating a base key (e.g. keys.expenses)
// also refreshes every filtered list under it.
export const keys = {
  me: ['me'],
  household: ['household'],
  balances: ['balances'],
  settlements: ['settlements'],
  expenses: ['expenses'],
  expenseList: (tag?: ExpenseTag) => (tag ? ['expenses', tag] : ['expenses']),
  rentSchedules: ['rentSchedules'],
  billTypes: ['billTypes'],
  bonds: ['bonds'],
} as const
