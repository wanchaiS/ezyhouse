import { formatMoney } from '../format/money'
import styles from './Money.module.css'

interface MoneyProps {
  amount: number
  tone?: 'pos' | 'neg' | 'muted'
}

/** An amount as "$1,234.50" in tabular figures, green, orange or grey if asked. */
export function Money({ amount, tone }: MoneyProps) {
  return <span className={tone ? `${styles.amount} ${styles[tone]}` : styles.amount}>{formatMoney(amount)}</span>
}
