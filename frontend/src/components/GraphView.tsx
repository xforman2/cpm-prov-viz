import cytoscape from 'cytoscape'
import cytoscapeElk from 'cytoscape-elk'
import { useEffect, useRef } from 'react'
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

export function GraphView({ elements }: GraphViewProps) {
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const cy = cytoscape({ container: containerRef.current, elements, style: graphStyle })
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

  return <div ref={containerRef} className={styles.graph} />
}
