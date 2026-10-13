import type { BrowserBounds } from '@maui/api/ui/remote-webview'

/** Intersects the slot with every clipping/scrolling ancestor; no guessed OS offsets or DPI. */
export function measureBrowserSlot(slot: HTMLElement): BrowserBounds {
  const rect = slot.getBoundingClientRect()
  let left = Math.max(0, rect.left), top = Math.max(0, rect.top)
  let right = Math.min(innerWidth, rect.right), bottom = Math.min(innerHeight, rect.bottom)
  for (let parent = slot.parentElement; parent; parent = parent.parentElement) {
    const style = getComputedStyle(parent), clip = parent.getBoundingClientRect()
    if (/(auto|scroll|hidden|clip)/.test(style.overflowX)) {
      left = Math.max(left, clip.left); right = Math.min(right, clip.right)
    }
    if (/(auto|scroll|hidden|clip)/.test(style.overflowY)) {
      top = Math.max(top, clip.top); bottom = Math.min(bottom, clip.bottom)
    }
  }
  const obscured = !!document.querySelector('dialog[open], [aria-modal="true"]:not([hidden]), [data-browser-obscured="true"]') ||
    (typeof CSS !== 'undefined' && CSS.supports?.('selector(:popover-open)') && !!document.querySelector(':popover-open'))
  return { x: rect.left, y: rect.top, width: rect.width, height: rect.height,
    clipX: left, clipY: top, clipWidth: Math.max(0, right - left), clipHeight: Math.max(0, bottom - top),
    viewportWidth: innerWidth, viewportHeight: innerHeight,
    visible: !document.hidden && !obscured && right > left && bottom > top }
}
