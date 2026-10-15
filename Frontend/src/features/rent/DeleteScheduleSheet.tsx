import { Button } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Note } from '../../shared/components/Text'
import { formatMoney } from '../../shared/format/money'
import { FREQUENCY_LABELS } from './format'
import type { RentSchedule } from './types'

interface DeleteScheduleSheetProps {
  /** null keeps the sheet closed. */
  schedule: RentSchedule | null
  onClose: () => void
  onConfirm: (schedule: RentSchedule) => void
  deleting: boolean
  error: unknown
}

export function DeleteScheduleSheet({ schedule, onClose, onConfirm, deleting, error }: DeleteScheduleSheetProps) {
  return (
    <Sheet open={schedule !== null} onClose={onClose} title="Delete this rent?">
      {schedule && (
        <>
          <Note>
            {formatMoney(schedule.amount)} {FREQUENCY_LABELS[schedule.frequency].toLowerCase()}. Deleting it also
            deletes all of its periods and their paid ticks.
          </Note>
          <ErrorMessage error={error} />
          <SheetActions>
            <Button variant="danger" block disabled={deleting} onClick={() => onConfirm(schedule)}>
              Delete rent
            </Button>
            <Button variant="plain" block onClick={onClose}>
              Cancel
            </Button>
          </SheetActions>
        </>
      )}
    </Sheet>
  )
}
