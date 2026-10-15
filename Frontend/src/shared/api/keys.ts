import type { BillStatus, FurnitureStatus } from './types'

// Query keys shared by two or more slices. Invalidating a base key (e.g. keys.bills)
// also refreshes every filtered list under it.
export const keys = {
  me: ['me'],
  household: ['household'],
  balances: ['balances'],
  settlements: ['settlements'],
  rentPeriods: ['rentPeriods'],
  bills: ['bills'],
  billList: (status?: BillStatus) => (status ? ['bills', status] : ['bills']),
  furniture: ['furniture'],
  furnitureList: (status?: FurnitureStatus) => (status ? ['furniture', status] : ['furniture']),
  bonds: ['bonds'],
} as const
