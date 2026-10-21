export interface GraphBundle {
  key: string
  id: string
  iri: string
  prefixes: Record<string, string>
  isProvenanceComponent: boolean
}
