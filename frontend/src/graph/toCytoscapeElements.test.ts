import type { ElementDefinition } from 'cytoscape'
import { describe, expect, it } from 'vitest'
import type { Graph } from '../api/contract/Graph'
import samplingV0 from '../../../testdata/contract/embrc/dataset1/SamplingBundle_V0.json'
import processingV1 from '../../../testdata/contract/embrc/dataset2/ProcessingBundle_V1.json'
import twoBundles from '../../../testdata/contract/handmade/two-bundles.json'
import { toCytoscapeElements } from './toCytoscapeElements'

const twoBundlesGraph = twoBundles as Graph

function byKey(elements: ElementDefinition[], key: string): ElementDefinition | undefined {
  return elements.find((e) => e.data.id === key)
}

describe('toCytoscapeElements', () => {
  it.each([
    ['embrc/dataset1/SamplingBundle_V0.json', samplingV0],
    ['embrc/dataset2/ProcessingBundle_V1.json', processingV1],
    ['handmade/two-bundles.json', twoBundles],
  ])('draws every element and relation of %s, joined only to nodes that exist', (_, fixture) => {
    const graph = fixture as Graph
    const elements = toCytoscapeElements(graph)
    const nodeIds = new Set(elements.filter((e) => e.group === 'nodes').map((e) => e.data.id))
    const edges = elements.filter((e) => e.group === 'edges')

    expect(nodeIds.size).toBe(graph.bundles.length + graph.elements.length)
    expect(edges).toHaveLength(graph.relations.length)
    for (const edge of edges) {
      expect(nodeIds).toContain(edge.data.source)
      expect(nodeIds).toContain(edge.data.target)
    }
    for (const node of elements.filter((e) => e.data.parent !== undefined)) {
      expect(nodeIds).toContain(node.data.parent)
    }
  })

  it('turns each bundle into a compound node that holds its elements', () => {
    const elements = toCytoscapeElements(twoBundlesGraph)

    expect(byKey(elements, 'b1')).toMatchObject({ group: 'nodes', classes: 'bundle' })
    expect(byKey(elements, 'b2')).toMatchObject({ group: 'nodes', classes: 'bundle' })
    expect(byKey(elements, 'e5')?.data.parent).toBe('b1')
    expect(byKey(elements, 'e7')?.data.parent).toBe('b2')
  })

  it('leaves top-level elements outside every bundle', () => {
    const elements = toCytoscapeElements(twoBundlesGraph)

    for (const key of ['e1', 'e2', 'e3']) {
      expect(byKey(elements, key)?.data).not.toHaveProperty('parent')
    }
  })

  it('gives each element its kind as a class', () => {
    const elements = toCytoscapeElements(twoBundlesGraph)

    expect(byKey(elements, 'e3')?.classes).toBe('agent')
    expect(byKey(elements, 'e5')?.classes).toBe('entity')
    expect(byKey(elements, 'e6')?.classes).toBe('activity')
  })

  it('draws each relation as an edge from its source to its target, labelled with its type', () => {
    const elements = toCytoscapeElements(twoBundlesGraph)

    expect(byKey(elements, 'r5')).toEqual({
      group: 'edges',
      data: { id: 'r5', source: 'e5', target: 'e4', label: 'specializationOf' },
    })
    expect(byKey(elements, 'r6')).toEqual({
      group: 'edges',
      data: { id: 'r6', source: 'e8', target: 'e7', label: 'used' },
    })
  })

  it('leaves out a relation that is missing its source or its target', () => {
    const graph: Graph = {
      ...twoBundlesGraph,
      relations: [
        { ...twoBundlesGraph.relations[0], key: 'r1', source: null },
        { ...twoBundlesGraph.relations[0], key: 'r2', target: null },
      ],
    }

    const edges = toCytoscapeElements(graph).filter((e) => e.group === 'edges')

    expect(edges).toEqual([])
  })

  it('labels elements and bundles with the local part of their identifier', () => {
    const elements = toCytoscapeElements(twoBundlesGraph)

    expect(byKey(elements, 'e5')?.data.label).toBe('waterSample')
    expect(byKey(elements, 'b1')?.data.label).toBe('samplingBundle')
  })

  it('cuts a long identifier short so that hash-like identifiers stay readable', () => {
    const elements = toCytoscapeElements(samplingV0 as Graph)

    // gen:92b8f2fa96e3b102c47d14fabec8e2a3ded5d27b5996e5a6967c8e39b062b9f5
    expect(byKey(elements, 'e2')?.data.label).toBe('92b8f2fa96e3b102c47d14f…')
    // storage:dc8efed0-0035-4029-9065-8c46667151db
    expect(byKey(elements, 'b1')?.data.label).toBe('dc8efed0-0035-4029-9065…')
  })
})
