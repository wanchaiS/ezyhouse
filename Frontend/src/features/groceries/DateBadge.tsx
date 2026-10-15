import { formatShortDate } from '../../shared/format/date'
import styles from './DateBadge.module.css'

/** The day over the short month, at the start of a row. */
export function DateBadge({ date }: { date: string }) {
  const [day, month] = formatShortDate(date).split(' ')
  return (
    <span className={styles.badge}>
      <b>{day}</b>
      {month}
    </span>
  )
}
