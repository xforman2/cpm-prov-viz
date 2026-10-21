import cytoscape from 'cytoscape'
import cytoscapeElk from 'cytoscape-elk'
import { useEffect, useEffectEvent, useRef } from 'react'
import { toCytoscapeElements } from '../graph/cytoscape'
import { graphStyle } from '../graph/style'
import styles from './GraphView.module.css'
import type { GraphViewProps } from './GraphViewProps'

cytoscape.use(cytoscapeElk)

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

// Draws the graph with Cytoscape. The instance is an external system, so effects keep it in step
// with the props: a new one for each graph or toggle of the domain-specific information, and the
// selection mirrored onto it.
export function GraphView({ graph, hideDomainSpecificInformation, selected, onSelect }: GraphViewProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const cyRef = useRef<cytoscape.Core | null>(null)
  // The instance outlives renders, so its tap handler must call the latest onSelect without
  // rebuilding the graph when the callback changes.
  const select = useEffectEvent((key: string | null) => onSelect(key))

  // Mirrors the selection onto the graph. A tap has already selected its element; one picked in the
  // side panel is selected here and brought into view.
  const showSelection = useEffectEvent(() => {
    const cy = cyRef.current
    if (!cy) return
    const target = selected === null ? cy.collection() : cy.getElementById(selected)
    cy.elements(':selected').difference(target).unselect()
    if (target.nonempty() && !target.selected()) {
      target.select()
      cy.animate({ center: { eles: target } }, { duration: 200 })
    }
  })

  useEffect(() => {
    // Each instance draws into its own child of the container. cytoscape-elk can't stop a running
    // layout and writes the positions to the instance when it finishes, so an instance may be
    // destroyed only after the next one is drawn; destroying it then clears just its own child.
    const container = document.createElement('div')
    container.className = styles.canvas
    containerRef.current?.append(container)
    const elements = toCytoscapeElements(graph, hideDomainSpecificInformation)
    const cy = cytoscape({ container, elements, style: graphStyle, boxSelectionEnabled: false })
    cyRef.current = cy
    cy.on('tap', (event) => {
      if (event.target === cy) select(null)
      else if (event.target.selectable()) select(event.target.id())
    })
    const running = cy.layout(layout)
    let laidOut = false
    // An instance shows the selection only once its elements have their positions, and only if
    // it is still the one drawn.
    running.one('layoutstop', () => {
      laidOut = true
      if (cyRef.current === cy) showSelection()
    })
    running.run()

    return () => {
      cyRef.current = null
      container.remove()
      if (laidOut) cy.destroy()
      else running.one('layoutstop', () => cy.destroy())
    }
  }, [graph, hideDomainSpecificInformation])

  useEffect(() => {
    showSelection()
  }, [selected])

  return <div ref={containerRef} className={styles.graph} />
}
