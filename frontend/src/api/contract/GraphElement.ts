import type { CpmRole } from './CpmRole'
import type { ElementKind } from './ElementKind'
import type { ElementMembership } from './ElementMembership'
import type { GraphAttribute } from './GraphAttribute'

export interface GraphElement {
  key: string
  id: string
  iri: string
  kind: ElementKind
  bundle: string | null
  attributes: GraphAttribute[]
  cpmRoles: CpmRole[]
  // null outside any provenance component.
  membership: ElementMembership | null
}
