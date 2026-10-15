import { api } from '../../shared/api/client'
import type { Bill } from '../../shared/api/types'
import type { BillType, BillTypeInput, SubmitBillInput } from './types'

export const billTypesKey = ['billTypes'] as const

export async function getBillTypes() {
  return (await api.get<BillType[]>('/bill-types')).data
}

export async function createBillType(input: BillTypeInput) {
  return (await api.post<BillType>('/bill-types', input)).data
}

export async function updateBillType(id: number, input: BillTypeInput) {
  return (await api.put<BillType>(`/bill-types/${id}`, input)).data
}

export async function deleteBillType(id: number) {
  await api.delete(`/bill-types/${id}`)
}

export async function submitBill(id: number, input: SubmitBillInput) {
  return (await api.post<Bill>(`/bills/${id}/submit`, input)).data
}

export async function setBillSharePaid(billId: number, userId: number, paid: boolean) {
  return (await api.put<Bill>(`/bills/${billId}/shares/${userId}`, { paid })).data
}
