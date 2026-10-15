import type { FurnitureStatus } from '../../shared/api/types'
import { Pill } from '../../shared/components/Pill'
import styles from './Furniture.module.css'

/** "Sold" or "Disposed" beside an item's name; nothing while it is owned. */
export function StatusPill({ status }: { status: FurnitureStatus }) {
  if (status === 'owned') return null
  return (
    <span className={styles.titlePill}>
      <Pill>{status === 'sold' ? 'Sold' : 'Disposed'}</Pill>
    </span>
  )
}
