import type { ReactNode } from 'react'
import type { ExpenseTag } from '../../shared/api/types'
import { BillIcon, CartIcon, KeyIcon, SofaIcon } from '../../shared/components/icons'

/** Each tag's tab, icon and the one line that says what it is for. */
export const TAGS: Record<ExpenseTag, { label: string; to: string; icon: ReactNode; pitch: string }> = {
  rent: { label: 'Rent', to: '/rent', icon: <KeyIcon />, pitch: 'The amount, how often, and who pays what' },
  bills: { label: 'Bills', to: '/bills', icon: <BillIcon />, pitch: 'Electricity, internet, water: split when each arrives' },
  groceries: { label: 'Groceries', to: '/groceries', icon: <CartIcon />, pitch: 'Food, cleaning, takeaway: split, then settle up' },
  furniture: { label: 'Furniture', to: '/furniture', icon: <SofaIcon />, pitch: 'Things the house buys together, like a couch' },
}

export const TAG_ORDER: ExpenseTag[] = ['rent', 'bills', 'groceries', 'furniture']
