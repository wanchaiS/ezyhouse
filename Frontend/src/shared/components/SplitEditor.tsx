import { useState } from 'react'
import type { ShareInput, SplitType, UserRef } from '../api/types'
import { formatMoney, parseMoney } from '../format/money'
import { Avatar } from './Avatar'
import { Segmented } from './Segmented'
import styles from './SplitEditor.module.css'

export interface SplitValue {
  splitType: SplitType
  /** The people in the split, in members order; `amount` only when splitType is 'exact'. */
  shares: ShareInput[]
}

interface SplitEditorProps {
  /** Everyone who can be in the split. */
  members: UserRef[]
  /** The total being split; enables the "each" preview and the exact running total. */
  amount?: number
  value: SplitValue
  onChange: (value: SplitValue) => void
}

const SPLIT_OPTIONS = [
  { value: 'equal', label: 'Equally' },
  { value: 'exact', label: 'Exact amounts' },
] as const

const toCents = (amount: number) => Math.round(amount * 100)
const isAmount = (amount: number | undefined): amount is number =>
  amount !== undefined && Number.isFinite(amount)

/** Everyone in `value` starts ticked; pass all members for "everyone, equally". */
export function SplitEditor({ members, amount, value, onChange }: SplitEditorProps) {
  // What was typed in each exact box, so "12." survives while the value holds 12.
  const [drafts, setDrafts] = useState<Record<number, string>>({})
  const exact = value.splitType === 'exact'
  const shareOf = (userId: number) => value.shares.find((share) => share.userId === userId)

  function setShares(next: (userId: number) => ShareInput | undefined) {
    const shares = members.map((member) => next(member.id)).filter((share) => share !== undefined)
    onChange({ ...value, shares })
  }

  function toggle(userId: number) {
    setShares((id) => {
      const share = shareOf(id)
      if (id !== userId) return share
      return share ? undefined : { userId }
    })
  }

  function setType(splitType: SplitType) {
    setDrafts({})
    onChange({ splitType, shares: value.shares.map((share) => ({ userId: share.userId })) })
  }

  function setAmount(userId: number, text: string) {
    setDrafts({ ...drafts, [userId]: text })
    const parsed = parseMoney(text)
    setShares((id) => {
      const share = shareOf(id)
      if (id !== userId || !share) return share
      return { userId, amount: Number.isNaN(parsed) ? undefined : parsed }
    })
  }

  function amountText(share: ShareInput) {
    const draft = drafts[share.userId]
    const draftValue = draft === undefined ? undefined : parseMoney(draft)
    const same = (Number.isNaN(draftValue) && share.amount === undefined) || draftValue === share.amount
    return draft !== undefined && same ? draft : (share.amount?.toString() ?? '')
  }

  // The server gives leftover cents to the first share; the preview does the same.
  const equalShares = new Map<number, number>()
  if (!exact && isAmount(amount) && value.shares.length) {
    const total = toCents(amount)
    const base = Math.floor(total / value.shares.length)
    value.shares.forEach((share, index) => {
      const leftover = index === 0 ? total - base * value.shares.length : 0
      equalShares.set(share.userId, (base + leftover) / 100)
    })
  }

  const exactCents = value.shares.reduce(
    (sum, share) => sum + (isAmount(share.amount) ? toCents(share.amount) : 0),
    0,
  )
  const remainingCents = isAmount(amount) ? toCents(amount) - exactCents : 0

  return (
    <div className={styles.editor}>
      <span className={styles.label}>Split</span>
      <Segmented options={SPLIT_OPTIONS} value={value.splitType} onChange={setType} />
      <div className={styles.people}>
        {members.map((member) => {
          const share = shareOf(member.id)
          const each = equalShares.get(member.id)
          return (
            <div className={styles.person} key={member.id}>
              <label>
                <input type="checkbox" checked={!!share} onChange={() => toggle(member.id)} />
                <Avatar user={member} small />
                <span>{member.name}</span>
              </label>
              {each !== undefined && <span className={styles.each}>{formatMoney(each)}</span>}
              {exact && (
                <input
                  className={styles.amountInput}
                  inputMode="decimal"
                  placeholder="0.00"
                  aria-label={`${member.name}'s amount`}
                  disabled={!share}
                  value={share ? amountText(share) : ''}
                  onChange={(event) => setAmount(member.id, event.target.value)}
                />
              )}
            </div>
          )
        })}
      </div>
      {exact && (
        // Only a warning: the server checks that the shares add up.
        <div className={styles.total}>
          <span>
            Total {formatMoney(exactCents / 100)}
            {isAmount(amount) && ` of ${formatMoney(amount)}`}
          </span>
          {isAmount(amount) && remainingCents !== 0 && (
            <span className={styles.warn}>
              {remainingCents > 0
                ? `${formatMoney(remainingCents / 100)} left to split`
                : `${formatMoney(-remainingCents / 100)} too much`}
            </span>
          )}
        </div>
      )}
    </div>
  )
}
