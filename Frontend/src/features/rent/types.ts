import type { Frequency, ShareInput, SplitType } from '../../shared/api/types'

export interface RentScheduleInput {
  amount: number
  frequency: Frequency
  startDate: string
  endDate: string | null
  splitType: SplitType
  shares: ShareInput[]
}
