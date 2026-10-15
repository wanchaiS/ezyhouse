import type { RentPeriod } from '../../shared/api/types'
import { List } from '../../shared/components/List'
import { formatMoney } from '../../shared/format/money'
import { periodRange } from './format'
import { ShareRows } from './ShareRows'
import styles from './Rent.module.css'

interface CurrentPeriodProps {
  period: RentPeriod
  meId: number
  isPrimary: boolean
  busy: boolean
  onTick: (userId: number, paid: boolean) => void
}

export function CurrentPeriod({ period, ...shareProps }: CurrentPeriodProps) {
  return (
    <List
      head={
        <>
          <div className={styles.label}>
            {periodRange(period)} · pay {period.payee.name}
          </div>
          <div className={styles.big}>{formatMoney(period.amount)}</div>
        </>
      }
    >
      <ShareRows shares={period.shares} {...shareProps} />
    </List>
  )
}
