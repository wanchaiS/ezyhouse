import { api } from '../../shared/api/client'
import type { Household } from '../../shared/api/types'

export async function createHousehold(name: string) {
  return (await api.post<Household>('/household', { name })).data
}

export async function joinHousehold(inviteCode: string) {
  return (await api.post<Household>('/household/join', { inviteCode })).data
}
