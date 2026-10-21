import { useState } from 'react'
import type { OpenDocument } from './OpenDocument'
import { useApiInfo } from './api/useApiInfo'
import { useReadGraph } from './api/useReadGraph'
import { GraphView } from './components/GraphView'
import { OpenFileButton } from './components/OpenFileButton'
import { toCytoscapeElements } from './graph/toCytoscapeElements'

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
    <div className="app">
      <header className="app-header">
        <h1>
          CPM Provenance Viewer
          {openDocument && <span className="app-document"> · {openDocument.fileName}</span>}
        </h1>
        <span className={`api-status api-status--${apiInfo.status}`}>
          {apiInfo.isSuccess && `API ${apiInfo.data.version}`}
          {apiInfo.isPending && 'Connecting to API…'}
          {apiInfo.isError && 'API unavailable'}
        </span>
      </header>
      <nav className="app-menu" aria-label="Main menu">
        <ul>
          <li>
            <span className="app-menu-heading">Document</span>
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
      <main className="app-main">
        {readGraph.isPending && (
          <p className="document-status" role="status">
            Opening {readGraph.variables.name}…
          </p>
        )}
        {readGraph.isError && (
          <p className="document-status document-status--error" role="alert">
            {readGraph.variables.name} could not be opened. {readGraph.error.message}
          </p>
        )}
        {openDocument ? (
          <GraphView elements={openDocument.elements} />
        ) : (
          readGraph.isIdle && (
            <p className="empty-state">Open a PROV-JSON document from the Document menu.</p>
          )
        )}
      </main>
    </div>
  )
}
