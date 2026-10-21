import type { GraphAttributeValue } from './GraphAttributeValue'

export interface GraphAttribute {
  name: string
  iri: string
  values: GraphAttributeValue[]
}
