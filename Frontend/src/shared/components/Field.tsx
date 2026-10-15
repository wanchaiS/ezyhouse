import type { ComponentProps, ReactNode } from 'react'
import styles from './Field.module.css'

interface FieldProps {
  label: ReactNode
  /** The field's error message, e.g. errors.amount?.message. */
  error?: string
  children: ReactNode
}

/** A label above one input, with its error underneath. */
export function Field({ label, error, children }: FieldProps) {
  return (
    <label className={styles.field}>
      <span className={styles.label}>{label}</span>
      {children}
      {error && <span className={styles.error}>{error}</span>}
    </label>
  )
}

/** Two fields side by side, e.g. Paid by and Date. */
export function FieldRow({ children }: { children: ReactNode }) {
  return <div className={styles.two}>{children}</div>
}

type InputProps = ComponentProps<'input'> & { big?: boolean }

/** A text-style input; works with register() since ref is passed through. */
export function Input({ big, className, ...rest }: InputProps) {
  const classes = [styles.input, big && styles.big, className].filter(Boolean).join(' ')
  return <input className={classes} {...rest} />
}

export function Select({ className, ...rest }: ComponentProps<'select'>) {
  return <select className={[styles.input, className].filter(Boolean).join(' ')} {...rest} />
}
