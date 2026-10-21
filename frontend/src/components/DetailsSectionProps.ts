import type { ReactNode } from 'react'

export interface DetailsSectionProps {
  title: string
  // Shows "None" in place of the fields.
  isEmpty?: boolean
  children?: ReactNode
}
