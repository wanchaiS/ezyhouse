import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { getBills } from '../../shared/api/bills'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import type { Bill, BillStatus } from '../../shared/api/types'
import { Button, IconButton, TextButton } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { PlusIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Segmented } from '../../shared/components/Segmented'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Spinner } from '../../shared/components/Spinner'
import { Note, SectionTitle } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import {
  billTypesKey,
  createBillType,
  deleteBillType,
  getBillTypes,
  setBillSharePaid,
  submitBill,
  updateBillType,
} from './api'
import { BillDetail } from './BillDetail'
import { BillList } from './BillList'
import styles from './Bills.module.css'
import { BillTypeDetail } from './BillTypeDetail'
import { BillTypeForm } from './BillTypeForm'
import { BillTypeList } from './BillTypeList'
import { SubmitBillForm } from './SubmitBillForm'
import type { BillTypeInput, SubmitBillInput } from './types'

type SheetState =
  | { kind: 'submit'; bill: Bill }
  | { kind: 'bill'; billId: number }
  | { kind: 'type'; typeId: number }
  | { kind: 'typeForm'; typeId?: number }
  | { kind: 'deleteType'; typeId: number }
  | null

const FILTERS = [
  { value: 'pending', label: 'Pending' },
  { value: 'submitted', label: 'Submitted' },
] as const

