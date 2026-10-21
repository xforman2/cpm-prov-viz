import type { ElementDefinition } from 'cytoscape'
import type { Graph } from './api/contract/Graph'

// The document whose graph is drawn: its graph contract and the Cytoscape elements made from it.
export interface OpenDocument {
  fileName: string
  graph: Graph
  elements: ElementDefinition[]
}
