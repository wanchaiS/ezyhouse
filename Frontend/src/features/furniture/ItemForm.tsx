import { Controller, useForm, useWatch, type UseFormSetError } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import type { Furniture, UserRef } from '../../shared/api/types'
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
import styles from './Furniture.module.css'
import type { FurnitureInput } from './types'

interface ItemFormProps {
  members: UserRef[]
  meId: number
  /** The item being edited; omit to add one. */
  item?: Furniture
  /** Rejects with the API error, which the form shows. */
  onSave: (input: FurnitureInput) => Promise<unknown>
  onCancel: () => void
}

interface ItemFormValues {
  name: string
  amount: string
  date: string
  paidByUserId: number | null
  split: SplitValue
}

// The server's keys are "Name" and "Purchase.Amount" etc.; its split errors belong to the split editor.
const SERVER_FIELDS = [
  'name',
  'purchase.date',
  'purchase.amount',
  'purchase.paidByUserId',
  'purchase.splitType',
  'purchase.shares',
] as const

export function ItemForm({ members, meId, item, onSave, onCancel }: ItemFormProps) {
  const purchase = item?.purchase
  const rank = new Map(members.map((member, index) => [member.id, index]))
  const { control, register, handleSubmit, setError, formState } = useForm<ItemFormValues>({
    defaultValues: purchase
      ? {
          name: item.name,
          amount: purchase.amount.toFixed(2),
          date: purchase.date,
          paidByUserId: purchase.paidBy.id,
          split: {
            splitType: purchase.splitType,
            // The API lists shares in no set order; the split editor expects members order.
            shares: [...purchase.shares]
              .sort((a, b) => (rank.get(a.user.id) ?? members.length) - (rank.get(b.user.id) ?? members.length))
              .map((share) => ({
                userId: share.user.id,
                amount: purchase.splitType === 'exact' ? share.amount : undefined,
              })),
          },
        }
      : {
          name: '',
          amount: '',
          date: today(),
          paidByUserId: meId,
          split: { splitType: 'equal', shares: members.map((member) => ({ userId: member.id })) },
        },
  })
  const { errors, isSubmitting } = formState
  const amount = parseMoney(useWatch({ control, name: 'amount' }))

  // applyFieldErrors only passes names from SERVER_FIELDS, so the casts below cover every case.
  const setServerError: UseFormSetError<FurnitureInput> = (name, error, options) => {
    const field = name.replace(/^purchase\./, '')
    setError(
      field === 'splitType' || field === 'shares' ? 'split' : (field as 'name' | 'date' | 'amount' | 'paidByUserId'),
      error,
      options,
    )
  }

  const onSubmit = handleSubmit(async (form) => {
    try {
      await onSave({
        name: form.name.trim(),
        purchase: {
          date: form.date,
          amount: parseMoney(form.amount),
          paidByUserId: form.paidByUserId!,
          splitType: form.split.splitType,
          shares: form.split.shares,
        },
      })
    } catch (error) {
      applyFieldErrors(error, setServerError, SERVER_FIELDS)
    }
  })

  return (
    <form onSubmit={onSubmit} noValidate>
      <Field label="Item" error={errors.name?.message}>
        <Input
          {...register('name', { required: 'Name the item' })}
          placeholder="e.g. Couch"
          aria-invalid={!!errors.name}
        />
      </Field>
      <Field label="Price" error={errors.amount?.message}>
        <Controller
          name="amount"
          control={control}
          rules={{ required: 'Enter the price' }}
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
        <Field label="Bought on" error={errors.date?.message}>
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
      <ErrorMessage message={errors.root?.message} />
      <SheetActions>
        <Button type="submit" variant="primary" block disabled={isSubmitting}>
          {item ? 'Save' : 'Add item'}
        </Button>
        <Button variant="plain" block onClick={onCancel}>
          Cancel
        </Button>
      </SheetActions>
    </form>
  )
}
