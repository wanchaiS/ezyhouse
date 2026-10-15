import type { Bill } from '../../../shared/api/types'
import { Card } from '../../../shared/components/Card'
import { Money } from '../../../shared/components/Money'
import { formatShortDate } from '../../../shared/format/date'
import styles from '../Dashboard.module.css'
import { ButtonLink } from '../../../shared/components/Button'

interface BillsCardProps {
  meId: number
  bills: Bill[]
}

export function BillsCard({ meId, bills }: BillsCardProps) {
  const pending = bills.filter((b) => b.status === 'pending').sort((a, b) => a.dueDate.localeCompare(b.dueDate))
  const owing = bills.flatMap((b) => {
    const share = b.payee?.id !== meId ? b.shares.find((s) => s.user.id === meId && !s.paid) : undefined
    return b.status === 'submitted' && share ? [{ bill: b, amount: share.amount }] : []
  })
  return (
    <Card
      title="Bills"
      actions={
        <ButtonLink to="/bills">Open bills</ButtonLink>
      }
    >
      <h4 className={styles.h4}>Waiting for an amount</h4>
      {pending.length ? (
        <ul className={styles.plain}>
          {pending.map((b) => (
            <li key={b.id}>
              <span>{b.billType.name}</span>
              <span className={styles.sub}>due {formatShortDate(b.dueDate)}</span>
            </li>
          ))}
        </ul>
      ) : (
        <p className={styles.muted}>Nothing waiting.</p>
      )}
      <h4 className={`${styles.h4} ${styles.gap}`}>You owe</h4>
      {owing.length ? (
        <ul className={styles.plain}>
          {owing.map(({ bill, amount }) => (
            <li key={bill.id}>
              <span>
                {bill.billType.name} · pay {bill.payee?.name}
              </span>
              <Money amount={amount} tone="neg" />
            </li>
          ))}
        </ul>
      ) : (
        <p className={styles.muted}>Nothing. Your bill shares are all paid.</p>
      )}
    </Card>
  )
}
