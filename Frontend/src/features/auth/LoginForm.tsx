import { useForm } from 'react-hook-form'
import { applyFieldErrors } from '../../shared/api/problem'
import { Button } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field, Input } from '../../shared/components/Field'
import type { LoginInput } from './types'

const FIELDS = ['email', 'password'] as const

interface LoginFormProps {
  /** Resolves once signed in; a rejection's ProblemDetails is shown on the form. */
  onSubmit: (input: LoginInput) => Promise<unknown>
}

export function LoginForm({ onSubmit }: LoginFormProps) {
  const { register, handleSubmit, setError, formState } = useForm<LoginInput>({
    defaultValues: { email: '', password: '' },
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
          autoComplete="current-password"
          aria-invalid={!!errors.password}
          {...register('password', { required: 'Enter your password' })}
        />
      </Field>
      <Button type="submit" variant="primary" block disabled={isSubmitting}>
        Log in
      </Button>
    </form>
  )
}
