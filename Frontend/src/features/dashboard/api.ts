import { api } from '../../shared/api/client'
import type { Balances, Settlement, SettlementInput } from './types'

export async function getBalances() {
  return (await api.get<Balances>('/balances')).data
}

export async function getSettlements() {
  return (await api.get<Settlement[]>('/settlements')).data
}

export async function createSettlement(input: SettlementInput) {
  return (await api.post<Settlement>('/settlements', input)).data
}
