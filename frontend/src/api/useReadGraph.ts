import { useMutation } from '@tanstack/react-query'
import { readGraph } from './graph'

// Sends a picked PROV-JSON file to the API. A mutation, not a query: each pick is a one-off POST and
// the API keeps nothing, so there is nothing to cache.
export function useReadGraph() {
  return useMutation({ mutationFn: (file: File) => readGraph(file) })
}
