import type { ShareInput, SplitType } from '../../shared/api/types'

export interface FurnitureInput {
  name: string
  purchase: { date: string; amount: number; paidByUserId: number; splitType: SplitType; shares: ShareInput[] }
}

export interface SaleInput {
  date: string
  amount: number
  receivedByUserId: number
  splitType: SplitType
  shares: ShareInput[]
}

/** `amount` may be 0. */
export interface DisposalInput {
  date: string
  amount: number
  paidByUserId: number
  splitType: SplitType
  shares: ShareInput[]
}
