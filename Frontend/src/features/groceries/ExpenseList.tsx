import { EmptyState } from '../../shared/components/EmptyState'
import { List, Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { SectionTitle } from '../../shared/components/Text'
import { formatMoney } from '../../shared/format/money'
import { DateBadge } from './DateBadge'
import listStyles from './Groceries.module.css'
import type { Expense } from './types'

interface ExpenseListProps {
  /** Newest first. */
  expenses: Expense[]
  meId: number
  onOpen: (expense: Expense) => void
}

const monthName = new Intl.DateTimeFormat('en-AU', { month: 'long', year: 'numeric' })

/** Expenses grouped by month, each row showing what it means for the viewer. */
export function ExpenseList({ expenses, meId, onOpen }: ExpenseListProps) {
  if (!expenses.length) return <EmptyState>No expenses yet. Tap + to add one.</EmptyState>

  const months = new Map<string, Expense[]>()
  for (const expense of expenses) {
    const month = expense.date.slice(0, 7)
    months.set(month, [...(months.get(month) ?? []), expense])
  }

  return (
    <>
      {[...months].map(([month, list]) => {
        const [year, monthNumber] = month.split('-').map(Number)
        return (
          <div key={month}>
            <SectionTitle>{monthName.format(new Date(year, monthNumber - 1, 1))}</SectionTitle>
            <List>
              {list.map((expense) => (
                <Row
                  key={expense.id}
                  lead={<DateBadge date={expense.date} />}
                  title={expense.description}
                  sub={`${expense.paidBy.name} paid ${formatMoney(expense.amount)}`}
                  end={<YourPart expense={expense} meId={meId} />}
                  onClick={() => onOpen(expense)}
                />
              ))}
            </List>
          </div>
        )
      })}
    </>
  )
}

function YourPart({ expense, meId }: { expense: Expense; meId: number }) {
  const myShare = expense.shares.find((share) => share.user.id === meId)?.amount ?? 0
  if (expense.paidBy.id === meId) {
    const lent = expense.amount - myShare
    if (!lent) return <span className={listStyles.sub}>just you</span>
    return (
      <>
        <div className={listStyles.sub}>you lent</div>
        <Money amount={lent} tone="pos" />
      </>
    )
  }
  if (!myShare) return <span className={listStyles.sub}>not involved</span>
  return (
    <>
      <div className={listStyles.sub}>you owe</div>
      <Money amount={myShare} tone="neg" />
    </>
  )
}
