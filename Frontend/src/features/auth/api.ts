import { api } from '../../shared/api/client'
import type { AuthResult, LoginInput, RegisterInput } from './types'

export async function login(input: LoginInput) {
  return (await api.post<AuthResult>('/auth/login', input)).data
}

export async function register(input: RegisterInput) {
  return (await api.post<AuthResult>('/auth/register', input)).data
}
