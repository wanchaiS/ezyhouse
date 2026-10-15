import { Controller, useForm, useWatch, type UseFormSetError } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import type { UserRef } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { DateInput } from '../../shared/components/DateInput'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field, FieldRow, Input } from '../../shared/components/Field'
import { MemberSelect } from '../../shared/components/MemberSelect'
import { MoneyInput } from '../../shared/components/MoneyInput'
import { SheetActions } from '../../shared/components/Sheet'
import { SplitEditor, type SplitValue } from '../../shared/components/SplitEditor'
import { today } from '../../shared/format/date'
import { parseMoney } from '../../shared/format/money'
import styles from './Groceries.module.css'
import type { Expense, ExpenseInput } from './types'

interface ExpenseFormProps {
  members: UserRef[]
  meId: number
  /** The expense being edited; omit to add one. */
  expense?: Expense
  /** Rejects with the API error, which the form shows. */
  onSave: (input: ExpenseInput) => Promise<unknown>
  onCancel: () => void
}

interface ExpenseFormValues {
  description: string
  amount: string
  date: string
  paidByUserId: number | null
  split: SplitValue
}

// The server's field names: its "SplitType" and "Shares" errors belong to the split editor.
type ServerFields = Omit<ExpenseFormValues, 'split'> & Pick<ExpenseInput, 'splitType' | 'shares'>
const SERVER_FIELDS = ['description', 'amount', 'date', 'paidByUserId', 'splitType', 'shares'] as const

export function ExpenseForm({ members, meId, expense, onSave, onCancel }: ExpenseFormProps) {
  const { control, register, handleSubmit, setError, formState } = useForm<ExpenseFormValues>({
    defaultValues: expense
      ? {
          description: expense.description,
          amount: expense.amount.toFixed(2),
          date: expense.date,
          paidByUserId: expense.paidBy.id,
          split: {
            splitType: expense.splitType,
            shares: expense.shares.map((share) => ({
              userId: share.user.id,
              amount: expense.splitType === 'exact' ? share.amount : undefined,
            })),
          },
        }
      : {
          description: '',
          amount: '',
          date: today(),
          paidByUserId: meId,
          split: { splitType: 'equal', shares: members.map((member) => ({ userId: member.id })) },
        },
  })
  const { errors, isSubmitting } = formState
  const amount = parseMoney(useWatch({ control, name: 'amount' }))

  // applyFieldErrors only passes names from SERVER_FIELDS, so no nested "shares.N" paths arrive.
  const setServerError: UseFormSetError<ServerFields> = (name, error, options) =>
    name === 'splitType' || name === 'shares'
      ? setError('split', error, options)
      : setError(name as Exclude<typeof name, `shares.${string}`>, error, options)

  const onSubmit = handleSubmit(async (form) => {
    try {
      await onSave({
        description: form.description.trim(),
        amount: parseMoney(form.amount),
        date: form.date,
        paidByUserId: form.paidByUserId!,
        splitType: form.split.splitType,
        shares: form.split.shares,
      })
    } catch (error) {
      applyFieldErrors(error, setServerError, SERVER_FIELDS)
    }
  })

  return (
    <form onSubmit={onSubmit} noValidate>
      <Field label="What was it?" error={errors.description?.message}>
        <Input
          {...register('description', { required: 'Say what it was' })}
          placeholder="e.g. Woolworths"
          aria-invalid={!!errors.description}
        />
      </Field>
      <Field label="Amount" error={errors.amount?.message}>
        <Controller
          name="amount"
          control={control}
          rules={{ required: 'Enter an amount' }}
          render={({ field }) => <MoneyInput {...field} aria-invalid={!!errors.amount} />}
        />
      </Field>
      <FieldRow>
        <Field label="Paid by" error={errors.paidByUserId?.message}>
          <Controller
            name="paidByUserId"
            control={control}
            rules={{ required: 'Choose who paid' }}
            render={({ field }) => <MemberSelect {...field} members={members} />}
          />
        </Field>
        <Field label="Date" error={errors.date?.message}>
          <Controller
            name="date"
            control={control}
            rules={{ required: 'Choose a date' }}
            render={({ field }) => <DateInput {...field} aria-invalid={!!errors.date} />}
          />
        </Field>
      </FieldRow>
      <Controller
        name="split"
        control={control}
        render={({ field }) => (
          <SplitEditor
            members={members}
            amount={Number.isNaN(amount) ? undefined : amount}
            value={field.value}
            onChange={field.onChange}
          />
        )}
      />
      {errors.split?.message && <p className={styles.fieldError}>{errors.split.message}</p>}
      {/* Below the split, since most rule errors without a field (e.g. shares not adding up) are about it. */}
      <ErrorMessage message={errors.root?.message} />
      <SheetActions>
        <Button type="submit" variant="primary" block disabled={isSubmitting}>
          {expense ? 'Save' : 'Add expense'}
        </Button>
        <Button variant="plain" block onClick={onCancel}>
          Cancel
        </Button>
      </SheetActions>
    </form>
  )
}
