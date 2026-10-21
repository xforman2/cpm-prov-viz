import { elementLabel } from '../graph/labels'
import { findElement, findRelation } from '../graph/lookup'
import { ElementDetails } from './ElementDetails'
import { RelationDetails } from './RelationDetails'
import styles from './DetailsPanel.module.css'
import type { DetailsPanelProps } from './DetailsPanelProps'

// The side panel with the details of the selected element or relation.
export function DetailsPanel({ graph, selected, onSelect }: DetailsPanelProps) {
  const element = findElement(graph, selected)
  const relation = findRelation(graph, selected)
  return (
    <aside className={styles.panel} aria-label="Details">
      {element && (
        <>
          <h2 className={styles.title}>{elementLabel(element)}</h2>
          <ElementDetails element={element} graph={graph} onSelect={onSelect} />
        </>
      )}
      {relation && (
        <>
          <h2 className={styles.title}>{relation.type}</h2>
          <RelationDetails relation={relation} graph={graph} onSelect={onSelect} />
        </>
      )}
      {!element && !relation && <p className={styles.empty}>Click an element or relation to see its details.</p>}
    </aside>
  )
}
