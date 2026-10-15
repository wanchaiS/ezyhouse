import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { getBalances } from '../../shared/api/balances'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import { Button, IconButton } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { PlusIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { SettleUpSheet } from '../../shared/components/SettleUpSheet'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Spinner } from '../../shared/components/Spinner'
import { Note } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import { createExpense, deleteExpense, expensesKey, getExpenses, getSettlements, updateExpense } from './api'
import { BalancesCard } from './BalancesCard'
import { ExpenseDetail } from './ExpenseDetail'
import { ExpenseForm } from './ExpenseForm'
import { ExpenseList } from './ExpenseList'
import styles from './Groceries.module.css'
import { SettlementHistory } from './SettlementHistory'
import type { ExpenseInput } from './types'

type ExpenseSheet =
  | { kind: 'add' }
  | { kind: 'view' | 'edit' | 'delete'; id: number }

const newestFirst = (a: { date: string; id: number }, b: { date: string; id: number }) =>
  b.date.localeCompare(a.date) || b.id - a.id

export function GroceriesPage() {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const me = useMe().data
  const household = useHousehold()
  const expenses = useQuery({ queryKey: expensesKey, queryFn: getExpenses })
  const balances = useQuery({ queryKey: keys.balances, queryFn: getBalances })
  const settlements = useQuery({ queryKey: keys.settlements, queryFn: getSettlements })
  const [sheet, setSheet] = useState<ExpenseSheet | null>(null)
  const [settling, setSettling] = useState(false)

  function finish(message: string) {
    queryClient.invalidateQueries({ queryKey: expensesKey })
    queryClient.invalidateQueries({ queryKey: keys.balances })
    // Close the sheet first: a toast under an open dialog is hidden.
    setSheet(null)
    showToast(message)
  }

  const save = useMutation({
    mutationFn: ({ id, input }: { id?: number; input: ExpenseInput }) =>
      id === undefined ? createExpense(input) : updateExpense(id, input),
    onSuccess: (_, { id }) => finish(id === undefined ? 'Expense added' : 'Expense saved'),
  })
  const remove = useMutation({
    mutationFn: deleteExpense,
    onSuccess: () => finish('Expense deleted'),
  })

  const failed = [household, expenses, balances, settlements].find((query) => query.isError)
  if (failed) {
    return (
      <>
        <PageHeader title="Groceries" />
        <ErrorMessage error={failed.error} />
      </>
    )
  }
  if (!me || !household.data || !expenses.data || !balances.data || !settlements.data) return <Spinner />

  const members = household.data.members
  const sorted = [...expenses.data].sort(newestFirst)
  const selected = sheet && sheet.kind !== 'add' ? sorted.find((expense) => expense.id === sheet.id) : undefined
  // A selected expense that has gone (e.g. deleted by its creator elsewhere) closes the sheet.
  const open = sheet !== null && (sheet.kind === 'add' || selected !== undefined)

  // Start the settle-up form from the viewer's own debt if there is one; the API only lets
  // the payer or the payee record a settlement.
  const myDebt =
    balances.data.debts.find((debt) => debt.from.id === me.id) ??
    balances.data.debts.find((debt) => debt.to.id === me.id)

  function closeSheet() {
    setSheet(null)
    save.reset()
    remove.reset()
  }

  return (
    <>
      <PageHeader
        title="Groceries"
        action={
          <IconButton label="Add expense" accent onClick={() => setSheet({ kind: 'add' })}>
            <PlusIcon />
          </IconButton>
        }
      />
      <div className={styles.layout}>
        <div className={styles.balances}>
          <BalancesCard balances={balances.data} meId={me.id} onSettle={() => setSettling(true)} />
        </div>
        <div className={styles.expenses}>
          <ExpenseList
            expenses={sorted}
            meId={me.id}
            onOpen={(expense) => setSheet({ kind: 'view', id: expense.id })}
          />
        </div>
        <div className={styles.settlements}>
          <SettlementHistory settlements={[...settlements.data].sort(newestFirst)} />
        </div>
      </div>

      <Sheet
        open={open}
        onClose={closeSheet}
        title={
          sheet?.kind === 'add'
            ? 'Add expense'
            : sheet?.kind === 'edit'
              ? 'Edit expense'
              : sheet?.kind === 'delete'
                ? 'Delete this expense?'
                : selected?.description
        }
      >
        {sheet?.kind === 'add' && (
          <ExpenseForm
            members={members}
            meId={me.id}
            onSave={(input) => save.mutateAsync({ input })}
            onCancel={closeSheet}
          />
        )}
        {sheet?.kind === 'view' && selected && (
          <ExpenseDetail
            expense={selected}
            canChange={selected.createdBy.id === me.id}
            onEdit={() => setSheet({ kind: 'edit', id: selected.id })}
            onDelete={() => setSheet({ kind: 'delete', id: selected.id })}
          />
        )}
        {sheet?.kind === 'edit' && selected && (
          <ExpenseForm
            members={members}
            meId={me.id}
            expense={selected}
            onSave={(input) => save.mutateAsync({ id: selected.id, input })}
            onCancel={closeSheet}
          />
        )}
        {sheet?.kind === 'delete' && selected && (
          <>
            <Note>
              {selected.description} is removed for everyone, and the balances change straight away.
            </Note>
            <ErrorMessage error={remove.error} />
            <SheetActions>
              <Button variant="danger" block disabled={remove.isPending} onClick={() => remove.mutate(selected.id)}>
                Delete
              </Button>
              <Button variant="plain" block onClick={closeSheet}>
                Cancel
              </Button>
            </SheetActions>
          </>
        )}
      </Sheet>

      <SettleUpSheet
        open={settling}
        onClose={() => setSettling(false)}
        members={members}
        fromUserId={myDebt?.from.id ?? me.id}
        toUserId={myDebt?.to.id}
        amount={myDebt?.amount}
      />
    </>
  )
}
