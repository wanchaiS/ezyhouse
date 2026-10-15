import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { getExpenses } from '../../shared/api/expenses'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import { getRentSchedules } from '../../shared/api/rent'
import type { RentSchedule } from '../../shared/api/types'
import { Button, IconButton } from '../../shared/components/Button'
import { EmptyState } from '../../shared/components/EmptyState'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { ExpenseLog } from '../../shared/components/ExpenseLog'
import { ExpenseSheet, type ExpenseSheetState } from '../../shared/components/ExpenseSheet'
import { FirstRun } from '../../shared/components/FirstRun'
import { KeyIcon, PlusIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Spinner } from '../../shared/components/Spinner'
import { Lead, SectionTitle } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import { formatShortDate } from '../../shared/format/date'
import { createRentSchedule, deleteRentSchedule, updateRentSchedule } from './api'
import { DeleteScheduleSheet } from './DeleteScheduleSheet'
import { ScheduleFormSheet } from './ScheduleFormSheet'
import { ScheduleList } from './ScheduleList'
import { ScheduleSheet } from './ScheduleSheet'
import type { RentScheduleInput } from './types'
import styles from './Rent.module.css'

// Which schedule sheet is open; ids rather than objects, so a sheet shows the latest data after a refetch.
type OpenSheet =
  | { kind: 'schedule'; id: number }
  | { kind: 'form'; id: number | null }
  | { kind: 'delete'; id: number }
  | null

export function RentPage() {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const me = useMe()
  const household = useHousehold()
  const schedules = useQuery({ queryKey: keys.rentSchedules, queryFn: getRentSchedules })
  const rent = useQuery({ queryKey: keys.expenseList('rent'), queryFn: () => getExpenses('rent') })
  const [sheet, setSheet] = useState<OpenSheet>(null)
  const [expenseSheet, setExpenseSheet] = useState<ExpenseSheetState>(null)

  // A new schedule due today creates its rent straight away, and a deleted one unlinks its rent,
  // so the rent and the balances refresh with the schedules.
  const refreshRent = () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: keys.rentSchedules }),
      queryClient.invalidateQueries({ queryKey: keys.expenses }),
      queryClient.invalidateQueries({ queryKey: keys.balances }),
    ])

  const save = useMutation({
    mutationFn: ({ id, input }: { id: number | null; input: RentScheduleInput }) =>
      id === null ? createRentSchedule(input) : updateRentSchedule(id, input),
    onSuccess: (_schedule, { id }) => {
      setSheet(null)
      showToast(id === null ? 'Rent set up' : 'Rent saved')
      return refreshRent()
    },
  })

  const remove = useMutation({
    mutationFn: (schedule: RentSchedule) => deleteRentSchedule(schedule.id),
    onSuccess: () => {
      setSheet(null)
      showToast('Rent deleted')
      return refreshRent()
    },
  })

  if (me.isPending || household.isPending || schedules.isPending || rent.isPending) return <Spinner />
  const loadError = me.error ?? household.error ?? schedules.error ?? rent.error
  if (loadError || !me.data || !household.data || !schedules.data || !rent.data) {
    return (
      <>
        <PageHeader title="Rent" />
        <ErrorMessage error={loadError} message={loadError ? undefined : 'Could not load the rent.'} />
      </>
    )
  }

  const meId = me.data.id
  const { primary, members } = household.data
  const isPrimary = primary.id === meId
  const findSchedule = (id: number | null) => schedules.data.find((schedule) => schedule.id === id) ?? null
  const closeSheet = () => setSheet(null)
  const setUp = () => setSheet({ kind: 'form', id: null })
  const nextDue = schedules.data
    .map((schedule) => schedule.nextDueDate)
    .filter((date) => date !== null)
    .sort()[0]

  return (
    <>
      <PageHeader
        title="Rent"
        action={
          isPrimary && (
            <IconButton label="Set up rent" accent onClick={setUp}>
              <PlusIcon />
            </IconButton>
          )
        }
      />
      <div className={styles.page}>
        {!schedules.data.length && !rent.data.length ? (
          <FirstRun
            icon={<KeyIcon />}
            title="No rent yet"
            action={
              isPrimary && (
                <Button variant="primary" onClick={setUp}>
                  Set up rent
                </Button>
              )
            }
          >
            {isPrimary
              ? 'Set up the rent once: the amount, how often and who pays what. Each period’s rent then appears here when it’s due, paid by you.'
              : `${primary.name} sets up the rent. Each period shows here once it’s due.`}
          </FirstRun>
        ) : (
          <>
            <Lead>
              {isPrimary
                ? 'Each period’s rent appears here when it’s due, paid by you. Settle up on Home.'
                : `${primary.name} pays the rent. Your share counts in what you owe on Home.`}
            </Lead>
            <SectionTitle>Schedule</SectionTitle>
            {schedules.data.length ? (
              <ScheduleList schedules={schedules.data} onOpen={(schedule) => setSheet({ kind: 'schedule', id: schedule.id })} />
            ) : (
              <EmptyState>No rent schedule. No new rent is created.</EmptyState>
            )}
            {rent.data.length ? (
              <ExpenseLog
                expenses={rent.data}
                meId={meId}
                onOpen={(expense) => setExpenseSheet({ kind: 'view', expense })}
              />
            ) : (
              <>
                <SectionTitle>Rent so far</SectionTitle>
                <EmptyState>
                  {nextDue ? `The first rent appears on ${formatShortDate(nextDue)}.` : 'No rent has been created.'}
                </EmptyState>
              </>
            )}
          </>
        )}
      </div>

      <ScheduleSheet
        schedule={sheet?.kind === 'schedule' ? findSchedule(sheet.id) : null}
        onClose={closeSheet}
        isPrimary={isPrimary}
        primary={primary}
        onEdit={(schedule) => setSheet({ kind: 'form', id: schedule.id })}
        onDelete={(schedule) => {
          remove.reset()
          setSheet({ kind: 'delete', id: schedule.id })
        }}
      />
      <ScheduleFormSheet
        open={sheet?.kind === 'form'}
        onClose={closeSheet}
        schedule={sheet?.kind === 'form' ? findSchedule(sheet.id) : null}
        members={members}
        onSave={(input) => save.mutateAsync({ id: sheet?.kind === 'form' ? sheet.id : null, input })}
      />
      <DeleteScheduleSheet
        schedule={sheet?.kind === 'delete' ? findSchedule(sheet.id) : null}
        onClose={closeSheet}
        onConfirm={(schedule) => remove.mutate(schedule)}
        deleting={remove.isPending}
        error={remove.error}
      />
      <ExpenseSheet
        state={expenseSheet}
        onChange={setExpenseSheet}
        tag="rent"
        members={members}
        meId={meId}
        primaryName={primary.name}
      />
    </>
  )
}
