import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { getExpenses } from '../../shared/api/expenses'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import { Button, IconButton } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { ExpenseLog } from '../../shared/components/ExpenseLog'
import { ExpenseSheet, type ExpenseSheetState } from '../../shared/components/ExpenseSheet'
import { FirstRun } from '../../shared/components/FirstRun'
import { CartIcon, PlusIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Spinner } from '../../shared/components/Spinner'
import { Lead } from '../../shared/components/Text'
import styles from './Groceries.module.css'

export function GroceriesPage() {
  const me = useMe().data
  const household = useHousehold()
  const expenses = useQuery({ queryKey: keys.expenseList('groceries'), queryFn: () => getExpenses('groceries') })
  const [sheet, setSheet] = useState<ExpenseSheetState>(null)

  const failed = [household, expenses].find((query) => query.isError)
  if (failed) {
    return (
      <>
        <PageHeader title="Groceries" />
        <ErrorMessage error={failed.error} />
      </>
    )
  }
  if (!me || !household.data || !expenses.data) return <Spinner />

  const add = () => setSheet({ kind: 'add' })

  return (
    <>
      <PageHeader
        title="Groceries"
        action={
          <IconButton label="Add expense" accent onClick={add}>
            <PlusIcon />
          </IconButton>
        }
      />
      <div className={styles.page}>
        {expenses.data.length ? (
          <>
            <Lead>Add what you buy for the house. Settle up on Home.</Lead>
            <ExpenseLog expenses={expenses.data} meId={me.id} onOpen={(expense) => setSheet({ kind: 'view', expense })} />
          </>
        ) : (
          <FirstRun
            icon={<CartIcon />}
            title="No groceries yet"
            action={
              <Button variant="primary" onClick={add}>
                Add an expense
              </Button>
            }
          >
            When you buy something for the house (food, cleaning, takeaway), add it here. Everyone’s share is worked
            out for you, and you settle up with each person on Home.
          </FirstRun>
        )}
      </div>
      <ExpenseSheet
        state={sheet}
        onChange={setSheet}
        tag="groceries"
        members={household.data.members}
        meId={me.id}
        primaryName={household.data.primary.name}
      />
    </>
  )
}
