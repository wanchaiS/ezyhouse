import type { User } from '../../shared/api/types'

export interface LoginInput {
  email: string
  password: string
}

export interface RegisterInput {
  name: string
  email: string
  password: string
}

/** The token is also set as an HttpOnly cookie, so the app never reads it. */
export interface AuthResult {
  token: string
  user: User
}
