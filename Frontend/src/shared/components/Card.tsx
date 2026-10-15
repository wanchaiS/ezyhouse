import type { ReactNode } from 'react'
import styles from './Card.module.css'

interface CardProps {
  title?: ReactNode
  /** Buttons shown in a row along the bottom, sharing the width. */
  actions?: ReactNode
  children?: ReactNode
}

export function Card({ title, actions, children }: CardProps) {
  return (
    <section className={styles.card}>
      {title && <h3>{title}</h3>}
      {children}
      {actions && <div className={styles.actions}>{actions}</div>}
    </section>
  )
}

/** One column on a phone, two from 820px. */
export function CardGrid({ children }: { children: ReactNode }) {
  return <div className={styles.grid}>{children}</div>
}
