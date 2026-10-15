import { createContext, useContext } from 'react'

export const ToastContext = createContext<((message: string) => void) | null>(null)

/** Returns showToast(message): a short confirmation or error at the top of the screen. */
export function useToast() {
  const showToast = useContext(ToastContext)
  if (!showToast) throw new Error('useToast must be used inside <ToastProvider>')
  return showToast
}
