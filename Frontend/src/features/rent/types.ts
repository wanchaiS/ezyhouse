import type { Frequency, Share, ShareInput, SplitType } from '../../shared/api/types'

export interface RentSchedule {
  id: number
  amount: number
  frequency: Frequency
  startDate: string
  endDate: string | null
  splitType: SplitType
  shares: Share[]
  /** null once the schedule has ended. */
  nextDueDate: string | null
}

export interface RentScheduleInput {
  amount: number
  frequency: Frequency
  startDate: string
  endDate: string | null
  splitType: SplitType
  shares: ShareInput[]
}
