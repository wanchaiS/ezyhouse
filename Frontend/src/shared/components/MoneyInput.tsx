import type { ComponentProps } from 'react'
import { Input } from './Field'

type MoneyInputProps = Omit<ComponentProps<'input'>, 'value' | 'onChange'> & {
  /** The text as typed; turn it into a number with parseMoney when saving. */
  value: string
  onChange: (value: string) => void
  /** The large amount style used at the top of money forms. */
  big?: boolean
}

// Keeps the typed text rather than a number, so "10." or "10.50" survive while typing and
// a third decimal reaches the server, which owns the money rules.
export function MoneyInput({ value, onChange, big = true, ...rest }: MoneyInputProps) {
  return (
    <Input
      {...rest}
      big={big}
      inputMode="decimal"
      placeholder="$0.00"
      autoComplete="off"
      value={value}
      onChange={(event) => onChange(event.target.value)}
    />
  )
}
