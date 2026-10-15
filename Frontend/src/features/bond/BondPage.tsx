import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { getBonds } from '../../shared/api/bonds'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import type { Bond } from '../../shared/api/types'
import { IconButton } from '../../shared/components/Button'
import { EmptyState } from '../../shared/components/EmptyState'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { PlusIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Spinner } from '../../shared/components/Spinner'
import { Note } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import { createBond, deleteBond, updateBond } from './api'
import { BondCard } from './BondCard'
import { BondFormSheet } from './BondFormSheet'
import { DeleteBondSheet } from './DeleteBondSheet'
import type { BondInput } from './types'
import styles from './Bond.module.css'

// Ids rather than objects, so a sheet shows the latest data after a refetch.
type OpenSheet = { kind: 'form'; id: number | null } | { kind: 'delete'; id: number } | null

export function BondPage() {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const me = useMe()
  const household = useHousehold()
  const bonds = useQuery({ queryKey: keys.bonds, queryFn: getBonds })
  const [sheet, setSheet] = useState<OpenSheet>(null)

  // The bond is a record only, so no other key (balances etc.) is affected.
  const refreshBonds = () => queryClient.invalidateQueries({ queryKey: keys.bonds })

  const save = useMutation({
    mutationFn: ({ id, input }: { id: number | null; input: BondInput }) =>
      id === null ? createBond(input) : updateBond(id, input),
    onSuccess: (_bond, { id }) => {
      setSheet(null)
      showToast(id === null ? 'Bond added' : 'Bond saved')
      return refreshBonds()
    },
  })

  const remove = useMutation({
    mutationFn: (bond: Bond) => deleteBond(bond.id),
    onSuccess: () => {
      setSheet(null)
      showToast('Bond deleted')
      return refreshBonds()
    },
  })

  if (me.isPending || household.isPending || bonds.isPending) return <Spinner />
  const loadError = me.error ?? household.error ?? bonds.error
  if (loadError || !me.data || !household.data || !bonds.data) {
    return (
      <>
        <PageHeader title="Bond" backTo="/household" />
        <ErrorMessage error={loadError} message={loadError ? undefined : 'Could not load the bond.'} />
      </>
    )
  }

  const { primary, members } = household.data
  const isPrimary = primary.id === me.data.id
  const sorted = [...bonds.data].sort((a, b) => b.paidDate.localeCompare(a.paidDate))
  const findBond = (id: number | null) => bonds.data.find((bond) => bond.id === id) ?? null
  const closeSheet = () => setSheet(null)

  return (
    <>
      <PageHeader
        title="Bond"
        backTo="/household"
        action={
          isPrimary && (
            <IconButton label="Add bond" accent onClick={() => setSheet({ kind: 'form', id: null })}>
              <PlusIcon />
            </IconButton>
          )
        }
      />
      {sorted.length ? (
        <div className={styles.grid}>
          {sorted.map((bond) => (
            <BondCard
              key={bond.id}
              bond={bond}
              onEdit={isPrimary ? (b) => setSheet({ kind: 'form', id: b.id }) : undefined}
            />
          ))}
        </div>
      ) : (
        <EmptyState>{isPrimary ? 'No bond recorded. Tap + to add one.' : 'No bond recorded.'}</EmptyState>
      )}
      <Note>
        A record only of the bond and who put in what. The bond never affects anyone's balance.
        {isPrimary ? '' : ` Only ${primary.name} (primary) can change it.`}
      </Note>

      <BondFormSheet
        open={sheet?.kind === 'form'}
        onClose={closeSheet}
        bond={sheet?.kind === 'form' ? findBond(sheet.id) : null}
        members={members}
        onSave={(input) => save.mutateAsync({ id: sheet?.kind === 'form' ? sheet.id : null, input })}
        onDelete={(bond) => {
          remove.reset()
          setSheet({ kind: 'delete', id: bond.id })
        }}
      />
      <DeleteBondSheet
        bond={sheet?.kind === 'delete' ? findBond(sheet.id) : null}
        onClose={closeSheet}
        onConfirm={(bond) => remove.mutate(bond)}
        deleting={remove.isPending}
        error={remove.error}
      />
    </>
  )
}
