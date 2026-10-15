import type { UserRef } from '../../shared/api/types'

export interface BalanceMember {
  user: UserRef
  /** > 0: the household owes you; < 0: you owe. */
  net: number
}

/** One balance between two people: `from` owes `to`. */
export interface Debt {
  from: UserRef
  to: UserRef
  amount: number
}

export interface Balances {
  members: BalanceMember[]
  debts: Debt[]
}

export interface Settlement {
  id: number
  from: UserRef
  to: UserRef
  amount: number
  date: string
  createdBy: UserRef
}

export interface SettlementInput {
  fromUserId: number
  toUserId: number
  amount: number
}
