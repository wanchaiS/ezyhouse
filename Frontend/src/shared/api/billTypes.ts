import { api } from './client'
import type { BillType } from './types'

export async function getBillTypes() {
  return (await api.get<BillType[]>('/bill-types')).data
}
