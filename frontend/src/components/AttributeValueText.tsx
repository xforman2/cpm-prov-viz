import { QualifiedNameText } from './QualifiedNameText'
import { SelectLink } from './SelectLink'
import styles from './AttributeValueText.module.css'
import type { AttributeValueTextProps } from './AttributeValueTextProps'

// An attribute value exactly as written, with its language tag and datatype. Only a value the
// document types as a qualified name is shown as one, and it links to the element it names.
export function AttributeValueText({ value, onSelect }: AttributeValueTextProps) {
  const text = value.iri === null ? value.value : <QualifiedNameText text={value.value} iri={value.iri} />
  return (
    <span className={styles.value}>
      <SelectLink target={value.element} onSelect={onSelect}>
        {text}
      </SelectLink>
      {value.language !== null && (
        <span className={styles.tag} title="Language tag">
          @{value.language}
        </span>
      )}
      {value.datatype !== null && (
        <span className={styles.tag}>
          <QualifiedNameText text={value.datatype} iri={value.datatypeIri ?? value.datatype} />
        </span>
      )}
    </span>
  )
}
