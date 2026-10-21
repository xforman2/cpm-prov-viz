import type { Graph } from '../api/contract/Graph'
import type { GraphElement } from '../api/contract/GraphElement'

export interface ElementDetailsProps {
  element: GraphElement
  graph: Graph
  onSelect: (key: string) => void
}
