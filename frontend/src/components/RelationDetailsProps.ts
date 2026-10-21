import type { Graph } from '../api/contract/Graph'
import type { GraphRelation } from '../api/contract/GraphRelation'

export interface RelationDetailsProps {
  relation: GraphRelation
  graph: Graph
  onSelect: (key: string) => void
}
