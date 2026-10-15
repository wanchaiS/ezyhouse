import type { Balances } from '../../shared/api/types'
import { Avatar } from '../../shared/components/Avatar'
import { Button } from '../../shared/components/Button'
import { Card } from '../../shared/components/Card'
import { Money } from '../../shared/components/Money'
import { Note } from '../../shared/components/Text'
import { formatMoney } from '../../shared/format/money'
import styles from './Groceries.module.css'
import { BalancesChart } from './BalancesChart'

interface BalancesCardProps {
  balances: Balances
  meId: number
  onSettle: () => void
}

/** Everyone's net and every pairwise debt, headed by where the viewer stands. */
export function BalancesCard({ balances, meId, onSettle }: BalancesCardProps) {
  const net = balances.members.find((member) => member.user.id === meId)?.net ?? 0

  return (
    <Card
      title="Balances"
      actions={
        <Button variant="primary" onClick={onSettle}>
          Record a settlement
        </Button>
      }
    >
      <div className={styles.big}>
        {net > 0 ? (
          <span className={styles.pos}>You're owed {formatMoney(net)}</span>
        ) : net < 0 ? (
          <span className={styles.neg}>You owe {formatMoney(-net)}</span>
        ) : (
          "You're all settled"
        )}
      </div>

      <BalancesChart members={balances.members} meId={meId} />

      <div className={styles.lines}>
        {balances.members.map(({ user, net }) => (
          <div className={styles.line} key={user.id}>
            <span className={styles.who}>
              <Avatar user={user} small />
              {user.id === meId ? `${user.name} (you)` : user.name}
            </span>
            {net > 0 ? (
              <span>
                <span className={styles.sub}>is owed </span>
                <Money amount={net} tone="pos" />
              </span>
            ) : net < 0 ? (
              <span>
                <span className={styles.sub}>owes </span>
                <Money amount={-net} tone="neg" />
              </span>
            ) : (
              <span className={styles.sub}>settled</span>
            )}
          </div>
        ))}
      </div>

      <h4 className={styles.subhead}>Who owes whom</h4>
      {balances.debts.length ? (
        <div className={styles.lines}>
          {balances.debts.map((debt) => (
            <div className={styles.line} key={`${debt.from.id}-${debt.to.id}`}>
              <span>
                {debt.from.id === meId ? 'You' : debt.from.name} owe{debt.from.id === meId ? '' : 's'}{' '}
                {debt.to.id === meId ? 'you' : debt.to.name}
              </span>
              <Money amount={debt.amount} tone={debt.from.id === meId ? 'neg' : debt.to.id === meId ? 'pos' : undefined} />
            </div>
          ))}
        </div>
      ) : (
        <p className={styles.sub}>Nobody owes anybody.</p>
      )}
      <Note>Groceries and furniture only. Rent, bills and bond have their own screens.</Note>
    </Card>
  )
}
