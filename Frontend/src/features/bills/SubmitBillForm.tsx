import { Controller, useForm, useWatch } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import type { UserRef } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field } from '../../shared/components/Field'
import { MoneyInput } from '../../shared/components/MoneyInput'
import { SheetActions } from '../../shared/components/Sheet'
import { SplitEditor, type SplitValue } from '../../shared/components/SplitEditor'
import { Note } from '../../shared/components/Text'
import { formatDate } from '../../shared/format/date'
import { parseMoney } from '../../shared/format/money'
import type { SubmitBillInput } from './types'

interface SubmitBillFormProps {
  dueDate: string
  /** The bill type's members: an equal split always goes across exactly these people. */
  typeMembers: UserRef[]
  /** Everyone in the household, for an exact split. */
  members: UserRef[]
  /** Rejects with the API error, which the form shows. */
  onSubmit: (input: SubmitBillInput) => Promise<unknown>
  onCancel: () => void
}

interface SubmitBillValues {
  amount: string
  split: SplitValue
}

const FIELDS = ['amount'] as const

export function SubmitBillForm({ dueDate, typeMembers, members, onSubmit, onCancel }: SubmitBillFormProps) {
  const everyType = typeMembers.map((member) => ({ userId: member.id }))
  const { control, handleSubmit, setError, formState } = useForm<SubmitBillValues>({
    defaultValues: { amount: '', split: { splitType: 'equal', shares: everyType } },
  })
  const { errors, isSubmitting } = formState
  const amount = parseMoney(useWatch({ control, name: 'amount' }))

  const submit = handleSubmit(async ({ amount, split }) => {
    const input: SubmitBillInput =
      split.splitType === 'equal'
        ? { amount: parseMoney(amount), splitType: 'equal' }
        : { amount: parseMoney(amount), splitType: 'exact', shares: split.shares }
    try {
      await onSubmit(input)
    } catch (error) {
      applyFieldErrors(error, setError, FIELDS)
    }
  })

  return (
    <form onSubmit={submit} noValidate>
      <Note>Due {formatDate(dueDate)}</Note>
      <Field label="Amount on the bill" error={errors.amount?.message}>
        <Controller
          name="amount"
          control={control}
          rules={{ required: 'Enter the amount on the bill' }}
          render={({ field }) => <MoneyInput {...field} aria-invalid={!!errors.amount} />}
        />
      </Field>
      <Controller
        name="split"
        control={control}
        render={({ field }) => {
          const equal = field.value.splitType === 'equal'
          return (
            <>
              <SplitEditor
                members={equal ? typeMembers : members}
                amount={Number.isNaN(amount) ? undefined : amount}
                value={field.value}
                // The server ignores who is ticked for an equal split, so keep the bill type's people.
                onChange={(next) =>
                  field.onChange(next.splitType === 'equal' ? { splitType: 'equal', shares: everyType } : next)
                }
              />
              {equal && <Note>Equally between this bill's usual people.</Note>}
            </>
          )
        }}
      />
      <Note>Submit once you've paid the provider. Everyone then owes you their share.</Note>
      <ErrorMessage message={errors.root?.message} />
      <SheetActions>
        <Button type="submit" variant="primary" block disabled={isSubmitting}>
          Submit bill
        </Button>
        <Button variant="plain" block onClick={onCancel}>
          Cancel
        </Button>
      </SheetActions>
    </form>
  )
}
