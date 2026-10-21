import { useQuery } from '@tanstack/react-query'
import type { ApiInfo } from './ApiInfo'

export async function fetchApiInfo(signal?: AbortSignal): Promise<ApiInfo> {
  const response = await fetch('/api/info', { signal })
  if (!response.ok) {
    throw new Error(`GET /api/info failed with ${response.status}`)
  }
  return (await response.json()) as ApiInfo
}

// The API's name and version, which also tells whether the API is reachable.
export function useApiInfo() {
  return useQuery({ queryKey: ['apiInfo'], queryFn: ({ signal }) => fetchApiInfo(signal) })
}
