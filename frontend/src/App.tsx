import { useState } from 'react'
import styles from './App.module.css'
import type { OpenDocument } from './OpenDocument'
import { useReadGraph } from './api/graph'
import { useApiInfo } from './api/info'
import { GraphView } from './components/GraphView'
import { OpenFileButton } from './components/OpenFileButton'
import { toCytoscapeElements } from './graph/cytoscape'

const reservedMenuItems = ['Bundles', 'View', 'Validation']

export default function App() {
  const apiInfo = useApiInfo()
  const readGraph = useReadGraph()
  const [openDocument, setOpenDocument] = useState<OpenDocument | null>(null)

  const openFile = (file: File) =>
    readGraph.mutate(file, {
      // Callbacks passed to mutate run for the latest call only, so a slow earlier file can't
      // replace the graph of a newer one.
      onSuccess: (graph) => setOpenDocument({ fileName: file.name, elements: toCytoscapeElements(graph) }),
    })

  return (
    <div className={styles.app}>
      <header className={styles.header}>
        <h1 className={styles.title}>
          CPM Provenance Viewer
          {openDocument && <span className={styles.fileName}> · {openDocument.fileName}</span>}
        </h1>
        <span className={styles.apiStatus} data-status={apiInfo.status}>
          {apiInfo.isSuccess && `API ${apiInfo.data.version}`}
          {apiInfo.isPending && 'Connecting to API…'}
          {apiInfo.isError && 'API unavailable'}
        </span>
      </header>
      <nav className={styles.menu} aria-label="Main menu">
        <ul>
          <li>
            <span className={styles.menuHeading}>Document</span>
            <ul>
              <li>
                <OpenFileButton onOpen={openFile} />
              </li>
            </ul>
          </li>
          {reservedMenuItems.map((item) => (
            <li key={item}>
              <button type="button" disabled>
                {item}
              </button>
            </li>
          ))}
        </ul>
      </nav>
      <main className={styles.main}>
        {readGraph.isPending && (
          <p className={styles.documentStatus} role="status">
            Opening {readGraph.variables.name}…
          </p>
        )}
        {readGraph.isError && (
          <p className={styles.documentError} role="alert">
            {readGraph.variables.name} could not be opened. {readGraph.error.message}
          </p>
        )}
        {openDocument ? (
          <GraphView elements={openDocument.elements} />
        ) : (
          readGraph.isIdle && (
            <p className={styles.emptyState}>Open a PROV-JSON document from the Document menu.</p>
          )
        )}
      </main>
    </div>
  )
}
