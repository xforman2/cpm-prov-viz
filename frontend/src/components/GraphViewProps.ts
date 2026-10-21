import type { ElementDefinition } from 'cytoscape'

export interface GraphViewProps {
  elements: ElementDefinition[]
  // Key of the selected element or relation, or null when nothing is selected.
  selected: string | null
  onSelect: (key: string | null) => void
}
