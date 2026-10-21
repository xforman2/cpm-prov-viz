import styles from './SelectLink.module.css'
import type { SelectLinkProps } from './SelectLinkProps'

// A link in the side panel that selects another element or relation of the graph.
export function SelectLink({ target, onSelect, children }: SelectLinkProps) {
  if (target === null) return children
  return (
    <button type="button" className={styles.link} onClick={() => onSelect(target)}>
      {children}
    </button>
  )
}
