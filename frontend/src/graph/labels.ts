import type { GraphElement } from '../api/contract/GraphElement'
import type { RelationMembership } from '../api/contract/RelationMembership'

const maxLabelLength = 24

// The attributes whose first value labels an element, best first. Matched by IRI, so the prefix in the
// document doesn't matter. schema.org is written with either scheme.
const labelAttributeIris = [
  ['http://www.w3.org/ns/prov#label'],
  ['https://schema.org/name', 'http://schema.org/name'],
]

// An element's label: its prov:label, otherwise its schema:name, otherwise its shortened identifier.
export function elementLabel(element: GraphElement): string {
  for (const iris of labelAttributeIris) {
    const label = element.attributes.find((attribute) => iris.includes(attribute.iri))?.values[0]?.value
    if (label) return label
  }
  return shortenIdentifier(element.id)
}

// The local part of a qualified name (after the first ':'), cut short with an ellipsis when long.
export function shortenIdentifier(id: string): string {
  const localPart = id.slice(id.indexOf(':') + 1)
  return localPart.length <= maxLabelLength ? localPart : `${localPart.slice(0, maxLabelLength - 1)}…`
}

const membershipLabels: Record<RelationMembership, string> = {
  traversalInformation: 'Traversal information',
  domainSpecificInformation: 'Domain-specific information',
  joining: 'Joins traversal and domain-specific information',
}

// Which part of a provenance component an element or relation belongs to.
export function membershipLabel(membership: RelationMembership | null): string {
  return membership === null ? 'None (not in a provenance component)' : membershipLabels[membership]
}
