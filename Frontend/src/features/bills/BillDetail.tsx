import type { Bill } from '../../shared/api/types'
import { Avatar } from '../../shared/components/Avatar'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { List, Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { Note } from '../../shared/components/Text'
import { Tick } from '../../shared/components/Tick'
import { formatDate, formatShortDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import styles from './Bills.module.css'

interface BillDetailProps {
  bill: Bill
  meId: number
  isPrimary: boolean
  onTick: (userId: number, paid: boolean) => void
  /** Disables the ticks while a tick is saving. */
  saving: boolean
  error?: unknown
}

/** A submitted bill: its amount and each share with its paid tick. */
export function BillDetail({ bill, meId, isPrimary, onTick, saving, error }: BillDetailProps) {
  const payee = bill.payee?.name ?? 'the primary'
  return (
    <>
      <div className={styles.bigAmount}>{formatMoney(bill.amount ?? 0)}</div>
      <Note>
        Due {formatDate(bill.dueDate)}
        {bill.submittedDate && ` · ${payee} paid it on ${formatDate(bill.submittedDate)}`}
      </Note>
      <List>
        {bill.shares.map((share) => (
          <Row
            key={share.user.id}
            lead={<Avatar user={share.user} />}
            title={
              <>
                {share.user.name}
                {share.user.id === meId && <span className={styles.muted}> (you)</span>}
              </>
            }
            sub={share.paid && share.paidDate ? `Paid ${formatShortDate(share.paidDate)}` : 'Not paid yet'}
            end={<Money amount={share.amount} />}
            action={
              <Tick
                paid={share.paid}
                // That person or the primary may tick; the API still has the final say.
                disabled={saving || !(isPrimary || share.user.id === meId)}
                onChange={(paid) => onTick(share.user.id, paid)}
              />
            }
          />
        ))}
      </List>
      <ErrorMessage error={error} />
      <Note>Tick your share once you've paid {payee} back.</Note>
    </>
  )
}
