import type { ComponentProps } from 'react'
import { Input } from './Field'

type DateInputProps = Omit<ComponentProps<'input'>, 'value' | 'onChange' | 'type'> & {
  /** "YYYY-MM-DD", or "" when empty. */
  value: string
  onChange: (value: string) => void
}

export function DateInput({ value, onChange, ...rest }: DateInputProps) {
  return <Input {...rest} type="date" value={value} onChange={(event) => onChange(event.target.value)} />
}
