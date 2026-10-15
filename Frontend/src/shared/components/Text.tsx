import type { ReactNode } from 'react'
import styles from './Text.module.css'

/** The small grey heading above a list or card. */
export function SectionTitle({ children }: { children: ReactNode }) {
  return <h2 className={styles.sectionTitle}>{children}</h2>
}

/** A small grey explanatory paragraph. */
export function Note({ children }: { children: ReactNode }) {
  return <p className={styles.note}>{children}</p>
}
