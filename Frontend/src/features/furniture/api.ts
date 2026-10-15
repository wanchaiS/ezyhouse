import { api } from '../../shared/api/client'
import type { Furniture } from '../../shared/api/types'
import type { DisposalInput, FurnitureInput, SaleInput } from './types'

export async function createFurniture(input: FurnitureInput) {
  return (await api.post<Furniture>('/furniture', input)).data
}

export async function updateFurniture(id: number, input: FurnitureInput) {
  return (await api.put<Furniture>(`/furniture/${id}`, input)).data
}

export async function deleteFurniture(id: number) {
  await api.delete(`/furniture/${id}`)
}

export async function sellFurniture(id: number, input: SaleInput) {
  return (await api.post<Furniture>(`/furniture/${id}/sell`, input)).data
}

export async function disposeFurniture(id: number, input: DisposalInput) {
  return (await api.post<Furniture>(`/furniture/${id}/dispose`, input)).data
}
