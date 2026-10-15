import type { RentPeriod } from '../../../shared/api/types'
import { Card } from '../../../shared/components/Card'
import { Money } from '../../../shared/components/Money'
import { formatShortDate } from '../../../shared/format/date'
import styles from '../Dashboard.module.css'
import { OpenLink } from './OpenLink'

interface RentCardProps {
  meId: number
  periods: RentPeriod[]
}

export function RentCard({ meId, periods }: RentCardProps) {
  const next = periods
    .filter((p) => p.payee.id !== meId && p.shares.some((s) => s.user.id === meId && !s.paid))
    .sort((a, b) => a.startDate.localeCompare(b.startDate))[0]
  const share = next?.shares.find((s) => s.user.id === meId)
  return (
    <Card
      title="Rent"
      actions={
        <OpenLink to="/rent">Open rent</OpenLink>
      }
    >
      {next && share ? (
        <>
          <p className={styles.big}>
            <Money amount={share.amount} />
          </p>
          <p className={styles.sub}>
            Your next share, {formatShortDate(next.startDate)} – {formatShortDate(next.endDate)} · pay {next.payee.name}
          </p>
        </>
      ) : (
        <p className={`${styles.big} ${styles.pos}`}>All paid</p>
      )}
    </Card>
  )
}
