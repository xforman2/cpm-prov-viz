import type { StylesheetJson } from 'cytoscape'

// The Cytoscape stylesheet. Cytoscape draws on a canvas, which CSS can't reach, so the colours and
// the font are read from the design tokens in index.css when the stylesheet is built.
export function graphStyle(): StylesheetJson {
  return [
    {
      selector: 'node',
      style: {
        shape: 'round-rectangle',
        'corner-radius': '8px',
        width: 170,
        height: 40,
        'background-color': cssToken('--surface'),
        'border-width': 1,
        'border-color': cssToken('--node-border'),
        label: 'data(label)',
        'font-family': cssToken('--font-family'),
        'font-size': 12,
        color: cssToken('--text'),
        'text-valign': 'center',
        'text-wrap': 'ellipsis',
        'text-max-width': '160px',
      },
    },
    {
      selector: 'node.entity',
      style: { 'background-color': cssToken('--entity-bg'), 'border-color': cssToken('--entity-border') },
    },
    {
      selector: 'node.activity',
      style: { 'background-color': cssToken('--activity-bg'), 'border-color': cssToken('--activity-border') },
    },
    {
      selector: 'node.agent',
      style: { 'background-color': cssToken('--agent-bg'), 'border-color': cssToken('--agent-border') },
    },
    {
      selector: 'node.bundle',
      style: {
        'background-color': cssToken('--bg'),
        'border-style': 'dashed',
        'text-valign': 'top',
        'text-max-width': '1000px',
        'font-weight': 600,
      },
    },
    {
      selector: 'edge',
      style: {
        width: 1.5,
        'line-color': cssToken('--edge'),
        'target-arrow-shape': 'triangle',
        'target-arrow-color': cssToken('--edge'),
        'arrow-scale': 0.8,
        'curve-style': 'round-taxi',
        label: 'data(label)',
        'font-size': 10,
        color: cssToken('--text-muted'),
        'text-background-color': cssToken('--surface'),
        'text-background-opacity': 1,
      },
    },
  ]
}

// The value of a CSS custom property on the root element, e.g. cssToken('--text').
function cssToken(name: string): string {
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim()
}
