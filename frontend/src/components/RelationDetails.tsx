import { findBundle } from '../graph/lookup'
import { AttributeFields } from './AttributeFields'
import { DetailsSection } from './DetailsSection'
import { ElementReference } from './ElementReference'
import { Field } from './Field'
import { QualifiedNameText } from './QualifiedNameText'
import { SelectLink } from './SelectLink'
import type { RelationDetailsProps } from './RelationDetailsProps'

// Everything the document states about a relation: its two main arguments, which the graph draws as
// the edge, and its other arguments, which it doesn't.
export function RelationDetails({ relation, graph, onSelect }: RelationDetailsProps) {
  const bundle = findBundle(graph, relation.bundle)
  return (
    <>
      <DetailsSection title="Relation">
        <Field label="Type">{relation.type}</Field>
        <Field label="Identifier">
          {relation.id !== null && relation.iri !== null ? <QualifiedNameText text={relation.id} iri={relation.iri} /> : 'None'}
        </Field>
        {relation.iri !== null && <Field label="IRI">{relation.iri}</Field>}
        <Field label="Bundle">
          {bundle ? <QualifiedNameText text={bundle.id} iri={bundle.iri} /> : 'None (top level)'}
        </Field>
        <Field label="Source">
          <ElementReference elementKey={relation.source} graph={graph} onSelect={onSelect} />
        </Field>
        <Field label="Target">
          <ElementReference elementKey={relation.target} graph={graph} onSelect={onSelect} />
        </Field>
      </DetailsSection>
      <DetailsSection title="Other participants" isEmpty={relation.participants.length === 0}>
        {relation.participants.map((participant) => (
          <Field key={participant.name} label={<QualifiedNameText text={participant.name} iri={participant.nameIri} />}>
            <SelectLink target={participant.element ?? participant.relation} onSelect={onSelect}>
              <QualifiedNameText text={participant.id} iri={participant.iri} />
            </SelectLink>
          </Field>
        ))}
      </DetailsSection>
      <AttributeFields attributes={relation.attributes} onSelect={onSelect} />
    </>
  )
}
