import type { ElementKind } from './ElementKind'
import type { GraphAttribute } from './GraphAttribute'

export interface GraphElement {
  key: string
  id: string
  iri: string
  kind: ElementKind
  bundle: string | null
  attributes: GraphAttribute[]
}
