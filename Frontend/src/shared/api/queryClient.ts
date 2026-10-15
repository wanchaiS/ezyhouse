import { QueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'

// A 4xx won't change by asking again, so only network errors and 5xx are retried.
function shouldRetry(failureCount: number, error: unknown) {
  const status = isAxiosError(error) ? error.response?.status : undefined
  return failureCount < 2 && (status === undefined || status >= 500)
}

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: shouldRetry },
  },
})
