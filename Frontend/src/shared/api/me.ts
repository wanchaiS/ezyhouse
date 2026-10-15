import { useQuery } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { api } from './client'
import { keys } from './keys'
import { queryClient } from './queryClient'
import type { User } from './types'

/** The signed-in user, or null when signed out (401). */
async function getMe(): Promise<User | null> {
  try {
    return (await api.get<User>('/me')).data
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 401) return null
    throw error
  }
}

export function useMe() {
  return useQuery({ queryKey: keys.me, queryFn: getMe })
}

/** Clears the login cookie and every cached answer; the guards then show /login. */
export async function logout() {
  await api.post('/auth/logout')
  queryClient.removeQueries({ predicate: (query) => query.queryKey[0] !== keys.me[0] })
  queryClient.setQueryData(keys.me, null)
}
