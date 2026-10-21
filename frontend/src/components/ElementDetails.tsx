import { membershipLabel } from '../graph/labels'
import { findBundle } from '../graph/lookup'
import { AttributeFields } from './AttributeFields'
import { DetailsSection } from './DetailsSection'
import { Field } from './Field'
import { QualifiedNameText } from './QualifiedNameText'
import type { ElementDetailsProps } from './ElementDetailsProps'

// Everything the document states about an entity, activity or agent.
export function ElementDetails({ element, graph, onSelect }: ElementDetailsProps) {
  const bundle = findBundle(graph, element.bundle)
  return (
    <>
      <DetailsSection title="Element">
        <Field label="Identifier">
          <QualifiedNameText text={element.id} iri={element.iri} />
        </Field>
        <Field label="IRI">{element.iri}</Field>
        <Field label="Kind">{element.kind}</Field>
        <Field label="Bundle">
          {bundle ? <QualifiedNameText text={bundle.id} iri={bundle.iri} /> : 'None (top level)'}
        </Field>
      </DetailsSection>
      <DetailsSection title="CPM">
        <Field label="Roles">{element.cpmRoles.length === 0 ? 'None' : element.cpmRoles.join(', ')}</Field>
        <Field label="Membership">{membershipLabel(element.membership)}</Field>
      </DetailsSection>
      <AttributeFields attributes={element.attributes} onSelect={onSelect} />
    </>
  )
}
