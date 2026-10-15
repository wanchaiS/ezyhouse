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
import { PlusIcon, SofaIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Spinner } from '../../shared/components/Spinner'
import { Lead } from '../../shared/components/Text'
import styles from './Furniture.module.css'

export function FurniturePage() {
  const me = useMe().data
  const household = useHousehold()
  const expenses = useQuery({ queryKey: keys.expenseList('furniture'), queryFn: () => getExpenses('furniture') })
  const [sheet, setSheet] = useState<ExpenseSheetState>(null)

  const failed = [household, expenses].find((query) => query.isError)
  if (failed) {
    return (
      <>
        <PageHeader title="Furniture" />
        <ErrorMessage error={failed.error} />
      </>
    )
  }
  if (!me || !household.data || !expenses.data) return <Spinner />

  const { primary, members } = household.data
  // Only the primary adds furniture (the API answers 403 to anyone else).
  const isPrimary = primary.id === me.id
  const add = () => setSheet({ kind: 'add' })

  return (
    <>
      <PageHeader
        title="Furniture"
        action={
          isPrimary && (
            <IconButton label="Add furniture" accent onClick={add}>
              <PlusIcon />
            </IconButton>
          )
        }
      />
      <div className={styles.page}>
        {expenses.data.length ? (
          <>
            <Lead>
              {isPrimary
                ? 'What the house buys together, split between you. Settle up on Home.'
                : `${primary.name} adds what the house buys together. Settle up on Home.`}
            </Lead>
            <ExpenseLog expenses={expenses.data} meId={me.id} onOpen={(expense) => setSheet({ kind: 'view', expense })} />
          </>
        ) : (
          <FirstRun
            icon={<SofaIcon />}
            title="No furniture yet"
            action={
              isPrimary && (
                <Button variant="primary" onClick={add}>
                  Add furniture
                </Button>
              )
            }
          >
            {isPrimary
              ? 'When the house buys something to share, like a couch or a fridge, add it here and split it between you.'
              : `${primary.name} adds the furniture your house shares.`}
          </FirstRun>
        )}
      </div>
      <ExpenseSheet
        state={sheet}
        onChange={setSheet}
        tag="furniture"
        members={members}
        meId={me.id}
        primaryName={primary.name}
      />
    </>
  )
}
