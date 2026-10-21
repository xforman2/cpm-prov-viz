import type { ElementDefinition } from 'cytoscape'
import { useEffect, useRef, useState } from 'react'
import { readGraph } from './api/graph'
import { fetchApiInfo, type ApiInfo } from './api/info'
import { GraphView } from './components/GraphView'
import { OpenFileButton } from './components/OpenFileButton'
import { toCytoscapeElements } from './graph/toCytoscapeElements'

type ApiStatus = { state: 'loading' } | { state: 'ok'; info: ApiInfo } | { state: 'error' }

type DocumentStatus =
  | { state: 'none' }
  | { state: 'loading'; fileName: string }
  | { state: 'error'; fileName: string; message: string }

interface OpenDocument {
  fileName: string
  elements: ElementDefinition[]
}

const reservedMenuItems = ['Bundles', 'View', 'Validation']

export default function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>({ state: 'loading' })
  const [openDocument, setOpenDocument] = useState<OpenDocument | null>(null)
  const [documentStatus, setDocumentStatus] = useState<DocumentStatus>({ state: 'none' })
  const openRequest = useRef<AbortController | null>(null)

  useEffect(() => {
    const controller = new AbortController()
    fetchApiInfo(controller.signal)
      .then((info) => setApiStatus({ state: 'ok', info }))
      .catch(() => {
        if (!controller.signal.aborted) setApiStatus({ state: 'error' })
      })
    return () => controller.abort()
  }, [])

  useEffect(() => () => openRequest.current?.abort(), [])

  const openFile = async (file: File) => {
    openRequest.current?.abort()
    const controller = new AbortController()
    openRequest.current = controller
    setDocumentStatus({ state: 'loading', fileName: file.name })
    try {
      const graph = await readGraph(file, controller.signal)
      setOpenDocument({ fileName: file.name, elements: toCytoscapeElements(graph) })
      setDocumentStatus({ state: 'none' })
    } catch (error) {
      if (controller.signal.aborted) return
      const message = error instanceof Error ? error.message : String(error)
      setDocumentStatus({ state: 'error', fileName: file.name, message })
    }
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>
          CPM Provenance Viewer
          {openDocument && <span className="app-document"> · {openDocument.fileName}</span>}
        </h1>
        <span className={`api-status api-status--${apiStatus.state}`}>
          {apiStatus.state === 'ok' && `API ${apiStatus.info.version}`}
          {apiStatus.state === 'loading' && 'Connecting to API…'}
          {apiStatus.state === 'error' && 'API unavailable'}
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
        {documentStatus.state === 'loading' && (
          <p className="document-status" role="status">
            Opening {documentStatus.fileName}…
          </p>
        )}
        {documentStatus.state === 'error' && (
          <p className="document-status document-status--error" role="alert">
            {documentStatus.fileName} could not be opened. {documentStatus.message}
          </p>
        )}
        {openDocument ? (
          <GraphView elements={openDocument.elements} />
        ) : (
          documentStatus.state === 'none' && (
            <p className="empty-state">Open a PROV-JSON document from the Document menu.</p>
          )
        )}
      </main>
    </div>
  )
}
