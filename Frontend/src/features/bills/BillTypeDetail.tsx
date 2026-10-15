import { Button } from '../../shared/components/Button'
import { Fact, List } from '../../shared/components/List'
import { SheetActions } from '../../shared/components/Sheet'
import { Note } from '../../shared/components/Text'
import { formatDate } from '../../shared/format/date'
import styles from './Bills.module.css'
import { FREQUENCY_LABELS, type BillType } from './types'

interface BillTypeDetailProps {
  billType: BillType
  isPrimary: boolean
  primaryName: string
  onDelete: () => void
}

/** A bill type's facts; the primary also gets Delete (Edit sits in the sheet's title). */
export function BillTypeDetail({ billType, isPrimary, primaryName, onDelete }: BillTypeDetailProps) {
  return (
    <>
      <List>
        <Fact label="How often">{FREQUENCY_LABELS[billType.frequency]}</Fact>
        <Fact label="First due">{formatDate(billType.startDate)}</Fact>
        <Fact label="Next due">{billType.nextDueDate ? formatDate(billType.nextDueDate) : '—'}</Fact>
        <Fact label="Ends">{billType.endDate ? formatDate(billType.endDate) : 'Ongoing'}</Fact>
        <Fact label="Shared by">
          {billType.members.length ? billType.members.map((member) => member.name).join(', ') : 'Nobody'}
        </Fact>
      </List>
      {isPrimary ? (
        <SheetActions>
          <Button variant="quietDanger" block onClick={onDelete}>
            Delete
          </Button>
        </SheetActions>
      ) : (
        <div className={styles.center}>
          <Note>Only {primaryName} (primary) can change bill types.</Note>
        </div>
      )}
    </>
  )
}
