import type { Bond } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Note } from '../../shared/components/Text'
import { formatDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'

interface DeleteBondSheetProps {
  /** null keeps the sheet closed. */
  bond: Bond | null
  onClose: () => void
  onConfirm: (bond: Bond) => void
  deleting: boolean
  error: unknown
}

export function DeleteBondSheet({ bond, onClose, onConfirm, deleting, error }: DeleteBondSheetProps) {
  return (
    <Sheet open={bond !== null} onClose={onClose} title="Delete this bond record?">
      {bond && (
        <>
          <Note>
            The {formatMoney(bond.total)} bond paid on {formatDate(bond.paidDate)} and its contributions will be
            removed. Nobody's balance changes.
          </Note>
          <ErrorMessage error={error} />
          <SheetActions>
            <Button variant="danger" block disabled={deleting} onClick={() => onConfirm(bond)}>
              Delete bond
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
