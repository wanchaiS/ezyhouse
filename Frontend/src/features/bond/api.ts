import { api } from '../../shared/api/client'
import type { Bond } from '../../shared/api/types'
import type { BondInput } from './types'

export async function createBond(input: BondInput) {
  return (await api.post<Bond>('/bonds', input)).data
}

export async function updateBond(id: number, input: BondInput) {
  return (await api.put<Bond>(`/bonds/${id}`, input)).data
}

export async function deleteBond(id: number) {
  await api.delete(`/bonds/${id}`)
}
