// The Cytoscape extensions ship without TypeScript types.
declare module 'cytoscape-elk' {
  import type { Ext } from 'cytoscape'
  const ext: Ext
  export default ext
}

declare module 'cytoscape-expand-collapse' {
  import type { Ext } from 'cytoscape'
  const ext: Ext
  export default ext
}
