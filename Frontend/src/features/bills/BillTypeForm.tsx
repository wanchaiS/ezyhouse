import { Controller, useForm } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import type { Frequency, UserRef } from '../../shared/api/types'
import { Avatar } from '../../shared/components/Avatar'
import { Button } from '../../shared/components/Button'
import { DateInput } from '../../shared/components/DateInput'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field, FieldRow, Input, Select } from '../../shared/components/Field'
import { SheetActions } from '../../shared/components/Sheet'
import { Note } from '../../shared/components/Text'
import { today } from '../../shared/format/date'
import styles from './Bills.module.css'
import { FREQUENCY_LABELS, type BillType, type BillTypeInput } from './types'

interface BillTypeFormProps {
  /** The bill type being edited; omitted for a new one. */
  billType?: BillType
  members: UserRef[]
  /** Rejects with the API error, which the form shows. */
  onSubmit: (input: BillTypeInput) => Promise<unknown>
  onCancel: () => void
}

interface BillTypeValues {
  name: string
  frequency: Frequency
  startDate: string
  endDate: string
  memberIds: number[]
}

const FIELDS = ['name', 'frequency', 'startDate', 'endDate', 'memberIds'] as const

export function BillTypeForm({ billType, members, onSubmit, onCancel }: BillTypeFormProps) {
  const { register, control, handleSubmit, setError, formState } = useForm<BillTypeValues>({
    defaultValues: {
      name: billType?.name ?? '',
      frequency: billType?.frequency ?? 'monthly',
      startDate: billType?.startDate ?? today(),
      endDate: billType?.endDate ?? '',
      memberIds: (billType?.members ?? members).map((member) => member.id),
    },
  })
  const { errors, isSubmitting } = formState

  const submit = handleSubmit(async ({ endDate, ...values }) => {
    try {
      await onSubmit({ ...values, endDate: endDate || null })
    } catch (error) {
      applyFieldErrors(error, setError, FIELDS)
    }
  })

  return (
    <form onSubmit={submit} noValidate>
      <Field label="Name" error={errors.name?.message}>
        <Input placeholder="e.g. Water" aria-invalid={!!errors.name} {...register('name', { required: 'Enter a name' })} />
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
        <Field label="First due" error={errors.startDate?.message}>
          <Controller
            name="startDate"
            control={control}
            rules={{ required: 'Choose the first due date' }}
            render={({ field }) => <DateInput {...field} aria-invalid={!!errors.startDate} />}
          />
        </Field>
      </FieldRow>
      <Field label="Ends (optional)" error={errors.endDate?.message}>
        <Controller
          name="endDate"
          control={control}
          render={({ field }) => <DateInput {...field} aria-invalid={!!errors.endDate} />}
        />
      </Field>
      <Controller
        name="memberIds"
        control={control}
        render={({ field }) => (
          <div className={styles.members}>
            <span className={styles.label}>Shared by</span>
            <div className={styles.people}>
              {members.map((member) => (
                <div className={styles.person} key={member.id}>
                  <label>
                    <input
                      type="checkbox"
                      checked={field.value.includes(member.id)}
                      onChange={(event) =>
                        field.onChange(
                          event.target.checked
                            ? members.filter((m) => m.id === member.id || field.value.includes(m.id)).map((m) => m.id)
                            : field.value.filter((id) => id !== member.id),
                        )
                      }
                    />
                    <Avatar user={member} small />
                    <span>{member.name}</span>
                  </label>
                </div>
              ))}
            </div>
            {errors.memberIds?.message && <span className={styles.error}>{errors.memberIds.message}</span>}
          </div>
        )}
      />
      <Note>Each time it's due, a bill appears with no amount. You fill in the amount when the bill arrives.</Note>
      <ErrorMessage message={errors.root?.message} />
      <SheetActions>
        <Button type="submit" variant="primary" block disabled={isSubmitting}>
          {billType ? 'Save' : 'Add bill type'}
        </Button>
        <Button variant="plain" block onClick={onCancel}>
          Cancel
        </Button>
      </SheetActions>
    </form>
  )
}
