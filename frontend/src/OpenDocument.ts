import type { ElementDefinition } from 'cytoscape'

// The document whose graph is drawn.
export interface OpenDocument {
  fileName: string
  elements: ElementDefinition[]
}
