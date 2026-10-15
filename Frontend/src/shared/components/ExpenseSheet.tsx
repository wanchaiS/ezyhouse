import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createExpense, deleteExpense, updateExpense } from '../api/expenses'
import { keys } from '../api/keys'
import type { Expense, ExpenseInput, ExpenseTag, UserRef } from '../api/types'
import { formatShortDate } from '../format/date'
import { expenseTitle } from '../format/expense'
import { Button } from './Button'
import { ErrorMessage } from './ErrorMessage'
import { ExpenseDetail } from './ExpenseDetail'
import { ExpenseForm } from './ExpenseForm'
import { Sheet, SheetActions } from './Sheet'
import { Note } from './Text'
import { useToast } from './toastContext'

/** Which sheet is open: adding one, or looking at, editing or deleting a given expense. */
export type ExpenseSheetState = { kind: 'add' } | { kind: 'view' | 'edit' | 'delete'; expense: Expense } | null

interface ExpenseSheetProps {
  state: ExpenseSheetState
  onChange: (state: ExpenseSheetState) => void
  /** The tag a new expense gets. */
  tag: ExpenseTag
  members: UserRef[]
  meId: number
  primaryName: string
}

const ADD_TITLES: Record<ExpenseTag, string> = {
  groceries: 'Add expense',
  furniture: 'Add furniture',
  rent: 'Add rent',
  bills: 'Add bill',
}

/**
 * Adding, viewing, editing and deleting one expense, for every tab. Each write refreshes every
 * expense list and the balances, since any expense counts in the balances whatever its tag.
 */
export function ExpenseSheet({ state, onChange, tag, members, meId, primaryName }: ExpenseSheetProps) {
  const queryClient = useQueryClient()
  const showToast = useToast()

  function finish(message: string) {
    queryClient.invalidateQueries({ queryKey: keys.expenses })
    queryClient.invalidateQueries({ queryKey: keys.balances })
    // Close the sheet first: a toast under an open dialog is hidden.
    onChange(null)
    showToast(message)
  }

  const save = useMutation({
    mutationFn: ({ id, input }: { id?: number; input: ExpenseInput }) =>
      id === undefined ? createExpense({ tag, ...input }) : updateExpense(id, input),
    onSuccess: (_, { id }) => finish(id === undefined ? 'Expense added' : 'Expense saved'),
  })
  const remove = useMutation({
    mutationFn: deleteExpense,
    onSuccess: () => finish('Expense deleted'),
  })

  function close() {
    onChange(null)
    remove.reset()
  }

  const expense = state && state.kind !== 'add' ? state.expense : undefined
  // A bill still at $0 is opened to enter its amount, so the title says which bill it is.
  const entering = expense && expense.tag === 'bills' && expense.amount === 0
  const title =
    state?.kind === 'add'
      ? ADD_TITLES[tag]
      : state?.kind === 'edit'
        ? entering
          ? `${expense!.description} · due ${formatShortDate(expense!.date)}`
          : 'Edit expense'
        : state?.kind === 'delete'
          ? 'Delete this expense?'
          : expense && expenseTitle(expense)

  return (
    <Sheet open={state !== null} onClose={close} title={title}>
      {state?.kind === 'add' && (
        <ExpenseForm
          tag={tag}
          members={members}
          meId={meId}
          onSave={(input) => save.mutateAsync({ input })}
          onCancel={close}
        />
      )}
      {state?.kind === 'view' && (
        <ExpenseDetail
          expense={state.expense}
          primaryName={primaryName}
          onEdit={() => onChange({ kind: 'edit', expense: state.expense })}
          onDelete={() => {
            remove.reset()
            onChange({ kind: 'delete', expense: state.expense })
          }}
        />
      )}
      {state?.kind === 'edit' && (
        <ExpenseForm
          tag={state.expense.tag}
          members={members}
          meId={meId}
          expense={state.expense}
          onSave={(input) => save.mutateAsync({ id: state.expense.id, input })}
          onCancel={close}
        />
      )}
      {state?.kind === 'delete' && (
        <>
          <Note>
            {expenseTitle(state.expense)} is removed for everyone, and the balances change straight away.
            {(state.expense.rentScheduleId !== null || state.expense.billTypeId !== null) &&
              ' It won’t be created again.'}
          </Note>
          <ErrorMessage error={remove.error} />
          <SheetActions>
            <Button variant="danger" block disabled={remove.isPending} onClick={() => remove.mutate(state.expense.id)}>
              Delete
            </Button>
            <Button variant="plain" block onClick={close}>
              Cancel
            </Button>
          </SheetActions>
        </>
      )}
    </Sheet>
  )
}
