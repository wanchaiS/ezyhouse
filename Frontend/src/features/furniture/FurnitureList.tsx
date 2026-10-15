import type { Furniture } from '../../shared/api/types'
import { EmptyState } from '../../shared/components/EmptyState'
import { SofaIcon } from '../../shared/components/icons'
import { List, Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { formatShortDate } from '../../shared/format/date'
import { StatusPill } from './StatusPill'
import styles from './Furniture.module.css'

interface FurnitureListProps {
  items: Furniture[]
  /** Labels the pill on sold and disposed items, needed only when the list mixes statuses. */
  showStatus: boolean
  /** e.g. "Nothing sold yet." */
  emptyText: string
  onOpen: (item: Furniture) => void
}

const icon = (
  <span className={styles.iconCircle}>
    <SofaIcon />
  </span>
)

export function FurnitureList({ items, showStatus, emptyText, onOpen }: FurnitureListProps) {
  if (!items.length) return <EmptyState>{emptyText}</EmptyState>
  return (
    <List>
      {items.map((item) => (
        <Row
          key={item.id}
          lead={icon}
          title={
            <>
              {item.name}
              {showStatus && <StatusPill status={item.status} />}
            </>
          }
          sub={
            item.sale
              ? `Sold ${formatShortDate(item.sale.date)} · money to ${item.sale.receivedBy.name}`
              : item.disposal
                ? `Disposed ${formatShortDate(item.disposal.date)}`
                : `Bought ${formatShortDate(item.purchase.date)} · ${item.purchase.paidBy.name} paid`
          }
          end={<Money amount={item.sale?.amount ?? item.purchase.amount} />}
          onClick={() => onOpen(item)}
        />
      ))}
    </List>
  )
}
