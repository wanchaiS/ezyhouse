import type { ReactNode } from 'react'
import styles from './FirstRun.module.css'

interface FirstRunProps {
  icon: ReactNode
  title: string
  children: ReactNode
  /** The one thing to do next, if the viewer may do it. */
  action?: ReactNode
}

/** A tab with nothing on it yet: one honest explanation and at most one action. */
export function FirstRun({ icon, title, children, action }: FirstRunProps) {
  return (
    <div className={styles.first}>
      <span className={styles.icon}>{icon}</span>
      <h2>{title}</h2>
      <p>{children}</p>
      {action}
    </div>
  )
}
