import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { getBillTypes } from '../../shared/api/billTypes'
import { getExpenses } from '../../shared/api/expenses'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import { Button, IconButton, TextButton } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { ExpenseSheet, type ExpenseSheetState } from '../../shared/components/ExpenseSheet'
import { FirstRun } from '../../shared/components/FirstRun'
import { BillIcon, PlusIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Spinner } from '../../shared/components/Spinner'
import { Lead, Note } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import { createBillType, deleteBillType, updateBillType } from './api'
import { BillCards } from './BillCards'
import { BillTypeDetail } from './BillTypeDetail'
import { BillTypeForm } from './BillTypeForm'
import type { BillTypeInput } from './types'

// The bill type sheets; ids rather than objects, so a sheet shows the latest data after a refetch.
type TypeSheet =
  | { kind: 'type'; typeId: number }
  | { kind: 'typeForm'; typeId?: number }
  | { kind: 'deleteType'; typeId: number }
  | null

export function BillsPage() {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const [sheet, setSheet] = useState<TypeSheet>(null)
  const [expenseSheet, setExpenseSheet] = useState<ExpenseSheetState>(null)

  const me = useMe()
  const household = useHousehold()
  const bills = useQuery({ queryKey: keys.expenseList('bills'), queryFn: () => getExpenses('bills') })
  const billTypes = useQuery({ queryKey: keys.billTypes, queryFn: getBillTypes })

  // A new bill type due today creates its first bill straight away, and a deleted one unlinks
  // its bills, so the bills refresh with the bill types.
  const refreshBillTypes = () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: keys.billTypes }),
      queryClient.invalidateQueries({ queryKey: keys.expenses }),
      queryClient.invalidateQueries({ queryKey: keys.balances }),
    ])

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

  function open(next: TypeSheet) {
    removeType.reset()
    setSheet(next)
  }

  const loadError = me.error ?? household.error ?? billTypes.error ?? bills.error
  if (loadError) {
    return (
      <>
        <PageHeader title="Bills" />
        <ErrorMessage error={loadError} />
      </>
    )
  }
  if (!me.data || !household.data || !billTypes.data || !bills.data) return <Spinner />

  const meId = me.data.id
  const { primary, members } = household.data
  const isPrimary = primary.id === meId
  const viewedType =
    sheet && sheet.typeId !== undefined ? billTypes.data.find((billType) => billType.id === sheet.typeId) : undefined

  function sheetTitle() {
    switch (sheet?.kind) {
      case 'type':
        return viewedType?.name
      case 'typeForm':
        return sheet.typeId === undefined ? 'New bill type' : 'Edit bill type'
      case 'deleteType':
        return `Delete ${viewedType?.name ?? 'this bill'}?`
      default:
        return ''
    }
  }

  function sheetBody() {
    switch (sheet?.kind) {
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
            <Note>Bills already created stay. No new ones are created.</Note>
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
      {!billTypes.data.length && !bills.data.length ? (
        <FirstRun
          icon={<BillIcon />}
          title="No bills yet"
          action={
            isPrimary && (
              <Button variant="primary" onClick={() => open({ kind: 'typeForm' })}>
                Add your first bill
              </Button>
            )
          }
        >
          {isPrimary
            ? 'Add each bill your house shares once, say internet, monthly. A new one appears every time it’s due; you enter the amount, and everyone’s share counts on Home.'
            : `${primary.name} adds the bills your house shares. When one arrives, ${primary.name} enters the amount and your share shows here.`}
        </FirstRun>
      ) : (
        <>
          <Lead>
            {isPrimary
              ? 'Enter each bill when it arrives. Everyone’s share counts in the balances on Home.'
              : `${primary.name} enters each bill when it arrives. Settle up on Home.`}
          </Lead>
          <BillCards
            bills={bills.data}
            billTypes={billTypes.data}
            meId={meId}
            isPrimary={isPrimary}
            primary={primary}
            onEnter={(bill) => setExpenseSheet({ kind: 'edit', expense: bill })}
            onOpenBill={(bill) => setExpenseSheet({ kind: 'view', expense: bill })}
            onOpenType={(billType) => open({ kind: 'type', typeId: billType.id })}
          />
        </>
      )}
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
      <ExpenseSheet
        state={expenseSheet}
        onChange={setExpenseSheet}
        tag="bills"
        members={members}
        meId={meId}
        primaryName={primary.name}
      />
    </>
  )
}
