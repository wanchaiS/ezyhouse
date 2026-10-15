import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { getBillTypes } from '../../shared/api/billTypes'
import { getExpenses } from '../../shared/api/expenses'
import { useHousehold } from '../../shared/api/household'
import { keys } from '../../shared/api/keys'
import { useMe } from '../../shared/api/me'
import { getRentSchedules } from '../../shared/api/rent'
import type { ExpenseTag } from '../../shared/api/types'
import { ErrorMessage } from '../../shared/components/ErrorMessage'
import { ExpenseSheet, type ExpenseSheetState } from '../../shared/components/ExpenseSheet'
import { ChevronIcon } from '../../shared/components/icons'
import { PageHeader } from '../../shared/components/PageHeader'
import { Spinner } from '../../shared/components/Spinner'
import { Note, SectionTitle } from '../../shared/components/Text'
import { getBalances, getSettlements } from './api'
import { BillsToEnter } from './BillsToEnter'
import styles from './Home.module.css'
import { RecentList, type Activity } from './RecentList'
import { SettleUpCard } from './SettleUpCard'
import { SettleUpSheet } from './SettleUpSheet'
import { TAG_ORDER } from './tags'
import { TrackTiles } from './TrackTiles'
import type { Debt } from './types'

const RECENT = 6

export function DashboardPage() {
  const me = useMe().data
  const household = useHousehold()
  const balances = useQuery({ queryKey: keys.balances, queryFn: getBalances })
  const expenses = useQuery({ queryKey: keys.expenseList(), queryFn: () => getExpenses() })
  const settlements = useQuery({ queryKey: keys.settlements, queryFn: getSettlements })
  const schedules = useQuery({ queryKey: keys.rentSchedules, queryFn: getRentSchedules })
  const billTypes = useQuery({ queryKey: keys.billTypes, queryFn: getBillTypes })
  const [settling, setSettling] = useState<Debt | null>(null)
  const [expenseSheet, setExpenseSheet] = useState<ExpenseSheetState>(null)

  const failed = [household, balances, expenses, settlements, schedules, billTypes].find((query) => query.isError)
  if (failed) return <ErrorMessage error={failed.error} />
  if (!me || !household.data || !balances.data || !expenses.data || !settlements.data || !schedules.data || !billTypes.data) {
    return <Spinner />
  }

  const { primary, members, inviteCode } = household.data
  const isPrimary = primary.id === me.id
  const alone = members.length === 1

  const toEnter = expenses.data
    .filter((expense) => expense.tag === 'bills' && expense.amount === 0)
    .sort((a, b) => a.date.localeCompare(b.date))

  // What was spent and what was paid back, newest first; on the same day a payment comes first.
  const recent: Activity[] = [
    ...expenses.data.map((expense) => ({ kind: 'expense' as const, date: expense.date, id: expense.id, expense })),
    ...settlements.data.map((settlement) => ({ kind: 'payment' as const, date: settlement.date, id: settlement.id, settlement })),
  ]
    .sort((a, b) => b.date.localeCompare(a.date) || (a.kind === b.kind ? b.id - a.id : a.kind === 'payment' ? -1 : 1))
    .slice(0, RECENT)

  const used = new Set(expenses.data.map((expense) => expense.tag))
  const inUse: Record<ExpenseTag, boolean> = {
    rent: schedules.data.length > 0,
    bills: billTypes.data.length > 0,
    groceries: used.has('groceries'),
    furniture: used.has('furniture'),
  }
  // Rent, bills and furniture are the primary's to start; groceries are anyone's.
  const addable = TAG_ORDER.filter((tag) => !inUse[tag] && (isPrimary || tag === 'groceries'))
  const nothingYet = !TAG_ORDER.some((tag) => inUse[tag]) && !expenses.data.length

  return (
    <>
      <PageHeader title={`Hi, ${me.name}`} />
      <div className={styles.home}>
        {alone && (
          <Link className={styles.alone} to="/household">
            <span>
              <b>You’re the only one here.</b> Share the code {inviteCode} so housemates can join.
            </span>
            <ChevronIcon />
          </Link>
        )}
        {!alone && <SettleUpCard debts={balances.data.debts} meId={me.id} onSettle={setSettling} />}
        {isPrimary && toEnter.length > 0 && (
          <div>
            <SectionTitle>Bills to enter</SectionTitle>
            <BillsToEnter bills={toEnter} onEnter={(bill) => setExpenseSheet({ kind: 'edit', expense: bill })} />
          </div>
        )}
        {recent.length > 0 && (
          <div>
            <SectionTitle>Recent</SectionTitle>
            <RecentList items={recent} meId={me.id} />
          </div>
        )}
        {(addable.length > 0 || (!isPrimary && (!inUse.rent || !inUse.bills))) && (
          <div>
            <SectionTitle>{nothingYet ? 'What does your house share?' : 'Track something else'}</SectionTitle>
            {addable.length > 0 && <TrackTiles tags={addable} />}
            {!isPrimary && (!inUse.rent || !inUse.bills) && (
              <Note>{primary.name} sets up rent and bills. They’ll show here once added.</Note>
            )}
          </div>
        )}
      </div>

      <SettleUpSheet
        debt={settling}
        summary={
          settling === null
            ? ''
            : settling.from.id === me.id
              ? `You paid ${settling.to.name} back`
              : `${settling.from.name} paid you back`
        }
        onClose={() => setSettling(null)}
      />
      <ExpenseSheet
        state={expenseSheet}
        onChange={setExpenseSheet}
        tag="bills"
        members={members}
        meId={me.id}
        primaryName={primary.name}
      />
    </>
  )
}
