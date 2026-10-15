import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { getFurniture } from '../../shared/api/furniture'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import type { Furniture, FurnitureStatus } from '../../shared/api/types'
import { Button, IconButton, TextButton } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { PlusIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Segmented } from '../../shared/components/Segmented'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Spinner } from '../../shared/components/Spinner'
import { Note } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import { createFurniture, deleteFurniture, disposeFurniture, sellFurniture, updateFurniture } from './api'
import styles from './Furniture.module.css'
import { FurnitureList } from './FurnitureList'
import { ItemDetail } from './ItemDetail'
import { ItemForm } from './ItemForm'
import { SellOrDisposeForm } from './SellOrDisposeForm'
import { StatusPill } from './StatusPill'
import type { DisposalInput, FurnitureInput, SaleInput } from './types'

type Filter = FurnitureStatus | 'all'

type ItemSheet = { kind: 'add' } | { kind: 'view' | 'edit' | 'sell' | 'dispose' | 'delete'; id: number }

const FILTERS: { value: Filter; label: string; empty: string }[] = [
  { value: 'owned', label: 'Owned', empty: 'Nothing owned yet.' },
  { value: 'sold', label: 'Sold', empty: 'Nothing sold yet.' },
  { value: 'disposed', label: 'Disposed', empty: 'Nothing disposed of yet.' },
  { value: 'all', label: 'All', empty: 'No furniture yet.' },
]

const newestFirst = (a: Furniture, b: Furniture) => b.purchase.date.localeCompare(a.purchase.date) || b.id - a.id

