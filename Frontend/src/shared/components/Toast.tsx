import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react'
import { ToastContext } from './toastContext'
import styles from './Toast.module.css'

const SHOW_FOR_MS = 3500

/** Wrap the app once; components then call useToast(). */
export function ToastProvider({ children }: { children: ReactNode }) {
  const [message, setMessage] = useState<string | null>(null)
  const timer = useRef<number | undefined>(undefined)

  const showToast = useCallback((text: string) => {
    setMessage(text)
    window.clearTimeout(timer.current)
    timer.current = window.setTimeout(() => setMessage(null), SHOW_FOR_MS)
  }, [])

  useEffect(() => () => window.clearTimeout(timer.current), [])

  return (
    <ToastContext.Provider value={showToast}>
      {children}
      {message && (
        <div className={styles.toast} role="status">
          {message}
        </div>
      )}
    </ToastContext.Provider>
  )
}
