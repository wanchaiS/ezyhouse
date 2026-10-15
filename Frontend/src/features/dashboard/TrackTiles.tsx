import { Link } from 'react-router'
import type { ExpenseTag } from '../../shared/api/types'
import { PlusIcon } from '../../shared/components/icons'
import styles from './Home.module.css'
import { TAGS } from './tags'

/** Quiet tiles for tags the house doesn't use yet; each opens its tab to start one. */
export function TrackTiles({ tags }: { tags: ExpenseTag[] }) {
  return (
    <div className={styles.addGrid}>
      {tags.map((tag) => (
        <Link key={tag} className={styles.add} to={TAGS[tag].to}>
          <span className={styles.iconCircle}>{TAGS[tag].icon}</span>
          <span className={styles.addText}>
            <span className={styles.addTitle}>{TAGS[tag].label}</span>
            <span className={styles.addPitch}>{TAGS[tag].pitch}</span>
          </span>
          <span className={styles.plus}>
            <PlusIcon />
          </span>
        </Link>
      ))}
    </div>
  )
}
