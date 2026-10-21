import cytoscape, { type ElementDefinition } from 'cytoscape'
import cytoscapeElk from 'cytoscape-elk'
import expandCollapse from 'cytoscape-expand-collapse'
import { useEffect, useRef } from 'react'

cytoscape.use(cytoscapeElk)
cytoscape.use(expandCollapse)

interface GraphViewProps {
  elements: ElementDefinition[]
}

const style: cytoscape.StylesheetJson = [
  {
    selector: 'node',
    style: {
      shape: 'round-rectangle',
      'corner-radius': '8px',
      width: 170,
      height: 40,
      'background-color': '#ffffff',
      'border-width': 1,
      'border-color': '#cbd5e1',
      label: 'data(label)',
      'font-family': 'Inter, system-ui, sans-serif',
      'font-size': 12,
      color: '#0f172a',
      'text-valign': 'center',
      'text-wrap': 'ellipsis',
      'text-max-width': '160px',
    },
  },
  { selector: 'node.entity', style: { 'background-color': '#fef9c3', 'border-color': '#eab308' } },
  { selector: 'node.activity', style: { 'background-color': '#e0e7ff', 'border-color': '#6366f1' } },
  { selector: 'node.agent', style: { 'background-color': '#ffedd5', 'border-color': '#f97316' } },
  {
    selector: 'node.bundle',
    style: {
      'background-color': '#f8fafc',
      'border-style': 'dashed',
      'text-valign': 'top',
      'text-max-width': '1000px',
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
]

// Edges point towards the past, so laying them out leftwards makes time run left to right.
const layout = {
  name: 'elk',
  elk: {
    algorithm: 'layered',
    'elk.direction': 'LEFT',
    'elk.hierarchyHandling': 'INCLUDE_CHILDREN',
    'elk.layered.spacing.nodeNodeBetweenLayers': 120,
  },
} as cytoscape.LayoutOptions

export function GraphView({ elements }: GraphViewProps) {
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const cy = cytoscape({ container: containerRef.current, elements, style })
    const running = cy.layout(layout)
    let laidOut = false
    running.one('layoutstop', () => {
      laidOut = true
    })
    running.run()

    return () => {
      if (laidOut) {
        cy.destroy()
        return
      }
      // cytoscape-elk can't stop a running layout, and it writes the positions to the instance when
      // it finishes. Free the container now and destroy the instance only after that.
      cy.unmount()
      running.one('layoutstop', () => cy.destroy())
    }
  }, [elements])

  return <div ref={containerRef} className="graph" />
}
