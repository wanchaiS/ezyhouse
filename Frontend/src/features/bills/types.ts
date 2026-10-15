import type { Frequency, ShareInput, SplitType, UserRef } from '../../shared/api/types'

export interface BillType {
  id: number
  name: string
  frequency: Frequency
  startDate: string
  endDate: string | null
  members: UserRef[]
  nextDueDate: string | null
}

export interface BillTypeInput {
  name: string
  frequency: Frequency
  startDate: string
  endDate?: string | null
  memberIds: number[]
}

/** equal → the server splits across the bill type's members, so shares are omitted. */
export interface SubmitBillInput {
  amount: number
  splitType: SplitType
  shares?: ShareInput[]
}

export const FREQUENCY_LABELS: Record<Frequency, string> = {
  weekly: 'Weekly',
  fortnightly: 'Fortnightly',
  monthly: 'Monthly',
  quarterly: 'Quarterly',
}
