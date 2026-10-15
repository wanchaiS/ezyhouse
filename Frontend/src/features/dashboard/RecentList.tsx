import type { Expense, UserRef } from '../../shared/api/types'
import { YourShare } from '../../shared/components/ExpenseLog'
import { CheckIcon } from '../../shared/components/icons'
import { List, Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { formatShortDate } from '../../shared/format/date'
import { expenseTitle } from '../../shared/format/expense'
import { formatMoney } from '../../shared/format/money'
import styles from './Home.module.css'
import { TAGS } from './tags'
import type { Settlement } from './types'

export type Activity =
  | { kind: 'expense'; date: string; id: number; expense: Expense }
  | { kind: 'payment'; date: string; id: number; settlement: Settlement }

interface RecentListProps {
  /** Newest first. */
  items: Activity[]
  meId: number
}

/** The latest expenses of every tag and the latest payments, as one list. */
export function RecentList({ items, meId }: RecentListProps) {
  const name = (user: UserRef, start: boolean) => (user.id === meId ? (start ? 'You' : 'you') : user.name)
  return (
    <List>
      {items.map((item) => {
        if (item.kind === 'payment') {
          const { settlement } = item
          // Paid-back money steps back: a quiet tick, so what was spent stands out.
          return (
            <Row
              key={`p${item.id}`}
              lead={
                <span className={`${styles.iconCircle} ${styles.quietIcon}`}>
                  <CheckIcon />
                </span>
              }
              title={
                <span className={styles.payment}>
                  {name(settlement.from, true)} paid {name(settlement.to, false)} back
                </span>
              }
              sub={formatShortDate(settlement.date)}
              end={<Money amount={settlement.amount} tone="muted" />}
            />
          )
        }
        const { expense } = item
        const waiting = expense.tag === 'bills' && expense.amount === 0
        return (
          <Row
            key={`e${item.id}`}
            lead={<span className={styles.iconCircle}>{TAGS[expense.tag].icon}</span>}
            title={expenseTitle(expense)}
            sub={
              waiting
                ? `Amount not entered yet · due ${formatShortDate(expense.date)}`
                : `${name(expense.paidBy, true)} paid ${formatMoney(expense.amount)} · ${formatShortDate(expense.date)}`
            }
            end={waiting ? undefined : <YourShare expense={expense} meId={meId} />}
            to={TAGS[expense.tag].to}
          />
        )
      })}
    </List>
  )
}
