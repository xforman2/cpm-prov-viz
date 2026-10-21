import type { GraphAttribute } from '../api/contract/GraphAttribute'

export interface AttributeFieldsProps {
  attributes: GraphAttribute[]
  onSelect: (key: string) => void
}
