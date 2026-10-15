import { useQuery } from '@tanstack/react-query'
import { api } from './client'
import { keys } from './keys'
import type { Household } from './types'

async function getHousehold() {
  return (await api.get<Household>('/household')).data
}

export function useHousehold() {
  return useQuery({ queryKey: keys.household, queryFn: getHousehold })
}
