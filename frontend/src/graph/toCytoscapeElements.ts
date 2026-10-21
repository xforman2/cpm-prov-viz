import type { ElementDefinition } from 'cytoscape'
import type { Graph } from '../api/contract'

const maxLabelLength = 24

// Turns the graph contract into Cytoscape elements. Pure, so it can be tested without a renderer.
export function toCytoscapeElements(graph: Graph): ElementDefinition[] {
  const bundleNodes: ElementDefinition[] = graph.bundles.map((bundle) => ({
    group: 'nodes',
    data: { id: bundle.key, label: shortenIdentifier(bundle.id) },
    classes: 'bundle',
  }))
  const elementNodes: ElementDefinition[] = graph.elements.map((element) => {
    const data = { id: element.key, label: shortenIdentifier(element.id) }
    return {
      group: 'nodes',
      data: element.bundle === null ? data : { ...data, parent: element.bundle },
      classes: element.kind,
    }
  })
  // An edge needs both ends; a relation missing one can't be drawn.
  const relationEdges: ElementDefinition[] = graph.relations.flatMap((relation) =>
    relation.source === null || relation.target === null
      ? []
      : [{
          group: 'edges',
          data: { id: relation.key, source: relation.source, target: relation.target, label: relation.type },
        }],
  )
  return [...bundleNodes, ...elementNodes, ...relationEdges]
}

// The local part of a qualified name (after the first ':'), cut short with an ellipsis when long.
function shortenIdentifier(id: string): string {
  const localPart = id.slice(id.indexOf(':') + 1)
  return localPart.length <= maxLabelLength ? localPart : `${localPart.slice(0, maxLabelLength - 1)}…`
}
