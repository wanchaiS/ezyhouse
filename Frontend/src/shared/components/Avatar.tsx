import type { UserRef } from '../api/types'
import styles from './Avatar.module.css'

const COLOURS = ['#6366f1', '#0891b2', '#d97706', '#db2777', '#059669', '#7c3aed', '#dc2626']

interface AvatarProps {
  user: UserRef
  small?: boolean
}

/** The person's initial on a colour that stays the same for them everywhere. */
export function Avatar({ user, small }: AvatarProps) {
  return (
    <span
      className={small ? `${styles.avatar} ${styles.sm}` : styles.avatar}
      style={{ background: COLOURS[user.id % COLOURS.length] }}
      aria-hidden="true"
    >
      {user.name.charAt(0).toUpperCase()}
    </span>
  )
}
