import type { Debt, UserRef } from '../../../shared/api/types'
import { Button } from '../../../shared/components/Button'
import { Card } from '../../../shared/components/Card'
import { Money } from '../../../shared/components/Money'
import { formatMoney } from '../../../shared/format/money'
import styles from '../Dashboard.module.css'
import { OpenLink } from './OpenLink'

interface NetCardProps {
  me: UserRef
  net: number
  debts: Debt[]
  onSettle: () => void
}

export function NetCard({ me, net, debts, onSettle }: NetCardProps) {
  const mine = debts.filter((d) => d.from.id === me.id || d.to.id === me.id)
  return (
    <Card
      title="Groceries & shared costs"
      actions={
        <>
          <Button variant="primary" onClick={onSettle}>
            Settle up
          </Button>
          <OpenLink to="/groceries">Open groceries</OpenLink>
        </>
      }
    >
      {net > 0 ? (
        <p className={`${styles.big} ${styles.pos}`}>You're owed {formatMoney(net)}</p>
      ) : net < 0 ? (
        <p className={`${styles.big} ${styles.neg}`}>You owe {formatMoney(-net)}</p>
      ) : (
        <p className={styles.big}>You're all square</p>
      )}
      <p className={styles.sub}>Groceries plus furniture. Paid back with settle-up.</p>
      {mine.length > 0 && (
        <>
          <h4 className={`${styles.h4} ${styles.gap}`}>Who owes whom</h4>
          <ul className={styles.plain}>
            {mine.map((d) => (
              <li key={`${d.from.id}-${d.to.id}`}>
                <span>
                  {d.from.id === me.id ? 'You' : d.from.name} → {d.to.id === me.id ? 'you' : d.to.name}
                </span>
                <Money amount={d.amount} tone={d.from.id === me.id ? 'neg' : 'pos'} />
              </li>
            ))}
          </ul>
        </>
      )}
    </Card>
  )
}
