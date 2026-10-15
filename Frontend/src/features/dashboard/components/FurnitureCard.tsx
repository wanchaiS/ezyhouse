import { Card } from '../../../shared/components/Card'
import styles from '../Dashboard.module.css'
import { ButtonLink } from '../../../shared/components/Button'

export function FurnitureCard({ ownedCount }: { ownedCount: number }) {
  return (
    <Card
      title="Furniture"
      actions={
        <ButtonLink to="/furniture">Open furniture</ButtonLink>
      }
    >
      <p className={styles.big}>
        {ownedCount} {ownedCount === 1 ? 'item' : 'items'} owned
      </p>
    </Card>
  )
}
