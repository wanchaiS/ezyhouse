import type { Expense } from '../api/types'
import { formatShortDate } from './date'

/**
 * What a row calls an expense. Rent from a schedule also says which days it covers:
 * "Rent · 13–19 Oct", or "Rent · 28 Oct – 3 Nov" across a month end.
 */
export function expenseTitle(expense: Expense) {
  if (!expense.periodEnd) return expense.description
  const [startDay, startMonth] = formatShortDate(expense.date).split(' ')
  const end = formatShortDate(expense.periodEnd)
  const range = end.endsWith(` ${startMonth}`) ? `${startDay}–${end}` : `${startDay} ${startMonth} – ${end}`
  return `${expense.description} · ${range}`
}
