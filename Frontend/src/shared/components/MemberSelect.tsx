import type { ComponentProps } from 'react'
import type { UserRef } from '../api/types'
import { Select } from './Field'

type MemberSelectProps = Omit<ComponentProps<'select'>, 'value' | 'onChange'> & {
  members: UserRef[]
  /** The chosen user's id, or null for none yet. */
  value: number | null
  onChange: (userId: number) => void
}

export function MemberSelect({ members, value, onChange, ...rest }: MemberSelectProps) {
  return (
    <Select {...rest} value={value ?? ''} onChange={(event) => onChange(Number(event.target.value))}>
      {value === null && (
        <option value="" disabled>
          Choose…
        </option>
      )}
      {members.map((member) => (
        <option key={member.id} value={member.id}>
          {member.name}
        </option>
      ))}
    </Select>
  )
}
