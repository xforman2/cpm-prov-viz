import type { ReactNode } from 'react'

export interface SelectLinkProps {
  // Key of the element or relation to select, or null to show the children as plain text.
  target: string | null
  onSelect: (key: string) => void
  children: ReactNode
}
