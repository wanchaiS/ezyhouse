import { CheckIcon } from './icons'
import styles from './Tick.module.css'

interface TickProps {
  paid: boolean
  /** Called with the new value when tapped. */
  onChange: (paid: boolean) => void
  disabled?: boolean
}

/** The round "paid" toggle on rent and bill shares. */
export function Tick({ paid, onChange, disabled }: TickProps) {
  return (
    <button
      type="button"
      className={paid ? `${styles.tick} ${styles.on}` : styles.tick}
      aria-pressed={paid}
      aria-label={paid ? 'Paid' : 'Mark as paid'}
      disabled={disabled}
      onClick={() => onChange(!paid)}
    >
      <CheckIcon />
    </button>
  )
}
