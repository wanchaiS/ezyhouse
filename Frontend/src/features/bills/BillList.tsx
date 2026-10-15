import type { Bill, BillStatus } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { EmptyState } from '../../shared/components/EmptyState'
import { BillIcon } from '../../shared/components/icons'
import { List, Row } from '../../shared/components/List'
import { Pill } from '../../shared/components/Pill'
import { formatShortDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import styles from './Bills.module.css'

interface BillListProps {
  status: BillStatus
  bills: Bill[]
  meId: number
  isPrimary: boolean
  primaryName: string
  /** Opens the Submit sheet for a pending bill (primary only). */
  onEnter: (bill: Bill) => void
  /** Opens a submitted bill's shares. */
  onView: (bill: Bill) => void
}

const icon = (
  <span className={styles.iconCircle}>
    <BillIcon />
  </span>
)

/** The pending placeholders or the submitted history, depending on the filter. */
export function BillList({ status, bills, meId, isPrimary, primaryName, onEnter, onView }: BillListProps) {
  if (!bills.length) {
    return <EmptyState>{status === 'pending' ? 'No bills waiting for an amount.' : 'No bills submitted yet.'}</EmptyState>
  }
  if (status === 'pending') {
    return (
      <List>
        {bills.map((bill) => (
          <Row
            key={bill.id}
            lead={icon}
            title={bill.billType.name}
            sub={`Due ${formatShortDate(bill.dueDate)} · no amount yet`}
            end={isPrimary ? undefined : <span className={styles.waiting}>Waiting for {primaryName}</span>}
            action={
              isPrimary && (
                <Button variant="primary" small onClick={() => onEnter(bill)}>
                  Enter
                </Button>
              )
            }
          />
        ))}
      </List>
    )
  }
  return (
    <List>
      {bills.map((bill) => (
        <Row
          key={bill.id}
          lead={icon}
          title={bill.billType.name}
          sub={`${formatShortDate(bill.dueDate)} · ${formatMoney(bill.amount ?? 0)}`}
          end={<ShareStatus bill={bill} meId={meId} />}
          onClick={() => onView(bill)}
        />
      ))}
    </List>
  )
}

/** The payee sees how many shares are unpaid; everyone else sees their own share. */
function ShareStatus({ bill, meId }: { bill: Bill; meId: number }) {
  if (bill.payee?.id === meId) {
    const unpaid = bill.shares.filter((share) => !share.paid && share.user.id !== meId).length
    return unpaid ? <Pill tone="warn">{unpaid} unpaid</Pill> : <Pill tone="pos">All paid</Pill>
  }
  const mine = bill.shares.find((share) => share.user.id === meId)
  if (!mine) return <Pill>Not yours</Pill>
  return mine.paid ? <Pill tone="pos">Paid</Pill> : <Pill tone="neg">You owe {formatMoney(mine.amount)}</Pill>
}
