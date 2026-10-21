import type { ApiInfo } from './api/ApiInfo'

export type ApiStatus = { state: 'loading' } | { state: 'ok'; info: ApiInfo } | { state: 'error' }
