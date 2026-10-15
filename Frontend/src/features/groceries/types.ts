import type { Share, ShareInput, SplitType, UserRef } from '../../shared/api/types'

export interface Expense {
  id: number
  description: string
  amount: number
  date: string
  paidBy: UserRef
  createdBy: UserRef
  splitType: SplitType
  shares: Share[]
}

export interface ExpenseInput {
  description: string
  amount: number
  date: string
  paidByUserId: number
  splitType: SplitType
  shares: ShareInput[]
}
