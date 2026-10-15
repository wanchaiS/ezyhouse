import { useState } from 'react'
import type { Expense } from '../api/types'
import { expenseTitle } from '../format/expense'
import { formatMoney } from '../format/money'
import { DateBadge } from './DateBadge'
import styles from './Expense.module.css'
import { List, Row } from './List'
import { Money } from './Money'
import { SectionTitle } from './Text'

interface ExpenseLogProps {
  /** Newest first. */
  expenses: Expense[]
  meId: number
  onOpen: (expense: Expense) => void
}

const SHOWN = 10
const monthName = new Intl.DateTimeFormat('en-AU', { month: 'long', year: 'numeric' })

/** One tab's expenses by month, newest first; the latest ten, then the rest on request. */
export function ExpenseLog({ expenses, meId, onOpen }: ExpenseLogProps) {
  const [showAll, setShowAll] = useState(false)

  const months = new Map<string, Expense[]>()
  for (const expense of showAll ? expenses : expenses.slice(0, SHOWN)) {
    const month = expense.date.slice(0, 7)
    months.set(month, [...(months.get(month) ?? []), expense])
  }

  return (
    <div>
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
                  title={expenseTitle(expense)}
                  sub={`${expense.paidBy.id === meId ? 'You' : expense.paidBy.name} paid ${formatMoney(expense.amount)}`}
                  end={<YourShare expense={expense} meId={meId} />}
                  onClick={() => onOpen(expense)}
                />
              ))}
            </List>
          </div>
        )
      })}
      {expenses.length > SHOWN && (
        <button type="button" className={styles.more} onClick={() => setShowAll(!showAll)}>
          {showAll ? 'Show fewer' : `Show ${expenses.length - SHOWN} earlier`}
        </button>
      )}
    </div>
  )
}

/**
 * Paying back happens per person across every expense, never per expense, so a row states the
 * viewer's share as a fact of that expense; "you owe" would stay after they'd settled up.
 */
export function YourShare({ expense, meId }: { expense: Expense; meId: number }) {
  const mine = expense.shares.find((share) => share.user.id === meId)
  if (!mine) return <span className={styles.endNote}>not in the split</span>
  if (expense.shares.every((share) => share.user.id === meId)) return <span className={styles.endNote}>just you</span>
  return (
    <>
      <div className={styles.endNote}>your share</div>
      <Money amount={mine.amount} />
    </>
  )
}
