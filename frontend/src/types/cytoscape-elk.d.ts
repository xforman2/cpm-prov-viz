// The Cytoscape extension ships without TypeScript types.
declare module 'cytoscape-elk' {
  import type { Ext } from 'cytoscape'
  const ext: Ext
  export default ext
}
