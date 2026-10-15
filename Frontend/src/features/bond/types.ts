export interface ContributionInput {
  userId: number
  amount: number
  date: string
}

export interface BondInput {
  total: number
  paidDate: string
  contributions: ContributionInput[]
}
