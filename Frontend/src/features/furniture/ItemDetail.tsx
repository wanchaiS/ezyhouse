import type { Furniture, Share } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { Fact, List } from '../../shared/components/List'
import { SheetActions } from '../../shared/components/Sheet'
import { Note } from '../../shared/components/Text'
import { formatDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import styles from './Furniture.module.css'

interface ItemDetailProps {
  item: Furniture
  /** The person who added the item, or the primary. */
  canManage: boolean
  onSell: () => void
  onDispose: () => void
  onDelete: () => void
}

// The API lists shares in no set order, so sort them to read the same in every section.
function SharesNote({ shares }: { shares: Share[] }) {
  const sorted = [...shares].sort((a, b) => a.user.name.localeCompare(b.user.name))
  return <Note>{sorted.map((share) => `${share.user.name} ${formatMoney(share.amount)}`).join(' · ')}</Note>
}

/** An item's purchase, then its sale or disposal, then what the viewer may do with it. */
export function ItemDetail({ item, canManage, onSell, onDispose, onDelete }: ItemDetailProps) {
  const { purchase, sale, disposal } = item
  return (
    <>
      <div className={styles.section}>
        <List>
          <Fact label="Bought">{formatDate(purchase.date)}</Fact>
          <Fact label="Price">{formatMoney(purchase.amount)}</Fact>
          <Fact label="Paid by">{purchase.paidBy.name}</Fact>
        </List>
        <SharesNote shares={purchase.shares} />
      </div>
      {sale && (
        <div className={styles.section}>
          <List>
            <Fact label="Sold">{formatDate(sale.date)}</Fact>
            <Fact label="Sold for">{formatMoney(sale.amount)}</Fact>
            <Fact label="Money went to">{sale.receivedBy.name}</Fact>
          </List>
          <SharesNote shares={sale.shares} />
        </div>
      )}
      {disposal && (
        <div className={styles.section}>
          <List>
            <Fact label="Disposed">{formatDate(disposal.date)}</Fact>
            <Fact label="Cost">{formatMoney(disposal.amount)}</Fact>
            <Fact label="Paid by">{disposal.paidBy.name}</Fact>
          </List>
          {disposal.amount > 0 && <SharesNote shares={disposal.shares} />}
        </div>
      )}
      {!canManage ? (
        <p className={styles.centerNote}>
          Only {item.createdBy.name} (who added it) or the primary can change this.
        </p>
      ) : (
        <SheetActions>
          {/* Selling or disposing is only possible once, while the item is still owned. */}
          {item.status === 'owned' && (
            <>
              <Button variant="primary" block onClick={onSell}>
                Sell
              </Button>
              <Button block onClick={onDispose}>
                Get rid of it
              </Button>
            </>
          )}
          <Button variant="quietDanger" block onClick={onDelete}>
            Delete
          </Button>
        </SheetActions>
      )}
    </>
  )
}
