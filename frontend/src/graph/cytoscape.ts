import type { ElementDefinition } from 'cytoscape'
import type { Graph } from '../api/contract/Graph'
import type { RelationMembership } from '../api/contract/RelationMembership'
import { elementLabel, shortenIdentifier } from './labels'

// The class each membership gets, for the stylesheet. An element's membership is never 'joining'.
const membershipClasses: Record<RelationMembership, string> = {
  traversalInformation: 'traversal-information',
  domainSpecificInformation: 'domain-specific-information',
  joining: 'joining',
}

// Turns the graph contract into Cytoscape elements. Pure, so it can be tested without a renderer.
// Hiding the domain-specific information leaves out its elements and every relation touching them.
export function toCytoscapeElements(graph: Graph, hideDomainSpecificInformation: boolean): ElementDefinition[] {
  const bundleNodes: ElementDefinition[] = graph.bundles.map((bundle) => ({
    group: 'nodes',
    data: { id: bundle.key, label: shortenIdentifier(bundle.id) },
    classes: bundle.isProvenanceComponent ? ['bundle', 'provenance-component'] : ['bundle'],
    // A bundle is only a group: clicking it selects nothing.
    selectable: false,
  }))
  const elementNodes: ElementDefinition[] = graph.elements.flatMap((element) => {
    if (hideDomainSpecificInformation && element.membership === 'domainSpecificInformation') return []
    const data = { id: element.key, label: elementLabel(element) }
    return [{
      group: 'nodes',
      data: element.bundle === null ? data : { ...data, parent: element.bundle },
      classes: element.membership === null ? [element.kind] : [element.kind, membershipClasses[element.membership]],
    }]
  })
  // An edge needs both ends; a relation missing one can't be drawn.
  const relationEdges: ElementDefinition[] = graph.relations.flatMap((relation) =>
    relation.source === null || relation.target === null
      || (hideDomainSpecificInformation && touchesDomainSpecificInformation(relation.membership))
      ? []
      : [{
          group: 'edges',
          data: { id: relation.key, source: relation.source, target: relation.target, label: relation.type },
          classes: relation.membership === null ? [] : [membershipClasses[relation.membership]],
        }],
  )
  return [...bundleNodes, ...elementNodes, ...relationEdges]
}

function touchesDomainSpecificInformation(membership: RelationMembership | null): boolean {
  return membership === 'domainSpecificInformation' || membership === 'joining'
}
