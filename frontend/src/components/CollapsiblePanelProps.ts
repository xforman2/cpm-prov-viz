import type { ReactNode } from 'react'

export interface CollapsiblePanelProps {
  // The edge of the window the panel sits against; it collapses towards that edge.
  side: 'left' | 'right'
  // What the panel holds, for its button: "Hide details", "Show details".
  name: string
  open: boolean
  onOpenChange: (open: boolean) => void
  // Placement in the parent's layout, e.g. a grid area.
  className?: string
  children: ReactNode
}