export function BillsPage() {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const [status, setStatus] = useState<BillStatus>('pending')
  const [sheet, setSheet] = useState<SheetState>(null)

  const me = useMe()
  const household = useHousehold()
  const bills = useQuery({ queryKey: keys.billList(status), queryFn: () => getBills(status) })
  const billTypes = useQuery({ queryKey: billTypesKey, queryFn: getBillTypes })

  // Bill type changes add or remove placeholders, so the bill lists refresh too.
  const refreshBillTypes = () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: billTypesKey }),
      queryClient.invalidateQueries({ queryKey: keys.bills }),
    ])

  const submit = useMutation({
    mutationFn: ({ billId, input }: { billId: number; input: SubmitBillInput }) => submitBill(billId, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: keys.bills })
      setSheet(null)
      showToast('Bill submitted')
    },
  })

  const tick = useMutation({
    mutationFn: ({ billId, userId, paid }: { billId: number; userId: number; paid: boolean }) =>
      setBillSharePaid(billId, userId, paid),
    // Keeps the ticks disabled until the list shows the new state.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: keys.bills }),
  })

  const saveType = useMutation({
    mutationFn: ({ id, input }: { id?: number; input: BillTypeInput }) =>
      id === undefined ? createBillType(input) : updateBillType(id, input),
    onSuccess: (_, { id }) => {
      refreshBillTypes()
      setSheet(null)
      showToast(id === undefined ? 'Bill type added' : 'Bill type saved')
    },
  })

  const removeType = useMutation({
    mutationFn: deleteBillType,
    onSuccess: () => {
      refreshBillTypes()
      setSheet(null)
      showToast('Bill type deleted')
    },
  })

  function open(next: SheetState) {
    tick.reset()
    removeType.reset()
    setSheet(next)
  }

  const loadError = me.error ?? household.error ?? billTypes.error
  if (loadError) return <ErrorMessage error={loadError} />
  if (!me.data || !household.data || !billTypes.data) return <Spinner />

  const meId = me.data.id
  const { primary, members } = household.data
  const isPrimary = primary.id === meId
  const findType = (id: number) => billTypes.data.find((billType) => billType.id === id)

  // Pending: soonest first; submitted: newest first.
  const shownBills = [...(bills.data ?? [])].sort((a, b) =>
    status === 'pending' ? a.dueDate.localeCompare(b.dueDate) : b.dueDate.localeCompare(a.dueDate),
  )
  const viewedBill = sheet?.kind === 'bill' ? bills.data?.find((bill) => bill.id === sheet.billId) : undefined
  const viewedType = sheet && 'typeId' in sheet && sheet.typeId !== undefined ? findType(sheet.typeId) : undefined

  function sheetTitle() {
    switch (sheet?.kind) {
      case 'submit':
        return `${sheet.bill.billType.name} bill`
      case 'bill':
        return viewedBill?.billType.name ?? 'Bill'
      case 'type':
        return viewedType?.name
      case 'typeForm':
        return sheet.typeId === undefined ? 'New bill type' : 'Edit bill type'
      case 'deleteType':
        return 'Delete this bill type?'
      default:
        return ''
    }
  }

  function sheetBody() {
    switch (sheet?.kind) {
      case 'submit': {
        const bill = sheet.bill
        return (
          <SubmitBillForm
            dueDate={bill.dueDate}
            typeMembers={findType(bill.billType.id)?.members ?? members}
            members={members}
            onSubmit={(input) => submit.mutateAsync({ billId: bill.id, input })}
            onCancel={() => setSheet(null)}
          />
        )
      }
      case 'bill':
        return (
          viewedBill && (
            <BillDetail
              bill={viewedBill}
              meId={meId}
              isPrimary={isPrimary}
              saving={tick.isPending}
              error={tick.error}
              onTick={(userId, paid) => tick.mutate({ billId: viewedBill.id, userId, paid })}
            />
          )
        )
      case 'type':
        return (
          viewedType && (
            <BillTypeDetail
              billType={viewedType}
              isPrimary={isPrimary}
              primaryName={primary.name}
              onDelete={() => open({ kind: 'deleteType', typeId: viewedType.id })}
            />
          )
        )
      case 'typeForm':
        return (
          <BillTypeForm
            billType={viewedType}
            members={members}
            onSubmit={(input) => saveType.mutateAsync({ id: sheet.typeId, input })}
            onCancel={() => setSheet(null)}
          />
        )
      case 'deleteType':
        return (
          <>
            <Note>Bills still waiting for an amount are removed. Past bills stay in the history.</Note>
            <ErrorMessage error={removeType.error} />
            <SheetActions>
              <Button
                variant="danger"
                block
                disabled={removeType.isPending}
                onClick={() => removeType.mutate(sheet.typeId)}
              >
                Delete
              </Button>
              <Button variant="plain" block onClick={() => setSheet(null)}>
                Cancel
              </Button>
            </SheetActions>
          </>
        )
      default:
        return null
    }
  }

  return (
    <>
      <PageHeader
        title="Bills"
        action={
          isPrimary && (
            <IconButton accent label="New bill type" onClick={() => open({ kind: 'typeForm' })}>
              <PlusIcon />
            </IconButton>
          )
        }
      />
      <div className={styles.filter}>
        <Segmented options={FILTERS} value={status} onChange={setStatus} />
      </div>
      {bills.isError ? (
        <ErrorMessage error={bills.error} />
      ) : bills.isPending ? (
        <Spinner />
      ) : (
        <BillList
          status={status}
          bills={shownBills}
          meId={meId}
          isPrimary={isPrimary}
          primaryName={primary.name}
          onEnter={(bill) => open({ kind: 'submit', bill })}
          onView={(bill) => open({ kind: 'bill', billId: bill.id })}
        />
      )}
      <SectionTitle>Bill types</SectionTitle>
      <BillTypeList
        billTypes={billTypes.data}
        isPrimary={isPrimary}
        onView={(billType) => open({ kind: 'type', typeId: billType.id })}
      />
      <Note>Bills aren't part of balances. Everyone owes the primary who paid the provider.</Note>
      <Sheet
        open={sheet !== null}
        onClose={() => setSheet(null)}
        title={sheetTitle()}
        titleAction={
          sheet?.kind === 'type' && isPrimary ? (
            <TextButton onClick={() => open({ kind: 'typeForm', typeId: sheet.typeId })}>Edit</TextButton>
          ) : undefined
        }
      >
        {sheetBody()}
      </Sheet>
    </>
  )
}
