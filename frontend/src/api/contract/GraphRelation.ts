import type { GraphAttribute } from './GraphAttribute'
import type { GraphParticipant } from './GraphParticipant'
import type { RelationMembership } from './RelationMembership'
import type { RelationType } from './RelationType'

export interface GraphRelation {
  key: string
  type: RelationType
  id: string | null
  iri: string | null
  bundle: string | null
  source: string | null
  target: string | null
  participants: GraphParticipant[]
  attributes: GraphAttribute[]
  // null outside any provenance component.
  membership: RelationMembership | null
}
