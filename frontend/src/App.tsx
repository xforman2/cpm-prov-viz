import { useMemo, useState } from 'react'
import styles from './App.module.css'
import type { OpenDocument } from './OpenDocument'
import { useReadGraph } from './api/graph'
import { useApiInfo } from './api/info'
import { DetailsPanel } from './components/DetailsPanel'
import { GraphView } from './components/GraphView'
import { OpenFileButton } from './components/OpenFileButton'
import { toCytoscapeElements } from './graph/cytoscape'

export default function App() {
  const apiInfo = useApiInfo()
  const readGraph = useReadGraph()
  const [openDocument, setOpenDocument] = useState<OpenDocument | null>(null)
  // Key of the selected element or relation.
  const [selected, setSelected] = useState<string | null>(null)
  const [hideDomainSpecificInformation, setHideDomainSpecificInformation] = useState(false)
  const elements = useMemo(
    () => (openDocument ? toCytoscapeElements(openDocument.graph, hideDomainSpecificInformation) : []),
    [openDocument, hideDomainSpecificInformation],
  )
  const hasProvenanceComponent = openDocument?.graph.bundles.some((bundle) => bundle.isProvenanceComponent) ?? false

  const openFile = (file: File) =>
    readGraph.mutate(file, {
      // Callbacks passed to mutate run for the latest call only, so a slow earlier file can't
      // replace the graph of a newer one.
      onSuccess: (graph) => {
        setOpenDocument({ fileName: file.name, graph })
        setSelected(null)
      },
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
          <li>
            <button type="button" disabled>
              Bundles
            </button>
          </li>
          <li>
            <span className={styles.menuHeading}>View</span>
            <ul>
              <li>
                <button
                  type="button"
                  aria-pressed={hideDomainSpecificInformation}
                  disabled={!hasProvenanceComponent}
                  title={hasProvenanceComponent ? undefined : 'The document has no provenance component.'}
                  onClick={() => setHideDomainSpecificInformation((hide) => !hide)}
                >
                  {hideDomainSpecificInformation ? 'Show' : 'Hide'} domain-specific information
                </button>
              </li>
            </ul>
          </li>
          <li>
            <button type="button" disabled>
              Validation
            </button>
          </li>
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
          <div className={styles.workspace}>
            <GraphView elements={elements} selected={selected} onSelect={setSelected} />
            <DetailsPanel graph={openDocument.graph} selected={selected} onSelect={setSelected} />
          </div>
        ) : (
          readGraph.isIdle && (
            <p className={styles.emptyState}>Open a PROV-JSON document from the Document menu.</p>
          )
        )}
      </main>
    </div>
  )
}
