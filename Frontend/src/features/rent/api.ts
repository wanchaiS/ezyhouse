import { api } from '../../shared/api/client'
import type { RentSchedule } from '../../shared/api/types'
import type { RentScheduleInput } from './types'

export async function createRentSchedule(input: RentScheduleInput) {
  return (await api.post<RentSchedule>('/rent-schedules', input)).data
}

export async function updateRentSchedule(id: number, input: RentScheduleInput) {
  return (await api.put<RentSchedule>(`/rent-schedules/${id}`, input)).data
}

export async function deleteRentSchedule(id: number) {
  await api.delete(`/rent-schedules/${id}`)
}
