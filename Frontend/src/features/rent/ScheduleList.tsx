import type { RentSchedule } from '../../shared/api/types'
import { List, Row } from '../../shared/components/List'
import { formatDate, formatShortDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import { FREQUENCY_LABELS } from './format'

interface ScheduleListProps {
  schedules: RentSchedule[]
  onOpen: (schedule: RentSchedule) => void
}

export function ScheduleList({ schedules, onOpen }: ScheduleListProps) {
  return (
    <List>
      {schedules.map((schedule) => (
        <Row
          key={schedule.id}
          title={`${formatMoney(schedule.amount)} ${FREQUENCY_LABELS[schedule.frequency].toLowerCase()}`}
          sub={
            schedule.endDate
              ? `${formatShortDate(schedule.startDate)} – ${formatShortDate(schedule.endDate)}`
              : `Since ${formatDate(schedule.startDate)}`
          }
          end={schedule.nextDueDate ? `Next ${formatShortDate(schedule.nextDueDate)}` : 'Ended'}
          onClick={() => onOpen(schedule)}
        />
      ))}
    </List>
  )
}
