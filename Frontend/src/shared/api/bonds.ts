import { api } from './client'
import type { Bond } from './types'

export async function getBonds() {
  return (await api.get<Bond[]>('/bonds')).data
}
