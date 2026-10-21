import { useQuery } from '@tanstack/react-query'
import { fetchApiInfo } from './info'

// The API's name and version, which also tells whether the API is reachable.
export function useApiInfo() {
  return useQuery({ queryKey: ['apiInfo'], queryFn: ({ signal }) => fetchApiInfo(signal) })
}
