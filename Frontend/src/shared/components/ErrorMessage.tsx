import { problemMessage } from '../api/problem'
import styles from './ErrorMessage.module.css'

interface ErrorMessageProps {
  /** An error from an API call; its ProblemDetails message is shown. */
  error?: unknown
  /** A message to show as is, e.g. a form's errors.root?.message. */
  message?: string
}

/** Renders nothing when there is no error. */
export function ErrorMessage({ error, message }: ErrorMessageProps) {
  const text = message ?? (error ? problemMessage(error) : undefined)
  if (!text) return null
  return (
    <div className={styles.error} role="alert">
      {text}
    </div>
  )
}
