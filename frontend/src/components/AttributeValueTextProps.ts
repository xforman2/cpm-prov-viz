import type { GraphAttributeValue } from '../api/contract/GraphAttributeValue'

export interface AttributeValueTextProps {
  value: GraphAttributeValue
  onSelect: (key: string) => void
}
