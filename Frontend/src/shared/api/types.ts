// Shapes used by two or more slices, exactly as in API.md.
// Money is a number with at most 2 decimals; dates are 'YYYY-MM-DD' strings.

export type SplitType = 'equal' | 'exact'
export type Frequency = 'weekly' | 'fortnightly' | 'monthly' | 'quarterly'
export type BillStatus = 'pending' | 'submitted'
export type FurnitureStatus = 'owned' | 'sold' | 'disposed'

export interface UserRef {
  id: number
  name: string
}

export interface Share {
  user: UserRef
  amount: number
}

/** `amount` only when splitType is 'exact'. */
export interface ShareInput {
  userId: number
  amount?: number
}

export interface PaidShare {
  user: UserRef
  amount: number
  paid: boolean
  paidDate: string | null
}

export interface User {
  id: number
  name: string
  email: string
  householdId: number | null
}

export interface Household {
  id: number
  name: string
  inviteCode: string
  primary: UserRef
  members: UserRef[]
}

export interface BalanceMember {
  user: UserRef
  /** > 0: the household owes you; < 0: you owe. */
  net: number
}

export interface Debt {
  from: UserRef
  to: UserRef
  amount: number
}

export interface Balances {
  members: BalanceMember[]
  debts: Debt[]
}

export interface Settlement {
  id: number
  from: UserRef
  to: UserRef
  amount: number
  date: string
  createdBy: UserRef
}

export interface SettlementInput {
  fromUserId: number
  toUserId: number
  amount: number
}

export interface RentPeriod {
  id: number
  scheduleId: number
  startDate: string
  endDate: string
  amount: number
  payee: UserRef
  shares: PaidShare[]
}

export interface Bill {
  id: number
  billType: { id: number; name: string }
  dueDate: string
  status: BillStatus
  amount: number | null
  payee: UserRef | null
  submittedDate: string | null
  /** Empty while pending. */
  shares: PaidShare[]
}

export interface Purchase {
  date: string
  amount: number
  paidBy: UserRef
  splitType: SplitType
  shares: Share[]
}

export interface Sale {
  date: string
  amount: number
  receivedBy: UserRef
  splitType: SplitType
  shares: Share[]
}

/** `amount` may be 0. */
export interface Disposal {
  date: string
  amount: number
  paidBy: UserRef
  splitType: SplitType
  shares: Share[]
}

export interface Furniture {
  id: number
  name: string
  status: FurnitureStatus
  createdBy: UserRef
  purchase: Purchase
  sale: Sale | null
  disposal: Disposal | null
}

export interface Contribution {
  user: UserRef
  amount: number
  date: string
}

export interface Bond {
  id: number
  total: number
  paidDate: string
  contributions: Contribution[]
  /** total minus the sum of contributions. */
  outstanding: number
}
