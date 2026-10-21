import { useId } from 'react'
import styles from './CollapsiblePanel.module.css'
import type { CollapsiblePanelProps } from './CollapsiblePanelProps'

// A side panel that collapses to a thin strip holding the button that opens it again.
export function CollapsiblePanel({ side, name, open, onOpenChange, className, children }: CollapsiblePanelProps) {
  const contentId = useId()
  const label = `${open ? 'Hide' : 'Show'} ${name}`
  // The arrow points the way the panel will move: towards its edge to hide, away from it to show.
  const pointsLeft = (side === 'left') === open
  return (
    <div className={[styles.panel, className].filter(Boolean).join(' ')} data-side={side} data-open={open}>
      <div className={styles.bar}>
        <button
          type="button"
          className={styles.toggle}
          aria-expanded={open}
          aria-controls={contentId}
          aria-label={label}
          title={label}
          onClick={() => onOpenChange(!open)}
        >
          <span aria-hidden="true">{pointsLeft ? '‹' : '›'}</span>
        </button>
      </div>
      <div id={contentId} className={styles.content} hidden={!open}>
        {children}
      </div>
    </div>
  )
}
