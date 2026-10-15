import type { Bond } from '../../shared/api/types'
import { Avatar } from '../../shared/components/Avatar'
import { TextButton } from '../../shared/components/Button'
import { Fact, List, Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { formatDate, formatShortDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import styles from './Bond.module.css'

interface BondCardProps {
  bond: Bond
  /** Shows Edit; only for the primary. */
  onEdit?: (bond: Bond) => void
}

/** One bond: its total, when it was paid, who put in what and what is still to collect. */
export function BondCard({ bond, onEdit }: BondCardProps) {
  return (
    <List
      head={
        <div className={styles.headRow}>
          <div>
            <div className={styles.label}>Paid to the agent on {formatDate(bond.paidDate)}</div>
            <div className={styles.big}>{formatMoney(bond.total)}</div>
          </div>
          {onEdit && (
            <TextButton onClick={() => onEdit(bond)} aria-label={`Edit bond of ${formatMoney(bond.total)}`}>
              Edit
            </TextButton>
          )}
        </div>
      }
    >
      {bond.contributions.map((contribution, index) => (
        <Row
          key={index}
          lead={<Avatar user={contribution.user} />}
          title={contribution.user.name}
          sub={`Put in ${formatShortDate(contribution.date)}`}
          end={<Money amount={contribution.amount} />}
        />
      ))}
      <Fact label="Still to collect">
        <Money amount={bond.outstanding} tone={bond.outstanding > 0 ? 'neg' : 'pos'} />
      </Fact>
    </List>
  )
}
