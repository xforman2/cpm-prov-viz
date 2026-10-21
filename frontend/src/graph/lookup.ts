import type { Graph } from '../api/contract/Graph'
import type { GraphBundle } from '../api/contract/GraphBundle'
import type { GraphElement } from '../api/contract/GraphElement'
import type { GraphRelation } from '../api/contract/GraphRelation'

// Finds the bundle, element or relation with a key from the graph contract.
export function findBundle(graph: Graph, key: string | null): GraphBundle | undefined {
  return graph.bundles.find((bundle) => bundle.key === key)
}

export function findElement(graph: Graph, key: string | null): GraphElement | undefined {
  return graph.elements.find((element) => element.key === key)
}

export function findRelation(graph: Graph, key: string | null): GraphRelation | undefined {
  return graph.relations.find((relation) => relation.key === key)
}
