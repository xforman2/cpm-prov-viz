import type { GraphAttribute } from './GraphAttribute'
import type { GraphParticipant } from './GraphParticipant'
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
}
