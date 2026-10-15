import type { RentPeriod } from '../../shared/api/types'
import { Pill } from '../../shared/components/Pill'
import { formatMoney } from '../../shared/format/money'

/** The payee sees how many others still owe; everyone else sees their own share. */
export function PeriodStatus({ period, meId }: { period: RentPeriod; meId: number }) {
  if (period.payee.id === meId) {
    const open = period.shares.filter((share) => !share.paid && share.user.id !== meId).length
    return open ? <Pill tone="warn">{open} unpaid</Pill> : <Pill tone="pos">All paid</Pill>
  }
  const mine = period.shares.find((share) => share.user.id === meId)
  if (!mine) return <Pill>Not yours</Pill>
  return mine.paid ? <Pill tone="pos">Paid</Pill> : <Pill tone="neg">You owe {formatMoney(mine.amount)}</Pill>
}
