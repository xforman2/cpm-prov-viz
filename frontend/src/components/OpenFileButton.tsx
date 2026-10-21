import { useRef, type ChangeEvent } from 'react'
import type { OpenFileButtonProps } from './OpenFileButtonProps'

// A button that opens the browser's file picker for a PROV-JSON document.
export function OpenFileButton({ onOpen }: OpenFileButtonProps) {
  const inputRef = useRef<HTMLInputElement>(null)

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    // Clear the input so that picking the same file again still counts as a change.
    event.target.value = ''
    if (file) onOpen(file)
  }

  return (
    <>
      <button type="button" onClick={() => inputRef.current?.click()}>
        Open file…
      </button>
      <input
        ref={inputRef}
        type="file"
        accept=".json,application/json"
        hidden
        onChange={handleChange}
      />
    </>
  )
}
