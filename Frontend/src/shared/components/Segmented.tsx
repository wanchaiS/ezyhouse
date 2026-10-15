import type { ReactNode } from 'react'
import styles from './Segmented.module.css'

interface SegmentedProps<T extends string> {
  options: readonly { value: T; label: ReactNode }[]
  value: T
  onChange: (value: T) => void
}

/** A segmented filter or switch, e.g. Owned / Sold / Disposed. */
export function Segmented<T extends string>({ options, value, onChange }: SegmentedProps<T>) {
  return (
    <div className={styles.seg} role="group">
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          className={option.value === value ? styles.on : undefined}
          aria-pressed={option.value === value}
          onClick={() => onChange(option.value)}
        >
          {option.label}
        </button>
      ))}
    </div>
  )
}
