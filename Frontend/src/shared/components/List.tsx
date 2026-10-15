import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { ChevronIcon } from './icons'
import styles from './List.module.css'

/** A white rounded list of Rows and Facts, with an optional head. */
export function List({ head, children }: { head?: ReactNode; children: ReactNode }) {
  return (
    <div className={styles.list}>
      {head && <div className={styles.head}>{head}</div>}
      {children}
    </div>
  )
}

interface RowProps {
  lead?: ReactNode
  title: ReactNode
  sub?: ReactNode
  end?: ReactNode
  /** Makes the whole row a button with a chevron. */
  onClick?: () => void
  /** Makes the whole row a link with a chevron. */
  to?: string
  /** Its own button (e.g. a Tick); only for rows without onClick or to. */
  action?: ReactNode
}

// A row is either tappable or carries its own action button, never both.
export function Row({ lead, title, sub, end, onClick, to, action }: RowProps) {
  const inner = (
    <>
      {lead}
      <div className={styles.main}>
        <div className={styles.title}>{title}</div>
        {sub && <div className={styles.sub}>{sub}</div>}
      </div>
      {end && <div className={styles.end}>{end}</div>}
    </>
  )
  const chevron = (
    <span className={styles.chev}>
      <ChevronIcon />
    </span>
  )
  if (to) {
    return (
      <Link className={styles.row} to={to}>
        {inner}
        {chevron}
      </Link>
    )
  }
  if (onClick) {
    return (
      <button type="button" className={styles.row} onClick={onClick}>
        {inner}
        {chevron}
      </button>
    )
  }
  return (
    <div className={styles.row}>
      {inner}
      {action}
    </div>
  )
}

/** A label on the left and a value on the right, for details. */
export function Fact({ label, children }: { label: ReactNode; children: ReactNode }) {
  return (
    <div className={styles.fact}>
      <span>{label}</span>
      <span>{children}</span>
    </div>
  )
}
