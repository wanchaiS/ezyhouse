import { isAxiosError } from 'axios'
import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

const GENERIC = 'Something went wrong. Please try again.'

/** Turns any error from an API call into ProblemDetails. */
export function toProblem(error: unknown): ProblemDetails {
  if (!isAxiosError(error) || !error.response) {
    return { title: 'Could not reach the server. Check your connection and try again.' }
  }
  const { status, data } = error.response
  if (status >= 500 || typeof data !== 'object' || data === null) {
    return { status, title: GENERIC }
  }
  return { status, ...(data as ProblemDetails) }
}

/** One message to show for an error: `detail`, else the field messages, else `title`. */
export function problemMessage(error: unknown): string {
  const problem = toProblem(error)
  if (problem.detail) return problem.detail
  const fieldMessages = Object.values(problem.errors ?? {}).flat()
  if (fieldMessages.length) return fieldMessages.join(' ')
  return problem.title ?? GENERIC
}

// "$.splitType" -> "splittype", "Shares[0].Amount" -> "shares.0.amount"
function normalise(key: string) {
  return key.replace(/^\$\./, '').replace(/\[(\d+)\]/g, '.$1').toLowerCase()
}

/**
 * Puts a 400's field errors on the form's fields (matched case-insensitively) and everything
 * else on `root`. `fields` lists the form's field names, because the server's keys may differ
 * in case or carry a `$.` prefix.
 */
export function applyFieldErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fields: readonly Path<T>[],
) {
  const problem = toProblem(error)
  if (problem.status !== 400 || !problem.errors) {
    setError('root', { message: problemMessage(error) })
    return
  }
  const byName = new Map(fields.map((field) => [field.toLowerCase(), field]))
  const unmatched: string[] = []
  for (const [key, messages] of Object.entries(problem.errors)) {
    const field = byName.get(normalise(key))
    if (field) setError(field, { message: messages.join(' ') })
    else unmatched.push(...messages)
  }
  if (unmatched.length) setError('root', { message: unmatched.join(' ') })
}
