import type { Graph } from '../api/contract/Graph'

export interface GraphViewProps {
  graph: Graph
  hideDomainSpecificInformation: boolean
  // Key of the selected element or relation, or null when nothing is selected.
  selected: string | null
  onSelect: (key: string | null) => void
}
