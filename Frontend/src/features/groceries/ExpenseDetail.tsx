import { Avatar } from '../../shared/components/Avatar'
import { Button } from '../../shared/components/Button'
import { List, Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { SheetActions } from '../../shared/components/Sheet'
import { Note } from '../../shared/components/Text'
import { formatDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import styles from './Groceries.module.css'
import type { Expense } from './types'

interface ExpenseDetailProps {
  expense: Expense
  /** Only the person who added an expense may change it. */
  canChange: boolean
  onEdit: () => void
  onDelete: () => void
}

export function ExpenseDetail({ expense, canChange, onEdit, onDelete }: ExpenseDetailProps) {
  return (
    <>
      <div className={styles.bigAmount}>{formatMoney(expense.amount)}</div>
      <Note>
        {expense.paidBy.name} paid on {formatDate(expense.date)} ·{' '}
        {expense.splitType === 'equal' ? 'split equally' : 'exact amounts'}
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
      {canChange ? (
        <SheetActions>
          <Button variant="primary" block onClick={onEdit}>
            Edit
          </Button>
          <Button variant="quietDanger" block onClick={onDelete}>
            Delete
          </Button>
        </SheetActions>
      ) : (
        <p className={styles.centerNote}>Added by {expense.createdBy.name}. Only they can change it.</p>
      )}
    </>
  )
}
