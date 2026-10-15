import { Card } from '../../../shared/components/Card'
import styles from '../Dashboard.module.css'
import { OpenLink } from './OpenLink'

export function FurnitureCard({ ownedCount }: { ownedCount: number }) {
  return (
    <Card
      title="Furniture"
      actions={
        <OpenLink to="/furniture">Open furniture</OpenLink>
      }
    >
      <p className={styles.big}>
        {ownedCount} {ownedCount === 1 ? 'item' : 'items'} owned
      </p>
    </Card>
  )
}
