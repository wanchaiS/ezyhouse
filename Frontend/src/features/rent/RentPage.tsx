import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import { getRentPeriods } from '../../shared/api/rent'
import { IconButton } from '../../shared/components/Button'
import { EmptyState } from '../../shared/components/EmptyState'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { PlusIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Spinner } from '../../shared/components/Spinner'
import { Note, SectionTitle } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import { today } from '../../shared/format/date'
import {
  createRentSchedule,
  deleteRentSchedule,
  getRentSchedules,
  rentScheduleKey,
  setRentSharePaid,
  updateRentSchedule,
} from './api'
import { CurrentPeriod } from './CurrentPeriod'
import { DeleteScheduleSheet } from './DeleteScheduleSheet'
import { EarlierPeriods } from './EarlierPeriods'
import { PeriodSheet } from './PeriodSheet'
import { ScheduleFormSheet } from './ScheduleFormSheet'
import { ScheduleList } from './ScheduleList'
import { ScheduleSheet } from './ScheduleSheet'
import type { RentSchedule, RentScheduleInput } from './types'
import styles from './Rent.module.css'

// Which sheet is open; ids rather than objects, so a sheet shows the latest data after a refetch.
type OpenSheet =
  | { kind: 'period'; id: number }
  | { kind: 'schedule'; id: number }
  | { kind: 'form'; id: number | null }
  | { kind: 'delete'; id: number }
  | null

export function RentPage() {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const me = useMe()
  const household = useHousehold()
  const periods = useQuery({ queryKey: keys.rentPeriods, queryFn: getRentPeriods })
  const schedules = useQuery({ queryKey: rentScheduleKey, queryFn: getRentSchedules })
  const [sheet, setSheet] = useState<OpenSheet>(null)

  // Creating, editing or deleting a schedule can add or remove periods too.
  const refreshRent = () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: rentScheduleKey }),
      queryClient.invalidateQueries({ queryKey: keys.rentPeriods }),
    ])

  const tick = useMutation({
    mutationFn: ({ periodId, userId, paid }: { periodId: number; userId: number; paid: boolean }) =>
      setRentSharePaid(periodId, userId, paid),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: keys.rentPeriods }),
  })

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

  if (me.isPending || household.isPending || periods.isPending || schedules.isPending) return <Spinner />
  const loadError = me.error ?? household.error ?? periods.error ?? schedules.error
  if (loadError || !me.data || !household.data || !periods.data || !schedules.data) {
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
  const now = today()
  const sorted = [...periods.data].sort((a, b) => b.startDate.localeCompare(a.startDate))
  const current = sorted.filter((period) => period.startDate <= now && now <= period.endDate)
  const earlier = sorted.filter((period) => !current.includes(period))

  const findPeriod = (id: number) => periods.data.find((period) => period.id === id) ?? null
  const findSchedule = (id: number | null) => schedules.data.find((schedule) => schedule.id === id) ?? null
  const shareProps = { meId, isPrimary, busy: tick.isPending }
  const tickIn = (periodId: number) => (userId: number, paid: boolean) => tick.mutate({ periodId, userId, paid })
  const closeSheet = () => setSheet(null)

  const emptyText = schedules.data.length
    ? 'No rent due right now.'
    : isPrimary
      ? 'No rent set up yet. Tap + to set it up.'
      : 'No rent set up yet.'

  return (
    <>
      <PageHeader
        title="Rent"
        action={
          isPrimary && (
            <IconButton label="Set up rent" accent onClick={() => setSheet({ kind: 'form', id: null })}>
              <PlusIcon />
            </IconButton>
          )
        }
      />
      {sheet === null && <ErrorMessage error={tick.error} />}
      <div className={styles.columns}>
        <div>
          <SectionTitle>This period</SectionTitle>
          {current.length ? (
            <div className={styles.stack}>
              {current.map((period) => (
                <CurrentPeriod key={period.id} period={period} {...shareProps} onTick={tickIn(period.id)} />
              ))}
            </div>
          ) : (
            <EmptyState>{emptyText}</EmptyState>
          )}
          {earlier.length > 0 && (
            <>
              <SectionTitle>Earlier</SectionTitle>
              <EarlierPeriods
                periods={earlier}
                meId={meId}
                onOpen={(period) => {
                  tick.reset()
                  setSheet({ kind: 'period', id: period.id })
                }}
              />
            </>
          )}
        </div>
        <div>
          <SectionTitle>Schedule</SectionTitle>
          {schedules.data.length ? (
            <ScheduleList schedules={schedules.data} onOpen={(schedule) => setSheet({ kind: 'schedule', id: schedule.id })} />
          ) : (
            <EmptyState>No rent schedule yet.</EmptyState>
          )}
          <Note>
            A new period appears each time rent is due. Pay the person shown on the period (whoever was primary when
            it was created), then tick your share. Editing a schedule only changes periods created after the edit. Rent
            is not part of balances.
          </Note>
        </div>
      </div>

      <PeriodSheet
        period={sheet?.kind === 'period' ? findPeriod(sheet.id) : null}
        onClose={closeSheet}
        {...shareProps}
        tickError={tick.error}
        onTick={tickIn(sheet?.kind === 'period' ? sheet.id : 0)}
      />
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
    </>
  )
}
