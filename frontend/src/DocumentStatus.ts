// Where opening a document stands. The graph of the last document that opened stays in OpenDocument.
export type DocumentStatus =
  | { state: 'none' }
  | { state: 'loading'; fileName: string }
  | { state: 'error'; fileName: string; message: string }
