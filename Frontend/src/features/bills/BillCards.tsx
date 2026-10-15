import { useState } from 'react'
import type { BillType, Expense, UserRef } from '../../shared/api/types'
import { Button } from '../../shared/components/Button'
import { BillIcon } from '../../shared/components/icons'
import { Money } from '../../shared/components/Money'
import { formatShortDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import styles from './Bills.module.css'
import { FREQUENCY_LABELS } from './types'

interface BillCardsProps {
  /** Every bills expense, newest first. */
  bills: Expense[]
  billTypes: BillType[]
  meId: number
  isPrimary: boolean
  primary: UserRef
  /** Opens the form to enter a $0 bill's amount. */
  onEnter: (bill: Expense) => void
  onOpenBill: (bill: Expense) => void
  onOpenType: (billType: BillType) => void
}

interface Group {
  key: string
  name: string
  /** Missing once the bill type has been deleted: its bills stay, but nothing new comes. */
  type?: BillType
  bills: Expense[]
}

const SHOWN = 3

// Up to three names read naturally; beyond that a count is easier to take in.
const who = (names: string[]) =>
  names.length > 3
    ? `${names.length} people`
    : names.length <= 2
      ? names.join(' and ')
      : `${names.slice(0, -1).join(', ')} and ${names.at(-1)}`

/** One card per repeating bill, with the bills it has created as a timeline, newest first. */
export function BillCards({ bills, billTypes, meId, isPrimary, primary, onEnter, onOpenBill, onOpenType }: BillCardsProps) {
  const [expanded, setExpanded] = useState<Record<string, boolean>>({})

  const typeIds = new Set(billTypes.map((type) => type.id))
  const groups: Group[] = billTypes.map((type) => ({
    key: `t${type.id}`,
    name: type.name,
    type,
    bills: bills.filter((bill) => bill.billTypeId === type.id),
  }))
  // Bills whose bill type was deleted (or that never had one) keep their history, grouped by name.
  const leftOver = new Map<string, Group>()
  for (const bill of bills) {
    if (bill.billTypeId !== null && typeIds.has(bill.billTypeId)) continue
    const group = leftOver.get(bill.description) ?? { key: `o${bill.description}`, name: bill.description, bills: [] }
    group.bills.push(bill)
    leftOver.set(bill.description, group)
  }
  groups.push(...leftOver.values())

  return (
    <div className={styles.billCards}>
      {groups.map((group) => {
        const members = group.type?.members ?? []
        const inSplit = group.type
          ? members.some((member) => member.id === meId)
          : group.bills.some((bill) => bill.shares.some((share) => share.user.id === meId))
        const memberNames = members.map((member) => (member.id === meId ? 'you' : member.name))
        const open = expanded[group.key]
        return (
          <section key={group.key} className={`${styles.billCard} ${inSplit ? '' : styles.notMine}`}>
            <button
              type="button"
              className={styles.billHead}
              onClick={() => group.type && onOpenType(group.type)}
              disabled={!group.type}
            >
              <span className={styles.iconCircle}>
                <BillIcon />
              </span>
              <span className={styles.itemMain}>
                <span className={styles.billName}>{group.name}</span>
                <span className={styles.itemSub}>
                  {!group.type
                    ? 'No longer repeating'
                    : inSplit
                      ? `${FREQUENCY_LABELS[group.type.frequency]} · split between ${who(memberNames)}`
                      : `${FREQUENCY_LABELS[group.type.frequency]} · ${who(memberNames)} · you’re not in this one`}
                </span>
              </span>
              {group.type?.nextDueDate && (
                <span className={styles.next}>Next {formatShortDate(group.type.nextDueDate)}</span>
              )}
            </button>
            {group.bills.length === 0 && (
              <p className={styles.nothing}>
                {group.type?.nextDueDate
                  ? `The first one appears on ${formatShortDate(group.type.nextDueDate)}.`
                  : 'No bills were created.'}
              </p>
            )}
            {group.bills.length > 0 && (
              <ol className={styles.timeline}>
                {group.bills.slice(0, open ? undefined : SHOWN).map((bill) => (
                  <BillEvent
                    key={bill.id}
                    bill={bill}
                    meId={meId}
                    isPrimary={isPrimary}
                    primary={primary}
                    onEnter={onEnter}
                    onOpenBill={onOpenBill}
                  />
                ))}
              </ol>
            )}
            {group.bills.length > SHOWN && (
              <button
                type="button"
                className={styles.more}
                onClick={() => setExpanded({ ...expanded, [group.key]: !open })}
              >
                {open ? 'Show fewer' : `Show ${group.bills.length - SHOWN} earlier`}
              </button>
            )}
          </section>
        )
      })}
    </div>
  )
}

interface BillEventProps {
  bill: Expense
  meId: number
  isPrimary: boolean
  primary: UserRef
  onEnter: (bill: Expense) => void
  onOpenBill: (bill: Expense) => void
}

/** One bill on the timeline: its date, then what it means for the viewer. */
function BillEvent({ bill, meId, isPrimary, primary, onEnter, onOpenBill }: BillEventProps) {
  const mine = bill.shares.find((share) => share.user.id === meId)
  const date = <span className={styles.eventDate}>{formatShortDate(bill.date)}</span>

  // A $0 bill is waiting for the primary to enter what the provider charged.
  if (bill.amount === 0) {
    if (!isPrimary) {
      return (
        <li className={mine ? styles.dotAmount : styles.dotNone}>
          <div className={styles.event}>
            {date}
            <span className={styles.status}>{mine ? `${primary.name} hasn’t entered it yet` : 'Not in your split'}</span>
          </div>
        </li>
      )
    }
    return (
      <li className={styles.dotAmount}>
        <div className={styles.eventLine}>
          <button type="button" className={styles.event} onClick={() => onOpenBill(bill)}>
            {date}
            <span className={`${styles.pill} ${styles.pillWarn}`}>Amount needed</span>
          </button>
          <Button small variant="primary" onClick={() => onEnter(bill)}>
            Enter amount
          </Button>
        </div>
      </li>
    )
  }

  return (
    <li className={mine ? styles.dotEntered : styles.dotNone}>
      <button type="button" className={styles.event} onClick={() => onOpenBill(bill)}>
        {date}
        <span className={styles.total}>{formatMoney(bill.amount)} bill</span>
        <span className={styles.eventEnd}>
          {mine ? (
            <>
              <span className={styles.endNote}>your share </span>
              <Money amount={mine.amount} />
            </>
          ) : (
            <span className={styles.notInSplit}>not in your split</span>
          )}
        </span>
      </button>
    </li>
  )
}
