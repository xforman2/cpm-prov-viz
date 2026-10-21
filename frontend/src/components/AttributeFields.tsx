import { AttributeValueText } from './AttributeValueText'
import { DetailsSection } from './DetailsSection'
import { Field } from './Field'
import { QualifiedNameText } from './QualifiedNameText'
import styles from './AttributeFields.module.css'
import type { AttributeFieldsProps } from './AttributeFieldsProps'

// The attributes of an element or relation, with every value of each, in document order.
export function AttributeFields({ attributes, onSelect }: AttributeFieldsProps) {
  return (
    <DetailsSection title="Attributes" isEmpty={attributes.length === 0}>
      {attributes.map((attribute, index) => (
        // An attribute name can repeat only in a malformed record, so the index keeps keys unique.
        <Field key={index} label={<QualifiedNameText text={attribute.name} iri={attribute.iri} />}>
          <ul className={styles.values}>
            {attribute.values.map((value, valueIndex) => (
              <li key={valueIndex}>
                <AttributeValueText value={value} onSelect={onSelect} />
              </li>
            ))}
          </ul>
        </Field>
      ))}
    </DetailsSection>
  )
}
