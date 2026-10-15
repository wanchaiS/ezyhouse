import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { keys } from '../../shared/api/keys'
import { applyFieldErrors } from '../../shared/api/problem'
import { Button } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field } from '../../shared/components/Field'
import { MoneyInput } from '../../shared/components/MoneyInput'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Note } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import { parseMoney } from '../../shared/format/money'
import { createSettlement } from './api'
import styles from './SettleUpSheet.module.css'
import type { Debt } from './types'

interface SettleUpSheetProps {
  /** The balance being paid down; null keeps the sheet closed. */
  debt: Debt | null
  /** Who paid whom, from the viewer's side, e.g. "You paid Ink back". */
  summary: string
  onClose: () => void
}

interface SettleUpForm {
  amount: string
}

const FIELDS = ['amount'] as const

/** Records money that has changed hands between two people (POST /settlements). */
export function SettleUpSheet({ debt, summary, onClose }: SettleUpSheetProps) {
  return (
    <Sheet open={debt !== null} onClose={onClose} title="Settle up">
      {debt && <SettleUpFormBody debt={debt} summary={summary} onClose={onClose} />}
    </Sheet>
  )
}

function SettleUpFormBody({ debt, summary, onClose }: { debt: Debt; summary: string; onClose: () => void }) {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const { control, handleSubmit, setError, formState } = useForm<SettleUpForm>({
    // The whole balance to start with; part of it is fine too.
    defaultValues: { amount: debt.amount.toFixed(2) },
  })
  const { errors } = formState

  const settle = useMutation({
    mutationFn: createSettlement,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: keys.balances })
      queryClient.invalidateQueries({ queryKey: keys.settlements })
      queryClient.invalidateQueries({ queryKey: keys.expenses })
      showToast('Payment recorded')
      onClose()
    },
    onError: (error) => applyFieldErrors(error, setError, FIELDS),
  })

  const onSubmit = handleSubmit((form) =>
    settle.mutate({ fromUserId: debt.from.id, toUserId: debt.to.id, amount: parseMoney(form.amount) }),
  )

  return (
    <form onSubmit={onSubmit} noValidate>
      <p className={styles.summary}>{summary}</p>
      <Field label="Amount" error={errors.amount?.message}>
        <Controller
          name="amount"
          control={control}
          rules={{ required: 'Enter an amount' }}
          render={({ field }) => <MoneyInput {...field} aria-invalid={!!errors.amount} />}
        />
      </Field>
      <Note>
        Record money that has already changed hands. It can't be undone. If you make a mistake, record a payment the
        other way.
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
