import type { Graph } from '../api/contract/Graph'

export interface DetailsPanelProps {
  graph: Graph
  // Key of the selected element or relation, or null when nothing is selected.
  selected: string | null
  onSelect: (key: string) => void
}
