import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { createSettlement } from '../api/balances'
import { keys } from '../api/keys'
import { applyFieldErrors } from '../api/problem'
import type { Settlement, UserRef } from '../api/types'
import { parseMoney } from '../format/money'
import { Button } from './Button'
import { ErrorMessage } from './ErrorMessage'
import { Field, FieldRow } from './Field'
import { MemberSelect } from './MemberSelect'
import { MoneyInput } from './MoneyInput'
import { Sheet, SheetActions } from './Sheet'
import { Note } from './Text'
import { useToast } from './toastContext'

interface SettleUpSheetProps {
  open: boolean
  onClose: () => void
  members: UserRef[]
  /** Starting values, e.g. from a debt row. */
  fromUserId?: number
  toUserId?: number
  amount?: number
  onSettled?: (settlement: Settlement) => void
}

interface SettleUpForm {
  fromUserId: number | null
  toUserId: number | null
  amount: string
}

const FIELDS = ['fromUserId', 'toUserId', 'amount'] as const

/** Records money that has changed hands (POST /settlements), then refreshes balances. */
export function SettleUpSheet({ open, onClose, ...formProps }: SettleUpSheetProps) {
  return (
    <Sheet open={open} onClose={onClose} title="Settle up">
      <SettleUpFormBody onClose={onClose} {...formProps} />
    </Sheet>
  )
}

function SettleUpFormBody({ onClose, members, fromUserId, toUserId, amount, onSettled }: Omit<SettleUpSheetProps, 'open'>) {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const { control, handleSubmit, setError, formState } = useForm<SettleUpForm>({
    defaultValues: {
      fromUserId: fromUserId ?? null,
      toUserId: toUserId ?? null,
      amount: amount === undefined ? '' : amount.toFixed(2),
    },
  })
  const { errors } = formState

  const settle = useMutation({
    mutationFn: createSettlement,
    onSuccess: (settlement) => {
      queryClient.invalidateQueries({ queryKey: keys.balances })
      queryClient.invalidateQueries({ queryKey: keys.settlements })
      showToast('Payment recorded')
      onSettled?.(settlement)
      onClose()
    },
    onError: (error) => applyFieldErrors(error, setError, FIELDS),
  })

  const onSubmit = handleSubmit((form) =>
    settle.mutate({
      fromUserId: form.fromUserId!,
      toUserId: form.toUserId!,
      amount: parseMoney(form.amount),
    }),
  )

  return (
    <form onSubmit={onSubmit} noValidate>
      <FieldRow>
        <Field label="From" error={errors.fromUserId?.message}>
          <Controller
            name="fromUserId"
            control={control}
            rules={{ required: 'Choose who paid' }}
            render={({ field }) => <MemberSelect {...field} members={members} />}
          />
        </Field>
        <Field label="To" error={errors.toUserId?.message}>
          <Controller
            name="toUserId"
            control={control}
            rules={{ required: 'Choose who was paid' }}
            render={({ field }) => <MemberSelect {...field} members={members} />}
          />
        </Field>
      </FieldRow>
      <Field label="Amount" error={errors.amount?.message}>
        <Controller
          name="amount"
          control={control}
          rules={{ required: 'Enter an amount' }}
          render={({ field }) => <MoneyInput {...field} aria-invalid={!!errors.amount} />}
        />
      </Field>
      <Note>
        Record money that has already changed hands. It can't be undone. If you make a mistake,
        record a payment the other way.
      </Note>
      <ErrorMessage message={errors.root?.message} />
      <SheetActions>
        <Button type="submit" variant="primary" block disabled={settle.isPending}>
          Record payment
        </Button>
        <Button variant="plain" block onClick={onClose}>
          Cancel
        </Button>
      </SheetActions>
    </form>
  )
}