export function FurniturePage() {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const me = useMe().data
  const household = useHousehold()
  const [filter, setFilter] = useState<Filter>('owned')
  const [sheet, setSheet] = useState<ItemSheet | null>(null)
  // One list for the counts, the filtered rows and the open sheet, so they always agree.
  const all = useQuery({ queryKey: keys.furnitureList(), queryFn: () => getFurniture() })

  // A failed write (e.g. a 409 because someone else already sold the item) means the list is
  // out of date, so it refreshes too.
  const refreshFurniture = () => queryClient.invalidateQueries({ queryKey: keys.furniture })

  function finish(message: string) {
    refreshFurniture()
    // Furniture money counts in balances, so every write refreshes them too.
    queryClient.invalidateQueries({ queryKey: keys.balances })
    // Close the sheet first: a toast under an open dialog is hidden.
    setSheet(null)
    showToast(message)
  }

  const save = useMutation({
    mutationFn: ({ id, input }: { id?: number; input: FurnitureInput }) =>
      id === undefined ? createFurniture(input) : updateFurniture(id, input),
    onSuccess: (_, { id }) => finish(id === undefined ? 'Item added' : 'Item saved'),
    onError: refreshFurniture,
  })
  const sell = useMutation({
    mutationFn: ({ id, input }: { id: number; input: SaleInput }) => sellFurniture(id, input),
    onSuccess: () => finish('Sale recorded'),
    onError: refreshFurniture,
  })
  const dispose = useMutation({
    mutationFn: ({ id, input }: { id: number; input: DisposalInput }) => disposeFurniture(id, input),
    onSuccess: () => finish('Disposal recorded'),
    onError: refreshFurniture,
  })
  const remove = useMutation({
    mutationFn: deleteFurniture,
    onSuccess: () => finish('Item deleted'),
    onError: refreshFurniture,
  })

  const failed = [household, all].find((query) => query.isError)
  if (failed) {
    return (
      <>
        <PageHeader title="Furniture" />
        <ErrorMessage error={failed.error} />
      </>
    )
  }
  if (!me || !household.data || !all.data) return <Spinner />

  const members = household.data.members
  const isPrimary = household.data.primary.id === me.id
  const selected = sheet && sheet.kind !== 'add' ? all.data.find((item) => item.id === sheet.id) : undefined
  // A selected item that has gone (e.g. deleted elsewhere) closes the sheet.
  const open = sheet !== null && (sheet.kind === 'add' || selected !== undefined)
  const counts: Record<Filter, number> = { owned: 0, sold: 0, disposed: 0, all: all.data.length }
  for (const item of all.data) counts[item.status]++
  const canManage = !!selected && (selected.createdBy.id === me.id || isPrimary)
  const current = FILTERS.find((option) => option.value === filter)!
  const shown = all.data.filter((item) => filter === 'all' || item.status === filter).sort(newestFirst)

  function closeSheet() {
    setSheet(null)
    remove.reset()
  }

  function sheetTitle() {
    switch (sheet?.kind) {
      case 'add':
        return 'Add furniture'
      case 'edit':
        return 'Edit item'
      case 'sell':
        return `Sell ${selected?.name}`
      case 'dispose':
        return `Get rid of ${selected?.name}`
      case 'delete':
        return 'Delete this item?'
      default:
        return (
          selected && (
            <>
              {selected.name}
              <StatusPill status={selected.status} />
            </>
          )
        )
    }
  }

  return (
    <>
      <PageHeader
        title="Furniture"
        action={
          <IconButton label="Add furniture" accent onClick={() => setSheet({ kind: 'add' })}>
            <PlusIcon />
          </IconButton>
        }
      />
      <div className={styles.filter}>
        <Segmented
          options={FILTERS.map((option) => ({
            value: option.value,
            label: (
              <>
                {option.label}
                <span className={styles.count}>{counts[option.value]}</span>
              </>
            ),
          }))}
          value={filter}
          onChange={setFilter}
        />
      </div>
      <FurnitureList
        items={shown}
        showStatus={filter === 'all'}
        emptyText={current.empty}
        onOpen={(item) => setSheet({ kind: 'view', id: item.id })}
      />
      <Note>Furniture counts in balances. Settle up on the Groceries tab.</Note>

      <Sheet
        open={open}
        onClose={closeSheet}
        title={sheetTitle()}
        titleAction={
          sheet?.kind === 'view' && canManage && selected?.status === 'owned' ? (
            <TextButton onClick={() => setSheet({ kind: 'edit', id: selected.id })}>Edit</TextButton>
          ) : undefined
        }
      >
        {sheet?.kind === 'add' && (
          <ItemForm
            members={members}
            meId={me.id}
            onSave={(input) => save.mutateAsync({ input })}
            onCancel={closeSheet}
          />
        )}
        {sheet?.kind === 'view' && selected && (
          <ItemDetail
            item={selected}
            canManage={canManage}
            onSell={() => setSheet({ kind: 'sell', id: selected.id })}
            onDispose={() => setSheet({ kind: 'dispose', id: selected.id })}
            onDelete={() => setSheet({ kind: 'delete', id: selected.id })}
          />
        )}
        {sheet?.kind === 'edit' && selected && (
          <ItemForm
            members={members}
            meId={me.id}
            item={selected}
            onSave={(input) => save.mutateAsync({ id: selected.id, input })}
            onCancel={closeSheet}
          />
        )}
        {sheet?.kind === 'sell' && selected && (
          <SellOrDisposeForm
            kind="sell"
            item={selected}
            members={members}
            meId={me.id}
            onSave={(input) => sell.mutateAsync({ id: selected.id, input })}
            onCancel={closeSheet}
          />
        )}
        {sheet?.kind === 'dispose' && selected && (
          <SellOrDisposeForm
            kind="dispose"
            item={selected}
            members={members}
            meId={me.id}
            onSave={(input) => dispose.mutateAsync({ id: selected.id, input })}
            onCancel={closeSheet}
          />
        )}
        {sheet?.kind === 'delete' && selected && (
          <>
            <Note>{selected.name} is removed for everyone, and its money comes out of everyone's balances.</Note>
            <ErrorMessage error={remove.error} />
            <SheetActions>
              <Button variant="danger" block disabled={remove.isPending} onClick={() => remove.mutate(selected.id)}>
                Delete
              </Button>
              <Button variant="plain" block onClick={closeSheet}>
                Cancel
              </Button>
            </SheetActions>
          </>
        )}
      </Sheet>
    </>
  )
}
