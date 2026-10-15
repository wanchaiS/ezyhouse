import { api } from '../../shared/api/client'
import type { Household } from '../../shared/api/types'

export async function setPrimary(userId: number) {
  return (await api.put<Household>('/household/primary', { userId })).data
}

export async function leaveHousehold() {
  await api.post('/household/leave')
}
