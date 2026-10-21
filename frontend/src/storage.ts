import { useState } from 'react'

// Settings remembered between reloads, in the browser's localStorage. Storage can be unavailable
// (a private window, blocked site data), so every read and write falls back silently.

function readFlag(key: string, fallback: boolean): boolean {
  try {
    const stored = localStorage.getItem(key)
    return stored === null ? fallback : stored === 'true'
  } catch {
    return fallback
  }
}

function writeFlag(key: string, value: boolean): void {
  try {
    localStorage.setItem(key, String(value))
  } catch {
    // Not remembered; the setting still holds until the page reloads.
  }
}

// A true/false setting kept under the key, `fallback` until it is first set.
export function useStoredFlag(key: string, fallback: boolean): [boolean, (value: boolean) => void] {
  const [flag, setFlag] = useState(() => readFlag(key, fallback))
  const set = (value: boolean) => {
    setFlag(value)
    writeFlag(key, value)
  }
  return [flag, set]
}
