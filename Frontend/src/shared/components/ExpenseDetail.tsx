import type { Expense } from '../api/types'
import { formatDate } from '../format/date'
import { formatMoney } from '../format/money'
import { Avatar } from './Avatar'
import { Button } from './Button'
import styles from './Expense.module.css'
import { List, Row } from './List'
import { Money } from './Money'
import { SheetActions } from './Sheet'
import { Note } from './Text'

interface ExpenseDetailProps {
  expense: Expense
  /** Named when someone else can't change it: rent, bills and furniture belong to the primary. */
  primaryName: string
  onEdit: () => void
  onDelete: () => void
}

export function ExpenseDetail({ expense, primaryName, onEdit, onDelete }: ExpenseDetailProps) {
  const waiting = expense.amount === 0 && expense.tag === 'bills'
  return (
    <>
      <div className={styles.bigAmount}>{formatMoney(expense.amount)}</div>
      <Note>
        {waiting
          ? `Due ${formatDate(expense.date)} · the amount hasn’t been entered yet`
          : `${expense.paidBy.name} paid on ${formatDate(expense.date)} · ${expense.splitType === 'equal' ? 'split equally' : 'exact amounts'}`}
      </Note>
      <List>
        {expense.shares.map((share) => (
          <Row
            key={share.user.id}
            lead={<Avatar user={share.user} />}
            title={share.user.name}
            end={<Money amount={share.amount} />}
          />
        ))}
      </List>
      {expense.canChange ? (
        <SheetActions>
          <Button variant="primary" block onClick={onEdit}>
            {waiting ? 'Enter amount' : 'Edit'}
          </Button>
          <Button variant="quietDanger" block onClick={onDelete}>
            Delete
          </Button>
        </SheetActions>
      ) : (
        <p className={styles.centerNote}>
          {expense.tag === 'groceries'
            ? `Added by ${expense.createdBy.name}. Only they can change it.`
            : `Only ${primaryName} (primary) can change it.`}
        </p>
      )}
    </>
  )
}
