import { PageHeader } from '../../shared/components/PageHeader'

interface LoginPageProps {
  mode: 'login' | 'register'
}

// Placeholder: the auth slice replaces this page.
export function LoginPage({ mode }: LoginPageProps) {
  return (
    <>
      <PageHeader title={mode === 'login' ? 'Log in' : 'Create account'} />
      <p>{mode === 'login' ? 'Email and password.' : 'Name, email and password.'}</p>
    </>
  )
}
