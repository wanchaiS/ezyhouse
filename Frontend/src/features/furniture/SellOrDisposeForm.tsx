import { Controller, useForm, useWatch, type UseFormSetError } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import type { Furniture, UserRef } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { DateInput } from '../../shared/components/DateInput'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field, FieldRow } from '../../shared/components/Field'
import { MemberSelect } from '../../shared/components/MemberSelect'
import { MoneyInput } from '../../shared/components/MoneyInput'
import { SheetActions } from '../../shared/components/Sheet'
import { SplitEditor, type SplitValue } from '../../shared/components/SplitEditor'
import { Note } from '../../shared/components/Text'
import { today } from '../../shared/format/date'
import { parseMoney } from '../../shared/format/money'
import styles from './Furniture.module.css'
import type { DisposalInput, SaleInput } from './types'

// onSave rejects with the API error, which the form shows.
type SellOrDisposeFormProps = {
  item: Furniture
  members: UserRef[]
  meId: number
  onCancel: () => void
} & (
  | { kind: 'sell'; onSave: (input: SaleInput) => Promise<unknown> }
  | { kind: 'dispose'; onSave: (input: DisposalInput) => Promise<unknown> }
)

interface Values {
  amount: string
  date: string
  userId: number | null
  split: SplitValue
}

const TEXT = {
  sell: {
    amount: 'Sold for',
    amountRequired: 'Enter what it sold for',
    person: 'Money went to',
    date: 'Sold on',
    submit: 'Record sale',
    note: "Whoever got the money owes everyone else their share. Once sold, the item can't be changed.",
  },
  dispose: {
    amount: 'Cost to get rid of it',
    amountRequired: 'Enter the cost, or 0.00',
    person: 'Paid by',
    date: 'Date',
    submit: 'Record disposal',
    note: "Leave it at $0.00 if it cost nothing. Once disposed, the item can't be changed.",
  },
}

type ServerFields = SaleInput & Pick<DisposalInput, 'paidByUserId'>

/** Records a sale or a disposal; the two differ only in wording and who the money goes to or from. */
export function SellOrDisposeForm(props: SellOrDisposeFormProps) {
  const { kind, item, members, meId, onCancel } = props
  const text = TEXT[kind]
  const personField = kind === 'sell' ? 'receivedByUserId' : 'paidByUserId'

  // Start with the people who shared the purchase and still live here, in members order as the
  // split editor expects; everyone if none of them are left.
  const buyerIds = new Set(item.purchase.shares.map((share) => share.user.id))
  const buyers = members.filter((member) => buyerIds.has(member.id))
  const shares = (buyers.length ? buyers : members).map((member) => ({ userId: member.id }))

  const { control, handleSubmit, setError, formState } = useForm<Values>({
    defaultValues: {
      amount: kind === 'dispose' ? '0.00' : '',
      date: today(),
      userId: meId,
      split: { splitType: 'equal', shares },
    },
  })
  const { errors, isSubmitting } = formState
  const amount = parseMoney(useWatch({ control, name: 'amount' }))

  // applyFieldErrors only passes the names listed below, so no nested "shares.N" paths arrive.
  const setServerError: UseFormSetError<ServerFields> = (name, error, options) => {
    if (name === 'splitType' || name === 'shares') setError('split', error, options)
    else if (name === 'receivedByUserId' || name === 'paidByUserId') setError('userId', error, options)
    else setError(name as 'amount' | 'date', error, options)
  }

  const onSubmit = handleSubmit(async (form) => {
    const common = {
      date: form.date,
      amount: parseMoney(form.amount),
      splitType: form.split.splitType,
      shares: form.split.shares,
    }
    try {
      if (props.kind === 'sell') await props.onSave({ ...common, receivedByUserId: form.userId! })
      else await props.onSave({ ...common, paidByUserId: form.userId! })
    } catch (error) {
      applyFieldErrors(error, setServerError, ['date', 'amount', personField, 'splitType', 'shares'])
    }
  })

  return (
    <form onSubmit={onSubmit} noValidate>
      <Field label={text.amount} error={errors.amount?.message}>
        <Controller
          name="amount"
          control={control}
          rules={{ required: text.amountRequired }}
          render={({ field }) => <MoneyInput {...field} aria-invalid={!!errors.amount} />}
        />
      </Field>
      <FieldRow>
        <Field label={text.person} error={errors.userId?.message}>
          <Controller
            name="userId"
            control={control}
            rules={{ required: 'Choose someone' }}
            render={({ field }) => <MemberSelect {...field} members={members} />}
          />
        </Field>
        <Field label={text.date} error={errors.date?.message}>
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
      <Note>{text.note}</Note>
      <ErrorMessage message={errors.root?.message} />
      <SheetActions>
        <Button type="submit" variant="primary" block disabled={isSubmitting}>
          {text.submit}
        </Button>
        <Button variant="plain" block onClick={onCancel}>
          Cancel
        </Button>
      </SheetActions>
    </form>
  )
}
