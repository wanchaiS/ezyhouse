import type { Expense } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { BillIcon } from '../../shared/components/icons'
import { List, Row } from '../../shared/components/List'
import { formatShortDate } from '../../shared/format/date'
import styles from './Home.module.css'

interface BillsToEnterProps {
  /** Bills still at $0, soonest due first. */
  bills: Expense[]
  onEnter: (bill: Expense) => void
}

/** For the primary: bills that have come due and are waiting for what the provider charged. */
export function BillsToEnter({ bills, onEnter }: BillsToEnterProps) {
  return (
    <List>
      {bills.map((bill) => (
        <Row
          key={bill.id}
          lead={
            <span className={styles.iconCircle}>
              <BillIcon />
            </span>
          }
          title={bill.description}
          sub={`due ${formatShortDate(bill.date)}`}
          action={
            <Button small variant="primary" onClick={() => onEnter(bill)}>
              Enter amount
            </Button>
          }
        />
      ))}
    </List>
  )
}
