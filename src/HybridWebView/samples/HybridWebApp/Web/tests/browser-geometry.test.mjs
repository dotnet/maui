import { test } from 'vitest'
import assert from 'node:assert/strict'
import { measureBrowserSlot } from '../src/browser-geometry.ts'

test('native slot geometry uses viewport CSS coordinates and real clipping ancestors rather than guessed native DPI/offsets', () => {
  const parent = document.createElement('main')
  const slot = document.createElement('div')
  parent.style.overflowX = 'hidden'
  parent.style.overflowY = 'auto'
  parent.append(slot)
  document.body.append(parent)
  parent.getBoundingClientRect = () => new DOMRect(150, 200, 600, 400)
  slot.getBoundingClientRect = () => new DOMRect(100, 160, 700, 500)
  const bounds = measureBrowserSlot(slot)
  assert.equal(bounds.x, 100)
  assert.equal(bounds.y, 160)
  assert.equal(bounds.clipX, 150)
  assert.equal(bounds.clipY, 200)
  assert.equal(bounds.clipWidth, 600)
  assert.equal(bounds.clipHeight, 400)
  assert.equal(bounds.viewportWidth, innerWidth)
  assert.equal(bounds.visible, true)
  const overlay = document.createElement('div')
  overlay.dataset.browserObscured = 'true'
  document.body.append(overlay)
  assert.equal(measureBrowserSlot(slot).visible, false)
  overlay.remove()
  parent.remove()
})
