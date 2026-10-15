import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { getBalances } from '../../shared/api/balances'
import { getBills } from '../../shared/api/bills'
import { getFurniture } from '../../shared/api/furniture'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import { getRentPeriods } from '../../shared/api/rent'
import { CardGrid } from '../../shared/components/Card'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { PageHeader } from '../../shared/components/PageHeader'
import { SettleUpSheet } from '../../shared/components/SettleUpSheet'
import { Spinner } from '../../shared/components/Spinner'
import { BillsCard } from './components/BillsCard'
import { FurnitureCard } from './components/FurnitureCard'
import { NetCard } from './components/NetCard'
import { RentCard } from './components/RentCard'

export function DashboardPage() {
  const me = useMe().data
  const household = useHousehold()
  const balances = useQuery({ queryKey: keys.balances, queryFn: getBalances })
  const rent = useQuery({ queryKey: keys.rentPeriods, queryFn: getRentPeriods })
  const bills = useQuery({ queryKey: keys.billList(), queryFn: () => getBills() })
  const furniture = useQuery({ queryKey: keys.furnitureList('owned'), queryFn: () => getFurniture('owned') })
  const [settling, setSettling] = useState(false)

  const queries = [household, balances, rent, bills, furniture]
  const error = queries.find((q) => q.error)?.error
  if (!me || !household.data || !balances.data || !rent.data || !bills.data || !furniture.data) {
    return error ? <ErrorMessage error={error} /> : <Spinner />
  }

  const net = balances.data.members.find((m) => m.user.id === me.id)?.net ?? 0
  // Prefill the sheet with the largest debt I owe, if any.
  const myDebt = balances.data.debts.find((d) => d.from.id === me.id)

  return (
    <>
      <PageHeader eyebrow={household.data.name} title={`Hi, ${me.name}`} />
      <CardGrid>
        <NetCard me={me} net={net} debts={balances.data.debts} onSettle={() => setSettling(true)} />
        <RentCard meId={me.id} periods={rent.data} />
        <BillsCard meId={me.id} bills={bills.data} />
        <FurnitureCard ownedCount={furniture.data.length} />
      </CardGrid>
      <SettleUpSheet
        open={settling}
        onClose={() => setSettling(false)}
        members={household.data.members}
        fromUserId={me.id}
        toUserId={myDebt?.to.id}
        amount={myDebt?.amount}
      />
    </>
  )
}
