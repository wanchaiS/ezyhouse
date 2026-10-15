import type { RentPeriod } from '../../shared/api/types'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { List } from '../../shared/components/List'
import { Sheet } from '../../shared/components/Sheet'
import { Note } from '../../shared/components/Text'
import { formatMoney } from '../../shared/format/money'
import { periodRange } from './format'
import { ShareRows } from './ShareRows'
import styles from './Rent.module.css'

interface PeriodSheetProps {
  /** null keeps the sheet closed. */
  period: RentPeriod | null
  onClose: () => void
  meId: number
  isPrimary: boolean
  busy: boolean
  /** The last tick's failure, shown inside the sheet so it isn't hidden by the backdrop. */
  tickError: unknown
  onTick: (userId: number, paid: boolean) => void
}

export function PeriodSheet({ period, onClose, tickError, ...shareProps }: PeriodSheetProps) {
  return (
    <Sheet open={period !== null} onClose={onClose} title={period ? `Rent ${periodRange(period)}` : ''}>
      {period && (
        <>
          <div className={styles.bigAmount}>{formatMoney(period.amount)}</div>
          <Note>Pay {period.payee.name}</Note>
          <ErrorMessage error={tickError} />
          <List>
            <ShareRows shares={period.shares} {...shareProps} />
          </List>
        </>
      )}
    </Sheet>
  )
}
