import type { UserRef } from '../../shared/api/types'
import { Avatar } from '../../shared/components/Avatar'
import { List, Row } from '../../shared/components/List'
import { Note } from '../../shared/components/Text'
import styles from './Household.module.css'

interface MemberListProps {
  members: UserRef[]
  primaryId: number
  meId: number
  isPrimary: boolean
  /** Asks to hand primary to this member; only offered to the primary. */
  onPick: (member: UserRef) => void
}

export function MemberList({ members, primaryId, meId, isPrimary, onPick }: MemberListProps) {
  return (
    <>
      <List>
        {members.map((member) => (
          <Row
            key={member.id}
            lead={<Avatar user={member} />}
            title={
              <>
                {member.name}
                {member.id === meId && <span className={styles.muted}> (you)</span>}
              </>
            }
            sub={member.id === primaryId ? 'Primary · manages rent, bills and bond' : undefined}
            onClick={isPrimary && member.id !== meId ? () => onPick(member) : undefined}
          />
        ))}
      </List>
      {isPrimary && members.length > 1 && <Note>Tap a member to make them the primary.</Note>}
    </>
  )
}
