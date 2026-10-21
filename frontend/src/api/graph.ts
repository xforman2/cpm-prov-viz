import { useMutation } from '@tanstack/react-query'
import type { Graph } from './contract/Graph'
import type { ProblemDetails } from './ProblemDetails'

const unreachableMessage = 'The API cannot be reached. Check that it is running.'

// Sends a PROV-JSON document to the API and returns its graph. Throws an Error whose message can be
// shown to the user: the API's own explanation when it rejects the document, otherwise a note that
// the API can't be reached.
export async function readGraph(file: Blob): Promise<Graph> {
  let response: Response
  try {
    response = await fetch('/api/graph', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: file,
    })
  } catch (error) {
    throw new Error(unreachableMessage, { cause: error })
  }

  if (response.ok) return (await response.json()) as Graph

  // The API answers its own errors with problem details. A server error without them comes from in
  // between, e.g. the dev server's proxy when the API is down.
  if (!response.headers.get('Content-Type')?.includes('application/problem+json')) {
    throw new Error(
      response.status >= 500 ? unreachableMessage : `The API answered with ${response.status} ${response.statusText}.`,
    )
  }
  const problem = (await response.json()) as ProblemDetails
  throw new Error(
    [problem.title, problem.detail].filter(Boolean).join(' ') || `The API answered with ${response.status}.`,
  )
}

// Sends a picked PROV-JSON file to the API. A mutation, not a query: each pick is a one-off POST and
// the API keeps nothing, so there is nothing to cache.
export function useReadGraph() {
  return useMutation({ mutationFn: (file: File) => readGraph(file) })
}
