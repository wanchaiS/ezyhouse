import type { Settlement } from '../../shared/api/types'
import { Card } from '../../shared/components/Card'
import { List, Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { Note } from '../../shared/components/Text'
import { DateBadge } from './DateBadge'
import styles from './Groceries.module.css'

/** Every settlement, newest first. They are append-only. */
export function SettlementHistory({ settlements }: { settlements: Settlement[] }) {
  return (
    <Card title="Settlements">
      {settlements.length ? (
        <div className={styles.flush}>
          <List>
            {settlements.map((settlement) => (
              <Row
                key={settlement.id}
                lead={<DateBadge date={settlement.date} />}
                title={`${settlement.from.name} paid ${settlement.to.name}`}
                sub={`Recorded by ${settlement.createdBy.name}`}
                end={<Money amount={settlement.amount} tone="muted" />}
              />
            ))}
          </List>
        </div>
      ) : (
        <p className={styles.sub}>No settlements yet.</p>
      )}
      <Note>Settlements can't be undone. To fix a mistake, record a settlement the other way.</Note>
    </Card>
  )
}
