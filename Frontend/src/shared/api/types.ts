// Shapes used by two or more slices, exactly as in API.md.
// Money is a number with at most 2 decimals; dates are 'YYYY-MM-DD' strings.

export type SplitType = 'equal' | 'exact'
export type Frequency = 'weekly' | 'fortnightly' | 'monthly' | 'quarterly'
/** Decides which tab shows an expense and who may change it; never how the money counts. */
export type ExpenseTag = 'rent' | 'bills' | 'groceries' | 'furniture'

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

export interface Expense {
  id: number
  tag: ExpenseTag
  description: string
  /** May be 0: a bill waiting for its amount. */
  amount: number
  date: string
  paidBy: UserRef
  createdBy: UserRef
  splitType: SplitType
  shares: Share[]
  /** Whether the viewer may edit or delete it. */
  canChange: boolean
  /** Rent created by a schedule. */
  rentScheduleId: number | null
  /** Rent created by a schedule: the last day it covers. */
  periodEnd: string | null
  /** Bills created by a bill type. */
  billTypeId: number | null
}

export interface ExpenseInput {
  description: string
  amount: number
  date: string
  paidByUserId: number
  splitType: SplitType
  shares: ShareInput[]
}

export interface NewExpenseInput extends ExpenseInput {
  tag: ExpenseTag
}

export interface RentSchedule {
  id: number
  amount: number
  frequency: Frequency
  startDate: string
  endDate: string | null
  splitType: SplitType
  shares: Share[]
  /** null once the schedule has ended. */
  nextDueDate: string | null
}

export interface BillType {
  id: number
  name: string
  frequency: Frequency
  startDate: string
  endDate: string | null
  members: UserRef[]
  /** null once the bill type has ended. */
  nextDueDate: string | null
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
