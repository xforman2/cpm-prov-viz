import type { StylesheetJson } from 'cytoscape'

// The graph's own colours, font and sizes. Cytoscape draws on a canvas, which CSS can't reach, so
// they live here rather than in index.css.
const fontFamily = 'Inter, system-ui, sans-serif'
const text = '#0f172a'
const textMuted = '#64748b'
const white = '#ffffff'
const bundleBackground = '#f8fafc'
const nodeBorder = '#cbd5e1'
const edgeColor = '#94a3b8'
const selectedColor = '#4f46e5'
const entity = { background: '#fef9c3', border: '#eab308' }
const activity = { background: '#e0e7ff', border: '#6366f1' }
const agent = { background: '#ffedd5', border: '#f97316' }

const nodeWidth = 170
const nodeHeight = 40
// Labels stop short of the node's edges and end in an ellipsis when longer.
const labelMaxWidth = nodeWidth - 10

export const graphStyle: StylesheetJson = [
  {
    selector: 'node',
    style: {
      shape: 'round-rectangle',
      'corner-radius': '8px',
      width: nodeWidth,
      height: nodeHeight,
      'background-color': white,
      'border-width': 1,
      'border-color': nodeBorder,
      label: 'data(label)',
      'font-family': fontFamily,
      'font-size': 12,
      color: text,
      'text-valign': 'center',
      'text-wrap': 'ellipsis',
      'text-max-width': `${labelMaxWidth}px`,
    },
  },
  { selector: 'node.entity', style: { 'background-color': entity.background, 'border-color': entity.border } },
  { selector: 'node.activity', style: { 'background-color': activity.background, 'border-color': activity.border } },
  { selector: 'node.agent', style: { 'background-color': agent.background, 'border-color': agent.border } },
  {
    selector: 'node.bundle',
    style: {
      'background-color': bundleBackground,
      'border-style': 'dashed',
      'text-valign': 'top',
      // A bundle grows with its contents, so its title isn't cut at the node width.
      'text-max-width': '1000px',
      'font-weight': 600,
    },
  },
  {
    selector: 'edge',
    style: {
      width: 1.5,
      'line-color': edgeColor,
      'target-arrow-shape': 'triangle',
      'target-arrow-color': edgeColor,
      'arrow-scale': 0.8,
      'curve-style': 'round-taxi',
      label: 'data(label)',
      'font-size': 10,
      color: textMuted,
      'text-background-color': white,
      'text-background-opacity': 1,
    },
  },
  { selector: 'node:selected', style: { 'border-width': 3, 'border-color': selectedColor } },
  {
    selector: 'edge:selected',
    style: { width: 3, 'line-color': selectedColor, 'target-arrow-color': selectedColor, color: selectedColor },
  },
]
