export const minimumSessionWidth = 240

/** Reserve a readable center column, including the incumbent catalog and separator. */
export function sessionWidthLimit(workspaceWidth: number, navigationOpen: boolean): number {
  return Math.max(minimumSessionWidth, Math.min(440, workspaceWidth - (navigationOpen ? 292 : 0) - 8 - 320))
}

export function clampSessionWidth(width: number, maximum: number): number {
  return Math.max(minimumSessionWidth, Math.min(maximum, width))
}
