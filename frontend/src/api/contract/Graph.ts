import type { GraphBundle } from './GraphBundle'
import type { GraphElement } from './GraphElement'
import type { GraphRelation } from './GraphRelation'
import type { ParseNote } from './ParseNote'
import type { ValidationFinding } from './ValidationFinding'

// The graph contract returned by POST /api/graph: this interface and the ones it holds, in this
// folder. It mirrors GraphDto and the …Dto records it holds
// (src/CpmProvViz.Application/Graph/Contract/), whose comments are its specification.
export interface Graph {
  prefixes: Record<string, string>
  bundles: GraphBundle[]
  elements: GraphElement[]
  relations: GraphRelation[]
  parseNotes: ParseNote[]
  validationFindings: ValidationFinding[]
}
