import cytoscape, { type ElementDefinition } from 'cytoscape'
import cytoscapeElk from 'cytoscape-elk'
import expandCollapse from 'cytoscape-expand-collapse'
import { useEffect, useRef } from 'react'

cytoscape.use(cytoscapeElk)
cytoscape.use(expandCollapse)

// Placeholder graph until the parser exists: one bundle with a tiny PROV chain.
const sampleElements: ElementDefinition[] = [
  { data: { id: 'bundle', label: 'ex:bundle1' } },
  { data: { id: 'dataset', parent: 'bundle', label: 'ex:dataset', kind: 'entity' } },
  { data: { id: 'analysis', parent: 'bundle', label: 'ex:analysis', kind: 'activity' } },
  { data: { id: 'report', parent: 'bundle', label: 'ex:report', kind: 'entity' } },
  { data: { id: 'researcher', label: 'ex:researcher', kind: 'agent' } },
  { data: { id: 'e1', source: 'analysis', target: 'dataset', label: 'used' } },
  { data: { id: 'e2', source: 'report', target: 'analysis', label: 'wasGeneratedBy' } },
  { data: { id: 'e3', source: 'analysis', target: 'researcher', label: 'wasAssociatedWith' } },
]

export function GraphView() {
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const cy = cytoscape({
      container: containerRef.current,
      elements: sampleElements,
      style: [
        {
          selector: 'node',
          style: {
            shape: 'round-rectangle',
            'corner-radius': '8px',
            width: 140,
            height: 40,
            'background-color': '#ffffff',
            'border-width': 1,
            'border-color': '#cbd5e1',
            label: 'data(label)',
            'font-family': 'Inter, system-ui, sans-serif',
            'font-size': 12,
            color: '#0f172a',
            'text-valign': 'center',
          },
        },
        { selector: 'node[kind = "entity"]', style: { 'background-color': '#fef9c3', 'border-color': '#eab308' } },
        { selector: 'node[kind = "activity"]', style: { 'background-color': '#e0e7ff', 'border-color': '#6366f1' } },
        { selector: 'node[kind = "agent"]', style: { 'background-color': '#ffedd5', 'border-color': '#f97316' } },
        {
          selector: ':parent',
          style: {
            'background-color': '#f8fafc',
            'border-style': 'dashed',
            'text-valign': 'top',
            'font-weight': 600,
          },
        },
        {
          selector: 'edge',
          style: {
            width: 1.5,
            'line-color': '#94a3b8',
            'target-arrow-shape': 'triangle',
            'target-arrow-color': '#94a3b8',
            'arrow-scale': 0.8,
            'curve-style': 'round-taxi',
            label: 'data(label)',
            'font-size': 10,
            color: '#64748b',
            'text-background-color': '#ffffff',
            'text-background-opacity': 1,
          },
        },
      ],
      layout: {
        name: 'elk',
        elk: {
          algorithm: 'layered',
          'elk.direction': 'LEFT',
          'elk.hierarchyHandling': 'INCLUDE_CHILDREN',
          'elk.layered.spacing.nodeNodeBetweenLayers': 120,
        },
      } as cytoscape.LayoutOptions,
    })

    return () => cy.destroy()
  }, [])

  return <div ref={containerRef} className="graph" />
}
