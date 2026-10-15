import type { UserRef } from '../../shared/api/types'
import { Avatar } from '../../shared/components/Avatar'
import { Button, TextButton } from '../../shared/components/Button'
import { Fact, List, Row } from '../../shared/components/List'
import { Money } from '../../shared/components/Money'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Note, SectionTitle } from '../../shared/components/Text'
import { formatDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import { FREQUENCY_LABELS } from './format'
import type { RentSchedule } from './types'
import styles from './Rent.module.css'

interface ScheduleSheetProps {
  /** null keeps the sheet closed. */
  schedule: RentSchedule | null
  onClose: () => void
  isPrimary: boolean
  primary: UserRef
  onEdit: (schedule: RentSchedule) => void
  onDelete: (schedule: RentSchedule) => void
}

/** A schedule's details; Edit and Delete only for the primary. */
export function ScheduleSheet({ schedule, onClose, isPrimary, primary, onEdit, onDelete }: ScheduleSheetProps) {
  const title = schedule ? (
    <span className={styles.sheetHead}>
      <span>
        {formatMoney(schedule.amount)} {FREQUENCY_LABELS[schedule.frequency].toLowerCase()}
      </span>
      {isPrimary && <TextButton onClick={() => onEdit(schedule)}>Edit</TextButton>}
    </span>
  ) : (
    ''
  )
  return (
    <Sheet open={schedule !== null} onClose={onClose} title={title}>
      {schedule && (
        <>
          <List>
            <Fact label="Started">{formatDate(schedule.startDate)}</Fact>
            <Fact label="Ends">{schedule.endDate ? formatDate(schedule.endDate) : 'Ongoing'}</Fact>
            <Fact label="Next period">{schedule.nextDueDate ? formatDate(schedule.nextDueDate) : '—'}</Fact>
            <Fact label="Split">{schedule.splitType === 'equal' ? 'Equally' : 'Exact amounts'}</Fact>
          </List>
          <SectionTitle>Each period</SectionTitle>
          <List>
            {schedule.shares.map((share) => (
              <Row
                key={share.user.id}
                lead={<Avatar user={share.user} />}
                title={share.user.name}
                end={<Money amount={share.amount} />}
              />
            ))}
          </List>
          {isPrimary ? (
            <>
              <Note>Changes apply from the next period. Periods already created keep their amount and split.</Note>
              <SheetActions>
                <Button variant="quietDanger" block onClick={() => onDelete(schedule)}>
                  Delete
                </Button>
              </SheetActions>
            </>
          ) : (
            <p className={`${styles.center} ${styles.label}`}>Only {primary.name} (primary) can change the rent.</p>
          )}
        </>
      )}
    </Sheet>
  )
}
