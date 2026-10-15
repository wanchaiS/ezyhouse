import { api } from '../../shared/api/client'
import type { Household } from '../../shared/api/types'

export async function setPrimary(userId: number) {
  return (await api.put<Household>('/household/primary', { userId })).data
}

/** `nextPrimaryUserId` is required when the primary leaves and others remain. */
export async function leaveHousehold(input: { nextPrimaryUserId?: number }) {
  await api.post('/household/leave', input)
}
