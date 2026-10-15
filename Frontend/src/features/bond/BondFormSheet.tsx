import { Controller, useFieldArray, useForm, useWatch, type Path } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import type { Bond, UserRef } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { DateInput } from '../../shared/components/DateInput'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field } from '../../shared/components/Field'
import { MemberSelect } from '../../shared/components/MemberSelect'
import { MoneyInput } from '../../shared/components/MoneyInput'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { today } from '../../shared/format/date'
import { formatMoney, parseMoney } from '../../shared/format/money'
import type { BondInput } from './types'
import styles from './Bond.module.css'

interface BondFormSheetProps {
  open: boolean
  onClose: () => void
  /** The bond being edited, or null to add a new one. */
  bond: Bond | null
  members: UserRef[]
  /** Saves the input; a rejection's errors are shown on the form. */
  onSave: (input: BondInput) => Promise<unknown>
  /** Only offered when editing. */
  onDelete: (bond: Bond) => void
}

interface ContributionRow {
  userId: number | null
  amount: string
  date: string
}

interface BondForm {
  total: string
  paidDate: string
  contributions: ContributionRow[]
}

const ROW_FIELDS = ['userId', 'amount', 'date'] as const

export function BondFormSheet({ open, onClose, bond, ...formProps }: BondFormSheetProps) {
  return (
    <Sheet open={open} onClose={onClose} title={bond ? 'Edit bond' : 'Add bond'}>
      <BondFormBody bond={bond} onClose={onClose} {...formProps} />
    </Sheet>
  )
}

function defaultsFor(bond: Bond | null): BondForm {
  if (!bond) return { total: '', paidDate: today(), contributions: [] }
  return {
    total: bond.total.toFixed(2),
    paidDate: bond.paidDate,
    contributions: bond.contributions.map((contribution) => ({
      userId: contribution.user.id,
      amount: contribution.amount.toFixed(2),
      date: contribution.date,
    })),
  }
}

function BondFormBody({ bond, members, onClose, onSave, onDelete }: Omit<BondFormSheetProps, 'open'>) {
  const { control, handleSubmit, setError, formState } = useForm<BondForm>({ defaultValues: defaultsFor(bond) })
  const { fields, append, remove } = useFieldArray({ control, name: 'contributions' })
  const { errors, isSubmitting } = formState

  // Someone who has since left the household can still appear on an old contribution.
  const people = [...members]
  for (const contribution of bond?.contributions ?? []) {
    if (!people.some((person) => person.id === contribution.user.id)) people.push(contribution.user)
  }

  const total = parseMoney(useWatch({ control, name: 'total' }))
  const rows = useWatch({ control, name: 'contributions' })
  const putIn = rows.reduce((sum, row) => {
    const amount = parseMoney(row.amount)
    return Number.isNaN(amount) ? sum : sum + amount
  }, 0)
  const toCollect = Number.isNaN(total) ? null : total - putIn

  const onSubmit = handleSubmit((form) => {
    const fieldNames: Path<BondForm>[] = ['total', 'paidDate']
    form.contributions.forEach((_, index) =>
      ROW_FIELDS.forEach((name) => fieldNames.push(`contributions.${index}.${name}`)),
    )
    return onSave({
      total: parseMoney(form.total),
      paidDate: form.paidDate,
      contributions: form.contributions.map((row) => ({
        userId: row.userId as number,
        amount: parseMoney(row.amount),
        date: row.date,
      })),
    }).catch((error: unknown) => applyFieldErrors(error, setError, fieldNames))
  })

  return (
    <form onSubmit={onSubmit} noValidate>
      <Field label="Bond total" error={errors.total?.message}>
        <Controller
          name="total"
          control={control}
          rules={{ validate: (value) => !Number.isNaN(parseMoney(value)) || 'Enter an amount' }}
          render={({ field }) => <MoneyInput {...field} aria-invalid={!!errors.total} />}
        />
      </Field>
      <Field label="Paid to the agent on" error={errors.paidDate?.message}>
        <Controller
          name="paidDate"
          control={control}
          rules={{ required: 'Choose a date' }}
          render={({ field }) => <DateInput {...field} aria-invalid={!!errors.paidDate} />}
        />
      </Field>

      <span className={styles.fieldLabel}>Who put in what</span>
      {fields.length > 0 && (
        <div className={styles.rows}>
          {fields.map((item, index) => {
            const rowErrors = errors.contributions?.[index]
            const messages = ROW_FIELDS.map((name) => rowErrors?.[name]?.message).filter(Boolean)
            return (
              <div key={item.id} className={styles.row}>
                <div className={styles.who}>
                  <Controller
                    name={`contributions.${index}.userId`}
                    control={control}
                    rules={{ validate: (value) => value !== null || 'Choose who put it in' }}
                    render={({ field }) => (
                      <MemberSelect
                        members={people}
                        value={field.value}
                        onChange={field.onChange}
                        onBlur={field.onBlur}
                        aria-label="Who"
                        aria-invalid={!!rowErrors?.userId}
                      />
                    )}
                  />
                  <Button variant="quietDanger" small onClick={() => remove(index)}>
                    Remove
                  </Button>
                </div>
                <Controller
                  name={`contributions.${index}.amount`}
                  control={control}
                  rules={{ validate: (value) => !Number.isNaN(parseMoney(value)) || 'Enter an amount' }}
                  render={({ field }) => (
                    <MoneyInput {...field} big={false} aria-label="Amount" aria-invalid={!!rowErrors?.amount} />
                  )}
                />
                <Controller
                  name={`contributions.${index}.date`}
                  control={control}
                  rules={{ required: 'Choose a date' }}
                  render={({ field }) => <DateInput {...field} aria-label="Date" aria-invalid={!!rowErrors?.date} />}
                />
                {messages.length > 0 && <span className={styles.rowError}>{messages.join(' ')}</span>}
              </div>
            )
          })}
        </div>
      )}
      <Button small onClick={() => append({ userId: null, amount: '', date: today() })}>
        Add contribution
      </Button>
      <div className={styles.total}>
        <span>Put in {formatMoney(putIn)}</span>
        {toCollect !== null && (
          <span className={toCollect < 0 ? styles.warn : undefined}>
            {toCollect < 0 ? `${formatMoney(-toCollect)} over the total` : `${formatMoney(toCollect)} still to collect`}
          </span>
        )}
      </div>

      <ErrorMessage message={errors.root?.message} />
      <SheetActions>
        <Button type="submit" variant="primary" block disabled={isSubmitting}>
          {bond ? 'Save' : 'Add bond'}
        </Button>
        {bond && (
          <Button variant="quietDanger" block onClick={() => onDelete(bond)}>
            Delete
          </Button>
        )}
        <Button variant="plain" block onClick={onClose}>
          Cancel
        </Button>
      </SheetActions>
    </form>
  )
}
