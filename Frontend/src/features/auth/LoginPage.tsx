import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router'
import { keys } from '../../shared/api/keys'
import { HomeIcon } from '../../shared/components/icons'
import { login, register } from './api'
import { LoginForm } from './LoginForm'
import { RegisterForm } from './RegisterForm'
import styles from './LoginPage.module.css'

interface LoginPageProps {
  mode: 'login' | 'register'
}

/** /login and /register. Once signed in, the route guards move on to /setup or /. */
export function LoginPage({ mode }: LoginPageProps) {
  const queryClient = useQueryClient()

  const onSignedIn = async () => {
    // A session that expired leaves the previous user's answers cached; start clean.
    queryClient.removeQueries({ predicate: (query) => query.queryKey[0] !== keys.me[0] })
    await queryClient.invalidateQueries({ queryKey: keys.me })
  }
  const logIn = useMutation({ mutationFn: login, onSuccess: onSignedIn })
  const signUp = useMutation({ mutationFn: register, onSuccess: onSignedIn })

  return (
    <main className={styles.auth}>
      <div className={styles.brandMark}>
        <HomeIcon />
      </div>
      <h1>EzyHouse</h1>
      <p className={styles.muted}>Split groceries, rent and bills with your housemates.</p>
      {mode === 'login' ? (
        <>
          <LoginForm onSubmit={logIn.mutateAsync} />
          <p className={styles.switch}>
            New here?{' '}
            <Link to="/register" className={styles.link}>
              Create an account
            </Link>
          </p>
        </>
      ) : (
        <>
          <RegisterForm onSubmit={signUp.mutateAsync} />
          <p className={styles.switch}>
            Have an account?{' '}
            <Link to="/login" className={styles.link}>
              Log in
            </Link>
          </p>
        </>
      )}
    </main>
  )
}
