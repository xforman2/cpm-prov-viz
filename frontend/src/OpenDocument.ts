import type { Graph } from './api/contract/Graph'

// The document whose graph is drawn: its file name and its graph contract.
export interface OpenDocument {
  fileName: string
  graph: Graph
}
