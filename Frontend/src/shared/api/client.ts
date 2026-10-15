import axios, { isAxiosError } from 'axios'
import { keys } from './keys'
import { queryClient } from './queryClient'

// The login cookie travels with every same-origin request, so there is no token handling here.
export const api = axios.create({ baseURL: '/api' })

api.interceptors.response.use(undefined, (error: unknown) => {
  // Signed out or expired: the route guards send the user to /login.
  if (isAxiosError(error) && error.response?.status === 401) {
    queryClient.setQueryData(keys.me, null)
  }
  return Promise.reject(error)
})
