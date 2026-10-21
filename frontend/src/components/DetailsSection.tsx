import styles from './DetailsSection.module.css'
import type { DetailsSectionProps } from './DetailsSectionProps'

// A titled group of fields in the side panel.
export function DetailsSection({ title, isEmpty = false, children }: DetailsSectionProps) {
  return (
    <section className={styles.section}>
      <h3 className={styles.title}>{title}</h3>
      {isEmpty ? <p className={styles.none}>None</p> : <dl className={styles.fields}>{children}</dl>}
    </section>
  )
}
