import { api } from '../../shared/api/client'
import type { BillType } from '../../shared/api/types'
import type { BillTypeInput } from './types'

export async function createBillType(input: BillTypeInput) {
  return (await api.post<BillType>('/bill-types', input)).data
}

export async function updateBillType(id: number, input: BillTypeInput) {
  return (await api.put<BillType>(`/bill-types/${id}`, input)).data
}

export async function deleteBillType(id: number) {
  await api.delete(`/bill-types/${id}`)
}
