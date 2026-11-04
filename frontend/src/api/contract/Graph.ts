import type { GraphBundle } from './GraphBundle'
import type { GraphElement } from './GraphElement'
import type { GraphRelation } from './GraphRelation'
import type { ParseNote } from './ParseNote'
import type { ValidationFinding } from './ValidationFinding'

// The graph contract returned by the provenance service's POST /api/graph: this interface and the
// ones it holds, in this folder. It mirrors the Java records GraphDto and the …Dto records and enums
// it holds (provenance-service/src/main/java/cz/muni/fi/cpmprovviz/provenance/application/graph/contract/),
// whose Javadoc is its specification.
export interface Graph {
  prefixes: Record<string, string>
  bundles: GraphBundle[]
  elements: GraphElement[]
  relations: GraphRelation[]
  parseNotes: ParseNote[]
  validationFindings: ValidationFinding[]
}
