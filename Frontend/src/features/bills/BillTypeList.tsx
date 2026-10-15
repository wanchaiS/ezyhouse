import { EmptyState } from '../../shared/components/EmptyState'
import { List, Row } from '../../shared/components/List'
import { formatShortDate } from '../../shared/format/date'
import { FREQUENCY_LABELS, type BillType } from './types'

interface BillTypeListProps {
  billTypes: BillType[]
  isPrimary: boolean
  onView: (billType: BillType) => void
}

export function BillTypeList({ billTypes, isPrimary, onView }: BillTypeListProps) {
  if (!billTypes.length) {
    return <EmptyState>{isPrimary ? 'No bill types yet. Tap + to add one.' : 'No bill types yet.'}</EmptyState>
  }
  return (
    <List>
      {billTypes.map((billType) => (
        <Row
          key={billType.id}
          title={billType.name}
          sub={`${FREQUENCY_LABELS[billType.frequency]} · next ${billType.nextDueDate ? formatShortDate(billType.nextDueDate) : '—'}`}
          onClick={() => onView(billType)}
        />
      ))}
    </List>
  )
}
