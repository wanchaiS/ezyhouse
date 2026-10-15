import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { getBonds } from '../../shared/api/bonds'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { logout, useMe } from '../../shared/api/me'
import { problemMessage, toProblem } from '../../shared/api/problem'
import type { User, UserRef } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { Field } from '../../shared/components/Field'
import { List } from '../../shared/components/List'
import { MemberSelect } from '../../shared/components/MemberSelect'
import { PageHeader } from '../../shared/components/PageHeader'
import { Sheet, SheetActions } from '../../shared/components/Sheet'
import { Spinner } from '../../shared/components/Spinner'
import { Note, SectionTitle } from '../../shared/components/Text'
import { useToast } from '../../shared/components/toastContext'
import { leaveHousehold, setPrimary } from './api'
import { BondSummary } from './BondSummary'
import styles from './Household.module.css'
import { InviteCodeCard } from './InviteCodeCard'
import { MemberList } from './MemberList'

type SheetState =
  | { kind: 'handOver'; member: UserRef }
  /** `nextPrimaryId`: who takes over when the primary leaves and others stay. */
  | { kind: 'leave'; nextPrimaryId: number | null }
  | { kind: 'blocked'; reasons: string[] }
  | null

export function HouseholdPage() {
  const queryClient = useQueryClient()
  const showToast = useToast()
  const [sheet, setSheet] = useState<SheetState>(null)

  const me = useMe()
  const household = useHousehold()
  const bonds = useQuery({ queryKey: keys.bonds, queryFn: getBonds })

  const handOver = useMutation({
    mutationFn: (member: UserRef) => setPrimary(member.id),
    onSuccess: (updated, member) => {
      queryClient.setQueryData(keys.household, updated)
      // Who is primary decides who may change rent, bills and furniture, and who pays new rent and bills.
      queryClient.invalidateQueries({ queryKey: keys.household })
      queryClient.invalidateQueries({ queryKey: keys.expenses })
      setSheet(null)
      showToast(`${member.name} is now the primary`)
    },
  })

  const leave = useMutation({
    mutationFn: leaveHousehold,
    onSuccess: () => {
      // Out of the household at once, so the guards send us to /setup without asking for
      // this household's data again; then nothing from it is kept for the next one.
      queryClient.setQueryData<User | null>(keys.me, (user) => user && { ...user, householdId: null })
      queryClient.removeQueries({ predicate: (query) => query.queryKey[0] !== keys.me[0] })
      queryClient.invalidateQueries({ queryKey: keys.me })
    },
    onError: (error) => {
      if (toProblem(error).status === 409) {
        setSheet({ kind: 'blocked', reasons: problemMessage(error).split('\n') })
      }
    },
  })

  const signOut = useMutation({ mutationFn: logout })

  if (me.isPending || household.isPending) return <Spinner />
  const loadError = me.error ?? household.error
  if (loadError || !me.data || !household.data) {
    return (
      <>
        <PageHeader title="Household" backTo="/" />
        <ErrorMessage error={loadError} message={loadError ? undefined : 'Could not load the household.'} />
      </>
    )
  }

  const meId = me.data.id
  const { name, inviteCode, primary, members } = household.data
  const isPrimary = primary.id === meId
  const others = members.filter((member) => member.id !== meId)
  // A primary who leaves others behind names who takes over (the API answers 400 otherwise).
  const needsNextPrimary = isPrimary && others.length > 0

  const copyCode = async () => {
    try {
      await navigator.clipboard.writeText(inviteCode)
      showToast(`Copied ${inviteCode}`)
    } catch {
      showToast(`Could not copy. The code is ${inviteCode}`)
    }
  }

  const closeSheet = () => {
    setSheet(null)
    handOver.reset()
    leave.reset()
  }

  return (
    <>
      <PageHeader title={name} backTo="/" />
      <div className={styles.columns}>
        <div>
          <InviteCodeCard code={inviteCode} onCopy={copyCode} />
          <SectionTitle>Members</SectionTitle>
          <MemberList
            members={members}
            primaryId={primary.id}
            meId={meId}
            isPrimary={isPrimary}
            onPick={(member) => {
              handOver.reset()
              setSheet({ kind: 'handOver', member })
            }}
          />
        </div>
        <div>
          <SectionTitle>Bond</SectionTitle>
          {bonds.isPending ? <Spinner /> : bonds.isError ? <ErrorMessage error={bonds.error} /> : <BondSummary bonds={bonds.data} />}
          <div className={styles.stackActions}>
            <Button
              variant="danger"
              block
              onClick={() => {
                leave.reset()
                setSheet({ kind: 'leave', nextPrimaryId: null })
              }}
            >
              Leave household
            </Button>
            <Button variant="plain" block disabled={signOut.isPending} onClick={() => signOut.mutate()}>
              Log out
            </Button>
          </div>
          <ErrorMessage error={signOut.error} />
        </div>
      </div>

      <Sheet
        open={sheet?.kind === 'handOver'}
        onClose={closeSheet}
        title={sheet?.kind === 'handOver' ? `Make ${sheet.member.name} the primary?` : ''}
      >
        <Note>
          They will manage rent, bills, furniture and bond. Rent and bills created from now on are paid by them. You
          become a normal member.
        </Note>
        <ErrorMessage error={handOver.error} />
        <SheetActions>
          <Button
            variant="primary"
            block
            disabled={handOver.isPending}
            onClick={() => sheet?.kind === 'handOver' && handOver.mutate(sheet.member)}
          >
            Make primary
          </Button>
          <Button variant="plain" block onClick={closeSheet}>
            Cancel
          </Button>
        </SheetActions>
      </Sheet>

      <Sheet open={sheet?.kind === 'leave'} onClose={closeSheet} title={`Leave ${name}?`}>
        <Note>You will be taken off the rent and every bill.</Note>
        {sheet?.kind === 'leave' && needsNextPrimary && (
          <Field label="Who becomes the primary?">
            <MemberSelect
              members={others}
              value={sheet.nextPrimaryId}
              onChange={(userId) => setSheet({ kind: 'leave', nextPrimaryId: userId })}
            />
          </Field>
        )}
        {toProblem(leave.error).status !== 409 && <ErrorMessage error={leave.error} />}
        <SheetActions>
          <Button
            variant="danger"
            block
            disabled={leave.isPending || (needsNextPrimary && sheet?.kind === 'leave' && sheet.nextPrimaryId === null)}
            onClick={() =>
              sheet?.kind === 'leave' &&
              leave.mutate(sheet.nextPrimaryId === null ? {} : { nextPrimaryUserId: sheet.nextPrimaryId })
            }
          >
            Leave household
          </Button>
          <Button variant="plain" block onClick={closeSheet}>
            Cancel
          </Button>
        </SheetActions>
      </Sheet>

      <Sheet open={sheet?.kind === 'blocked'} onClose={closeSheet} title="You can't leave yet">
        <List>
          {sheet?.kind === 'blocked' &&
            sheet.reasons.map((reason) => (
              <div key={reason} className={styles.reason}>
                {reason}
              </div>
            ))}
        </List>
        <SheetActions>
          <Button variant="primary" block onClick={closeSheet}>
            OK
          </Button>
        </SheetActions>
      </Sheet>
    </>
  )
}
