import { api } from './client'
import type { Bill, BillStatus } from './types'

export async function getBills(status?: BillStatus) {
  return (await api.get<Bill[]>('/bills', { params: { status } })).data
}
