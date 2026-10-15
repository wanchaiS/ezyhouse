import { api } from '../../shared/api/client'
import type { RentPeriod } from '../../shared/api/types'
import type { RentSchedule, RentScheduleInput } from './types'

export const rentScheduleKey = ['rentSchedules'] as const

export async function getRentSchedules() {
  return (await api.get<RentSchedule[]>('/rent-schedules')).data
}

export async function createRentSchedule(input: RentScheduleInput) {
  return (await api.post<RentSchedule>('/rent-schedules', input)).data
}

export async function updateRentSchedule(id: number, input: RentScheduleInput) {
  return (await api.put<RentSchedule>(`/rent-schedules/${id}`, input)).data
}

export async function deleteRentSchedule(id: number) {
  await api.delete(`/rent-schedules/${id}`)
}

export async function setRentSharePaid(periodId: number, userId: number, paid: boolean) {
  return (await api.put<RentPeriod>(`/rent-periods/${periodId}/shares/${userId}`, { paid })).data
}
