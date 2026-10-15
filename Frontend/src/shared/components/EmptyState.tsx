import type { ReactNode } from 'react'
import styles from './EmptyState.module.css'

/** The white card with grey centred text shown when a list is empty. */
export function EmptyState({ children }: { children: ReactNode }) {
  return <div className={styles.empty}>{children}</div>
}
