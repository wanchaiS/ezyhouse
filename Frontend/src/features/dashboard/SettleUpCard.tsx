import type { ReactNode } from 'react'
import type { UserRef } from '../../shared/api/types'
import { Avatar } from '../../shared/components/Avatar'
import { Button } from '../../shared/components/Button'
import { CheckIcon } from '../../shared/components/icons'
import { formatMoney } from '../../shared/format/money'
import styles from './Home.module.css'
import type { Debt } from './types'

interface SettleUpCardProps {
  debts: Debt[]
  meId: number
  /** Opens the payment form for this balance. */
  onSettle: (debt: Debt) => void
}

/**
 * One amount per person the viewer owes or is owed by. Each is paid separately, so they are
 * never added up into one total.
 */
export function SettleUpCard({ debts, meId, onSettle }: SettleUpCardProps) {
  const iOwe = debts.filter((debt) => debt.from.id === meId)
  const owedToMe = debts.filter((debt) => debt.to.id === meId)
  const others = debts.filter((debt) => debt.from.id !== meId && debt.to.id !== meId)

  return (
    <section className={styles.card} aria-labelledby="settle-title">
      <h2 id="settle-title" className={styles.cardTitle}>
        Settle up
      </h2>
      {iOwe.length + owedToMe.length === 0 ? (
        <p className={styles.square}>
          <CheckIcon /> You’re all square
        </p>
      ) : (
        <ul className={styles.debts}>
          {/* What you owe first: that's the one only you can act on. */}
          {iOwe.map((debt) => (
            <DebtRow key={debt.to.id} person={debt.to} text={`You owe ${debt.to.name}`} amount={debt.amount} tone={styles.neg}>
              <Button small variant="primary" onClick={() => onSettle(debt)}>
                I’ve paid {debt.to.name}
              </Button>
            </DebtRow>
          ))}
          {owedToMe.map((debt) => (
            <DebtRow key={debt.from.id} person={debt.from} text={`${debt.from.name} owes you`} amount={debt.amount} tone={styles.pos}>
              <Button small onClick={() => onSettle(debt)}>
                {debt.from.name} paid me
              </Button>
            </DebtRow>
          ))}
        </ul>
      )}
      {others.length > 0 && (
        <>
          <h3 className={styles.othersHead}>Between others</h3>
          <ul className={styles.others}>
            {others.map((debt) => (
              <li key={`${debt.from.id}-${debt.to.id}`}>
                <span>
                  {debt.from.name} owes {debt.to.name}
                </span>
                <b>{formatMoney(debt.amount)}</b>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  )
}

interface DebtRowProps {
  person: UserRef
  text: string
  amount: number
  tone: string
  children: ReactNode
}

function DebtRow({ person, text, amount, tone, children }: DebtRowProps) {
  return (
    <li className={styles.debt}>
      <Avatar user={person} small />
      <span className={styles.debtText}>{text}</span>
      <b className={`${styles.debtAmount} ${tone}`}>{formatMoney(amount)}</b>
      <span className={styles.debtAction}>{children}</span>
    </li>
  )
}
