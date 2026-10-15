import type { Frequency, RentPeriod } from '../../shared/api/types'
import { formatShortDate } from '../../shared/format/date'

export const FREQUENCY_LABELS: Record<Frequency, string> = {
  weekly: 'Weekly',
  fortnightly: 'Fortnightly',
  monthly: 'Monthly',
  quarterly: 'Quarterly',
}

/** "15 Oct – 21 Oct" */
export function periodRange(period: RentPeriod) {
  return `${formatShortDate(period.startDate)} – ${formatShortDate(period.endDate)}`
}
