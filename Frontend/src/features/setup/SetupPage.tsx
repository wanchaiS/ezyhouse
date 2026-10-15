import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { keys } from '../../shared/api/keys'
import { logout, useMe } from '../../shared/api/me'
import { Button } from '../../shared/components/Button'
import { Card } from '../../shared/components/Card'
import { PageHeader } from '../../shared/components/PageHeader'
import { Note } from '../../shared/components/Text'
import { createHousehold, joinHousehold } from './api'
import { HouseholdForm } from './HouseholdForm'
import styles from './Setup.module.css'

export function SetupPage() {
  const me = useMe()
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  const onSuccess = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: keys.me }),
      queryClient.invalidateQueries({ queryKey: keys.household }),
    ])
    navigate('/', { replace: true })
  }
  const create = useMutation({ mutationFn: createHousehold, onSuccess })
  const join = useMutation({ mutationFn: joinHousehold, onSuccess })

  return (
    // /setup sits outside the app Layout, so it brings its own page width and padding.
    <main className={styles.page}>
      <PageHeader title={`Welcome, ${me.data?.name ?? ''}`} />
      <p className={styles.lead}>You're not in a household yet.</p>
      <Card title="Join your housemates">
        <HouseholdForm
          field="inviteCode"
          label="Invite code"
          placeholder="e.g. ELM-4821"
          submitLabel="Join household"
          primary
          onSubmit={join.mutateAsync}
        />
      </Card>
      <div className={styles.or}>or</div>
      <Card title="Start a new household">
        <HouseholdForm
          field="name"
          label="Household name"
          placeholder="e.g. Elm Street House"
          submitLabel="Create household"
          onSubmit={create.mutateAsync}
        />
        <Note>You'll be the primary: you manage rent, bills and bond.</Note>
      </Card>
      <div className={styles.stackActions}>
        <Button variant="plain" block onClick={() => void logout()}>
          Log out
        </Button>
      </div>
    </main>
  )
}
