// The graph contract returned by POST /api/graph. It mirrors GraphDto and the …Dto records it holds
// (src/CpmProvViz.Application/Graph/Contract/), whose comments are its specification.

export type ElementKind = 'entity' | 'activity' | 'agent'

export type RelationType =
  | 'wasGeneratedBy'
  | 'used'
  | 'wasInformedBy'
  | 'wasStartedBy'
  | 'wasEndedBy'
  | 'wasInvalidatedBy'
  | 'wasDerivedFrom'
  | 'wasAttributedTo'
  | 'wasAssociatedWith'
  | 'actedOnBehalfOf'
  | 'wasInfluencedBy'
  | 'specializationOf'
  | 'alternateOf'
  | 'hadMember'
  | 'mentionOf'

export interface GraphAttributeValue {
  value: string
}

export interface GraphAttribute {
  name: string
  iri: string
  values: GraphAttributeValue[]
}

export interface GraphBundle {
  key: string
  id: string
  iri: string
  prefixes: Record<string, string>
}

export interface GraphElement {
  key: string
  id: string
  iri: string
  kind: ElementKind
  bundle: string | null
  attributes: GraphAttribute[]
}

export interface GraphRelation {
  key: string
  type: RelationType
  id: string | null
  iri: string | null
  bundle: string | null
  source: string | null
  target: string | null
  attributes: GraphAttribute[]
}

export interface ParseNote {
  message: string
  subject: string | null
}

export interface ValidationFinding {
  message: string
  subject: string | null
}

export interface Graph {
  prefixes: Record<string, string>
  bundles: GraphBundle[]
  elements: GraphElement[]
  relations: GraphRelation[]
  parseNotes: ParseNote[]
  validationFindings: ValidationFinding[]
}
