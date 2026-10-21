import styles from './QualifiedNameText.module.css'
import type { QualifiedNameTextProps } from './QualifiedNameTextProps'

// A qualified name in its compact form as written, with the full IRI on hover.
export function QualifiedNameText({ text, iri }: QualifiedNameTextProps) {
  return (
    <span className={styles.name} title={iri}>
      {text}
    </span>
  )
}
