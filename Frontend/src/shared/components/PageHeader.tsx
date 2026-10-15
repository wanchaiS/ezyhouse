import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { BackIcon } from './icons'
import styles from './PageHeader.module.css'

interface PageHeaderProps {
  title: ReactNode
  /** Shown on the right, e.g. an accent IconButton with a plus. */
  action?: ReactNode
  /** Shows a Back link to this path above the title. */
  backTo?: string
}

export function PageHeader({ title, action, backTo }: PageHeaderProps) {
  return (
    <header className={styles.top}>
      <div>
        {backTo && (
          <Link className={styles.back} to={backTo}>
            <BackIcon />
            <span>Back</span>
          </Link>
        )}
        <h1>{title}</h1>
      </div>
      {action}
    </header>
  )
}
