import type { Frequency } from '../../shared/api/types'

export interface BillTypeInput {
  name: string
  frequency: Frequency
  startDate: string
  endDate?: string | null
  memberIds: number[]
}

export const FREQUENCY_LABELS: Record<Frequency, string> = {
  weekly: 'Weekly',
  fortnightly: 'Fortnightly',
  monthly: 'Monthly',
  quarterly: 'Quarterly',
}
