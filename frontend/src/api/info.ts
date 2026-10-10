export interface ApiInfo {
  name: string
  version: string
}

export async function fetchApiInfo(signal?: AbortSignal): Promise<ApiInfo> {
  const response = await fetch('/api/info', { signal })
  if (!response.ok) {
    throw new Error(`GET /api/info failed with ${response.status}`)
  }
  return (await response.json()) as ApiInfo
}
