import { api } from './client'
import type { Furniture, FurnitureStatus } from './types'

export async function getFurniture(status?: FurnitureStatus) {
  return (await api.get<Furniture[]>('/furniture', { params: { status } })).data
}
