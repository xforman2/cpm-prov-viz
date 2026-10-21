import type { ElementDefinition } from 'cytoscape'
import type { Graph } from '../api/contract/Graph'
import { elementLabel, shortenIdentifier } from './labels'

// Turns the graph contract into Cytoscape elements. Pure, so it can be tested without a renderer.
export function toCytoscapeElements(graph: Graph): ElementDefinition[] {
  const bundleNodes: ElementDefinition[] = graph.bundles.map((bundle) => ({
    group: 'nodes',
    data: { id: bundle.key, label: shortenIdentifier(bundle.id) },
    classes: 'bundle',
    // A bundle is only a group: clicking it selects nothing.
    selectable: false,
  }))
  const elementNodes: ElementDefinition[] = graph.elements.map((element) => {
    const data = { id: element.key, label: elementLabel(element) }
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

