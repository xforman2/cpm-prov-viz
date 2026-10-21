import { findElement } from '../graph/lookup'
import { QualifiedNameText } from './QualifiedNameText'
import { SelectLink } from './SelectLink'
import type { ElementReferenceProps } from './ElementReferenceProps'

// A relation's main argument: the identifier of the element it names, as a link that selects it.
export function ElementReference({ elementKey, graph, onSelect }: ElementReferenceProps) {
  const element = findElement(graph, elementKey)
  if (!element) return 'No element in this bundle'
  return (
    <SelectLink target={element.key} onSelect={onSelect}>
      <QualifiedNameText text={element.id} iri={element.iri} />
    </SelectLink>
  )
}
