import styles from './Field.module.css'
import type { FieldProps } from './FieldProps'

// One labelled row of a details section.
export function Field({ label, children }: FieldProps) {
  return (
    <div className={styles.field}>
      <dt className={styles.label}>{label}</dt>
      <dd className={styles.content}>{children}</dd>
    </div>
  )
}
