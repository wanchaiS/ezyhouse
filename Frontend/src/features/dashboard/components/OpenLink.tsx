import { Link } from 'react-router'
import buttonStyles from '../../../shared/components/Button.module.css'
import styles from '../Dashboard.module.css'

/** A link to a screen that looks like the card's other buttons. */
export function OpenLink({ to, children }: { to: string; children: string }) {
  return (
    <Link className={`${buttonStyles.btn} ${styles.linkBtn}`} to={to}>
      {children}
    </Link>
  )
}
