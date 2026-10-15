import type { ComponentProps } from 'react'
import styles from './Button.module.css'

type Variant = 'default' | 'primary' | 'danger' | 'plain' | 'quietDanger'

type ButtonProps = ComponentProps<'button'> & {
  variant?: Variant
  block?: boolean
  small?: boolean
}

export function Button({ variant = 'default', block, small, className, type = 'button', ...rest }: ButtonProps) {
  const classes = [
    styles.btn,
    variant !== 'default' && styles[variant],
    block && styles.block,
    small && styles.small,
    className,
  ]
  return <button type={type} className={classes.filter(Boolean).join(' ')} {...rest} />
}

type IconButtonProps = ComponentProps<'button'> & {
  /** Read out by screen readers, since the button shows only an icon. */
  label: string
  accent?: boolean
}

export function IconButton({ label, accent, className, type = 'button', ...rest }: IconButtonProps) {
  const classes = [styles.iconBtn, accent && styles.accent, className].filter(Boolean).join(' ')
  return <button type={type} aria-label={label} className={classes} {...rest} />
}

/** A button that looks like a link (e.g. "Create an account"). */
export function TextButton({ className, type = 'button', ...rest }: ComponentProps<'button'>) {
  return <button type={type} className={[styles.link, className].filter(Boolean).join(' ')} {...rest} />
}
