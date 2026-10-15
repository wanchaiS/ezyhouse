import { useEffect, useRef, type ReactNode } from 'react'
import styles from './Sheet.module.css'

interface SheetProps {
  open: boolean
  /** Called when the user closes the sheet: Escape or a click on the backdrop. */
  onClose: () => void
  title: ReactNode
  children: ReactNode
}

/**
 * A modal on the native <dialog>: a bottom sheet below 820px, a centred dialog from 820px.
 * The body mounts only while open, so a form inside starts fresh every time.
 */
export function Sheet({ open, onClose, title, children }: SheetProps) {
  const ref = useRef<HTMLDialogElement>(null)
  // The native 'close' event also fires, a moment later, when we close the dialog ourselves. By
  // then the page may already have opened the next sheet, so that close must not reach onClose.
  const closingOurselves = useRef(false)

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) {
      closingOurselves.current = true
      dialog.close()
    }
  }, [open])

  return (
    <dialog
      ref={ref}
      className={styles.sheet}
      onClose={() => {
        if (closingOurselves.current) closingOurselves.current = false
        else onClose()
      }}
      // The inner div fills the dialog, so a click whose target is the dialog itself hit the backdrop.
      onClick={(event) => {
        if (event.target === event.currentTarget) onClose()
      }}
    >
      {open && (
        <div className={styles.inner}>
          <div className={styles.grabber} />
          <h2 className={styles.title}>{title}</h2>
          {children}
        </div>
      )}
    </dialog>
  )
}

/** The stacked full-width buttons at the bottom of a sheet (e.g. Save, then Cancel). */
export function SheetActions({ children }: { children: ReactNode }) {
  return <div className={styles.actions}>{children}</div>
}
