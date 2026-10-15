import { api } from './client'
import type { RentSchedule } from './types'

export async function getRentSchedules() {
  return (await api.get<RentSchedule[]>('/rent-schedules')).data
}
