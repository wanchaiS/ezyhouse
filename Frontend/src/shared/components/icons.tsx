import type { ReactNode } from 'react'

// The mockup's line icons (24x24, stroke = currentColor).
function Svg({ children }: { children: ReactNode }) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {children}
    </svg>
  )
}

export const HomeIcon = () => (
  <Svg>
    <path d="M3 10.5 12 3l9 7.5V20a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1z" />
  </Svg>
)
export const CartIcon = () => (
  <Svg>
    <circle cx="9" cy="20" r="1.4" />
    <circle cx="18" cy="20" r="1.4" />
    <path d="M2 3h3l2.6 12.2h11.7L21.5 7H6" />
  </Svg>
)
export const BillIcon = () => (
  <Svg>
    <path d="M6 2.5h12v19l-3-2-3 2-3-2-3 2z" />
    <path d="M9 7.5h6M9 11.5h6M9 15.5h4" />
  </Svg>
)
export const KeyIcon = () => (
  <Svg>
    <circle cx="8" cy="15" r="4.5" />
    <path d="M11.2 11.8 20 3M16 7l3 3M13.5 9.5l2 2" />
  </Svg>
)
export const SofaIcon = () => (
  <Svg>
    <path d="M5 11V8a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v3" />
    <path d="M3 12.5a2 2 0 0 1 4 0V15h10v-2.5a2 2 0 0 1 4 0V18H3z" />
    <path d="M5 18v2M19 18v2" />
  </Svg>
)
export const PeopleIcon = () => (
  <Svg>
    <circle cx="9" cy="8" r="3.5" />
    <path d="M2.5 20a6.5 6.5 0 0 1 13 0" />
    <circle cx="17" cy="9" r="2.5" />
    <path d="M16.5 14a5 5 0 0 1 5 5" />
  </Svg>
)
export const ShieldIcon = () => (
  <Svg>
    <path d="M12 3 20 6v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6z" />
  </Svg>
)
export const PlusIcon = () => (
  <Svg>
    <path d="M12 5v14M5 12h14" />
  </Svg>
)
export const ChevronIcon = () => (
  <Svg>
    <path d="m9 6 6 6-6 6" />
  </Svg>
)
export const BackIcon = () => (
  <Svg>
    <path d="m15 6-6 6 6 6" />
  </Svg>
)
export const CheckIcon = () => (
  <Svg>
    <path d="m5 12.5 4.5 4.5L19 7.5" />
  </Svg>
)
