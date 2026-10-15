import type { Bond } from '../../shared/api/types'
import { ShieldIcon } from '../../shared/components/icons'
import { List, Row } from '../../shared/components/List'
import { formatMoney } from '../../shared/format/money'
import styles from './Household.module.css'

/** One row per bond (total, still to collect), each opening the Bond screen. */
export function BondSummary({ bonds }: { bonds: Bond[] }) {
  const shield = (
    <span className={styles.iconCircle}>
      <ShieldIcon />
    </span>
  )
  return (
    <List>
      {bonds.length ? (
        bonds.map((bond) => (
          <Row
            key={bond.id}
            lead={shield}
            title={formatMoney(bond.total)}
            sub={bond.outstanding > 0 ? `${formatMoney(bond.outstanding)} still to collect` : 'Fully paid'}
            to="/bond"
          />
        ))
      ) : (
        <Row lead={shield} title="No bond recorded" to="/bond" />
      )}
    </List>
  )
}
