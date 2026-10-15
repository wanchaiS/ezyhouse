import type { PaidShare } from '../../shared/api/types'
import { Avatar } from '../../shared/components/Avatar'
import { Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { Tick } from '../../shared/components/Tick'
import { formatShortDate } from '../../shared/format/date'
import styles from './Rent.module.css'

interface ShareRowsProps {
  shares: PaidShare[]
  meId: number
  isPrimary: boolean
  /** Disables every tick while one is being saved. */
  busy: boolean
  onTick: (userId: number, paid: boolean) => void
}

/** One row per person with their paid tick; only that person or the primary may tick it. */
export function ShareRows({ shares, meId, isPrimary, busy, onTick }: ShareRowsProps) {
  return shares.map((share) => (
    <Row
      key={share.user.id}
      lead={<Avatar user={share.user} />}
      title={
        <>
          {share.user.name}
          {share.user.id === meId && <span className={styles.you}> (you)</span>}
        </>
      }
      sub={share.paid && share.paidDate ? `Paid ${formatShortDate(share.paidDate)}` : 'Not paid yet'}
      end={<Money amount={share.amount} />}
      action={
        <Tick
          paid={share.paid}
          disabled={busy || !(isPrimary || share.user.id === meId)}
          onChange={(paid) => onTick(share.user.id, paid)}
        />
      }
    />
  ))
}
