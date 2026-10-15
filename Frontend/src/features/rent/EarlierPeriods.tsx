import type { RentPeriod } from '../../shared/api/types'
import { List, Row } from '../../shared/components/List'
import { formatMoney } from '../../shared/format/money'
import { periodRange } from './format'
import { PeriodStatus } from './PeriodStatus'

interface EarlierPeriodsProps {
  periods: RentPeriod[]
  meId: number
  onOpen: (period: RentPeriod) => void
}

export function EarlierPeriods({ periods, meId, onOpen }: EarlierPeriodsProps) {
  return (
    <List>
      {periods.map((period) => (
        <Row
          key={period.id}
          title={periodRange(period)}
          sub={`${formatMoney(period.amount)} · pay ${period.payee.name}`}
          end={<PeriodStatus period={period} meId={meId} />}
          onClick={() => onOpen(period)}
        />
      ))}
    </List>
  )
}
