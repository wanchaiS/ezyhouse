import { api } from './client'
import type { Balances, Settlement, SettlementInput } from './types'

export async function getBalances() {
  return (await api.get<Balances>('/balances')).data
}

export async function createSettlement(input: SettlementInput) {
  return (await api.post<Settlement>('/settlements', input)).data
}
