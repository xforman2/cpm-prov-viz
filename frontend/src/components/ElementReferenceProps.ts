import type { Graph } from '../api/contract/Graph'

export interface ElementReferenceProps {
  // Key of the element, or null when the relation names none.
  elementKey: string | null
  graph: Graph
  onSelect: (key: string) => void
}
