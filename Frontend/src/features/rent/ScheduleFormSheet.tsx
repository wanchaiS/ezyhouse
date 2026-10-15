import { Controller, useForm, useWatch } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import type { Frequency, RentSchedule, UserRef } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { DateInput } from '../../shared/components/DateInput'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field, FieldRow, Select } from '../../shared/components/Field'
import { MoneyInput } from '../../shared/components/MoneyInput'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { SplitEditor, type SplitValue } from '../../shared/components/SplitEditor'
import { Note } from '../../shared/components/Text'
import { today } from '../../shared/format/date'
import { parseMoney } from '../../shared/format/money'
import { FREQUENCY_LABELS } from './format'
import styles from './Rent.module.css'
import type { RentScheduleInput } from './types'

interface ScheduleFormSheetProps {
  open: boolean
  onClose: () => void
  /** The schedule being edited, or null to set up a new one. */
  schedule: RentSchedule | null
  members: UserRef[]
  /** Saves the input; a rejection's errors are shown on the form. */
  onSave: (input: RentScheduleInput) => Promise<unknown>
}

interface ScheduleForm {
  amount: string
  frequency: Frequency
  startDate: string
  endDate: string
  split: SplitValue
}

// "splitType" and "shares" errors have no field of their own, so they show above the buttons.
const FIELDS = ['amount', 'frequency', 'startDate', 'endDate'] as const

export function ScheduleFormSheet({ open, onClose, schedule, ...formProps }: ScheduleFormSheetProps) {
  return (
    <Sheet open={open} onClose={onClose} title={schedule ? 'Edit rent' : 'Set up rent'}>
      <ScheduleFormBody schedule={schedule} onClose={onClose} {...formProps} />
    </Sheet>
  )
}

function defaultsFor(schedule: RentSchedule | null, members: UserRef[]): ScheduleForm {
  if (!schedule) {
    return {
      amount: '',
      frequency: 'weekly',
      startDate: today(),
      endDate: '',
      split: { splitType: 'equal', shares: members.map((member) => ({ userId: member.id })) },
    }
  }
  return {
    amount: schedule.amount.toFixed(2),
    frequency: schedule.frequency,
    startDate: schedule.startDate,
    endDate: schedule.endDate ?? '',
    split: {
      splitType: schedule.splitType,
      shares: schedule.shares.map((share) =>
        schedule.splitType === 'exact' ? { userId: share.user.id, amount: share.amount } : { userId: share.user.id },
      ),
    },
  }
}

function ScheduleFormBody({ schedule, members, onClose, onSave }: Omit<ScheduleFormSheetProps, 'open'>) {
  const { control, register, handleSubmit, setError, formState } = useForm<ScheduleForm>({
    defaultValues: defaultsFor(schedule, members),
  })
  const { errors, isSubmitting } = formState
  const amount = parseMoney(useWatch({ control, name: 'amount' }))

  const onSubmit = handleSubmit((form) =>
    onSave({
      amount: parseMoney(form.amount),
      frequency: form.frequency,
      startDate: form.startDate,
      endDate: form.endDate || null,
      splitType: form.split.splitType,
      shares: form.split.shares,
    }).catch((error: unknown) => applyFieldErrors(error, setError, FIELDS)),
  )

  return (
    <form onSubmit={onSubmit} noValidate>
      <Field label="Rent each period" error={errors.amount?.message}>
        <Controller
          name="amount"
          control={control}
          rules={{ validate: (value) => !Number.isNaN(parseMoney(value)) || 'Enter an amount' }}
          render={({ field }) => <MoneyInput {...field} aria-invalid={!!errors.amount} />}
        />
      </Field>
      <FieldRow>
        <Field label="How often" error={errors.frequency?.message}>
          <Select {...register('frequency')}>
            {Object.entries(FREQUENCY_LABELS).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </Select>
        </Field>
        <Field label="Starts" error={errors.startDate?.message}>
          <Controller
            name="startDate"
            control={control}
            rules={{ required: 'Choose a start date' }}
            render={({ field }) => <DateInput {...field} />}
          />
        </Field>
      </FieldRow>
      <Field label="Ends (optional)" error={errors.endDate?.message}>
        <Controller name="endDate" control={control} render={({ field }) => <DateInput {...field} />} />
      </Field>
      <Controller
        name="split"
        control={control}
        rules={{ validate: (split) => split.shares.length > 0 || 'Pick at least one person to share the rent.' }}
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
      {schedule && (
        <Note>Changes apply to rent created from now on. Rent already created keeps its amount and split.</Note>
      )}
      <ErrorMessage message={errors.root?.message} />
      <SheetActions>
        <Button type="submit" variant="primary" block disabled={isSubmitting}>
          {schedule ? 'Save' : 'Set up rent'}
        </Button>
        <Button variant="plain" block onClick={onClose}>
          Cancel
        </Button>
      </SheetActions>
    </form>
  )
}
