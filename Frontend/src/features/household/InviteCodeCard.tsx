import { Button } from '../../shared/components/Button'
import { Card } from '../../shared/components/Card'
import styles from './Household.module.css'

interface InviteCodeCardProps {
  code: string
  onCopy: () => void
}

export function InviteCodeCard({ code, onCopy }: InviteCodeCardProps) {
  return (
    <Card actions={<Button onClick={onCopy}>Copy code</Button>}>
      <div className={styles.label}>Invite code</div>
      <div className={styles.code}>{code}</div>
      <div className={styles.sub}>Share it so housemates can join.</div>
    </Card>
  )
}
