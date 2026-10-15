import { useForm } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import { Button } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field, Input } from '../../shared/components/Field'

interface HouseholdFormProps {
  /** The request body's field name, which is also the form's only field. */
  field: 'name' | 'inviteCode'
  label: string
  placeholder: string
  submitLabel: string
  primary?: boolean
  onSubmit: (value: string) => Promise<unknown>
}

type Values = Record<'name' | 'inviteCode', string>

export function HouseholdForm({ field, label, placeholder, submitLabel, primary, onSubmit }: HouseholdFormProps) {
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<Values>({ defaultValues: { name: '', inviteCode: '' } })

  const submit = handleSubmit(async (values) => {
    try {
      await onSubmit(values[field].trim())
    } catch (error) {
      applyFieldErrors(error, setError, [field])
    }
  })

  return (
    <form onSubmit={submit} noValidate>
      <Field label={label} error={errors[field]?.message}>
        <Input
          placeholder={placeholder}
          autoCapitalize={field === 'inviteCode' ? 'characters' : undefined}
          {...register(field, { required: `${label} is required.` })}
        />
      </Field>
      <Button type="submit" variant={primary ? 'primary' : 'default'} block disabled={isSubmitting}>
        {submitLabel}
      </Button>
      <ErrorMessage message={errors.root?.message} />
    </form>
  )
}
