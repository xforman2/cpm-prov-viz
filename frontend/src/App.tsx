import { useEffect, useState } from 'react'
import { fetchApiInfo, type ApiInfo } from './api/info'
import { GraphView } from './components/GraphView'

type ApiStatus = { state: 'loading' } | { state: 'ok'; info: ApiInfo } | { state: 'error' }

const menuItems = ['Document', 'Bundles', 'View', 'Validation']

export default function App() {
  const [status, setStatus] = useState<ApiStatus>({ state: 'loading' })

  useEffect(() => {
    const controller = new AbortController()
    fetchApiInfo(controller.signal)
      .then((info) => setStatus({ state: 'ok', info }))
      .catch(() => {
        if (!controller.signal.aborted) setStatus({ state: 'error' })
      })
    return () => controller.abort()
  }, [])

  return (
    <div className="app">
      <header className="app-header">
        <h1>CPM Provenance Viewer</h1>
        <span className={`api-status api-status--${status.state}`}>
          {status.state === 'ok' && `API ${status.info.version}`}
          {status.state === 'loading' && 'Connecting to API…'}
          {status.state === 'error' && 'API unavailable'}
        </span>
      </header>
      <nav className="app-menu" aria-label="Main menu">
        <ul>
          {menuItems.map((item) => (
            <li key={item}>
              <button type="button" disabled>
                {item}
              </button>
            </li>
          ))}
        </ul>
      </nav>
      <main className="app-main">
        <GraphView />
      </main>
    </div>
  )
}
