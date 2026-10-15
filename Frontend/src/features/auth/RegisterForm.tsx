import { useForm } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import { Button } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field, Input } from '../../shared/components/Field'
import type { RegisterInput } from './types'

const FIELDS = ['name', 'email', 'password'] as const

interface RegisterFormProps {
  /** Resolves once the account exists and is signed in; a rejection is shown on the form. */
  onSubmit: (input: RegisterInput) => Promise<unknown>
}

export function RegisterForm({ onSubmit }: RegisterFormProps) {
  const { register, handleSubmit, setError, formState } = useForm<RegisterInput>({
    defaultValues: { name: '', email: '', password: '' },
  })
  const { errors, isSubmitting } = formState

  const submit = handleSubmit(async (input) => {
    try {
      await onSubmit(input)
    } catch (error) {
      applyFieldErrors(error, setError, FIELDS)
    }
  })

  return (
    <form onSubmit={submit} noValidate>
      <ErrorMessage message={errors.root?.message} />
      <Field label="Name" error={errors.name?.message}>
        <Input
          autoComplete="name"
          aria-invalid={!!errors.name}
          {...register('name', { required: 'Enter your name' })}
        />
      </Field>
      <Field label="Email" error={errors.email?.message}>
        <Input
          type="email"
          autoComplete="email"
          aria-invalid={!!errors.email}
          {...register('email', { required: 'Enter your email' })}
        />
      </Field>
      <Field label="Password" error={errors.password?.message}>
        <Input
          type="password"
          autoComplete="new-password"
          aria-invalid={!!errors.password}
          {...register('password', { required: 'Choose a password' })}
        />
      </Field>
      <Button type="submit" variant="primary" block disabled={isSubmitting}>
        Create account
      </Button>
    </form>
  )
}
