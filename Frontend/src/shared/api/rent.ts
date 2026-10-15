import { api } from './client'
import type { RentPeriod } from './types'

export async function getRentPeriods() {
  return (await api.get<RentPeriod[]>('/rent-periods')).data
}
