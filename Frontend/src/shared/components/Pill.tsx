import type { ReactNode } from 'react'
import styles from './Pill.module.css'

interface PillProps {
  tone?: 'pos' | 'neg' | 'warn'
  children: ReactNode
}

export function Pill({ tone, children }: PillProps) {
  return <span className={tone ? `${styles.pill} ${styles[tone]}` : styles.pill}>{children}</span>
}
