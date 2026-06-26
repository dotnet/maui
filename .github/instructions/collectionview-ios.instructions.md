---
applyTo:
  - "src/Controls/src/Core/Handlers/Items2/**"
  - "src/Controls/src/Core/Handlers/Items/iOS/**"
  - "src/Controls/src/Core/Handlers/Items/*.iOS.cs"
  - "src/Controls/src/Core/Handlers/Items/*.ios.cs"
---
# CollectionView — iOS/MacCatalyst (Items2/ Handler)

> New iOS/MacCatalyst work targets Items2/. Items/iOS/ is deprecated. See `collectionview-handler-detection.instructions.md` for the full platform→handler mapping.

## UICollectionView Cell Measurement
- Scope cell layout invalidation to the affected cell — avoid `InvalidateLayout()` on the entire collection for single-cell changes
- Custom `UICollectionViewCell` subclasses should handle `MeasureInvalidated` only when the hosted MAUI control actually needs remeasuring
- Cell sizing must stay in sync between the measure pass and the layout pass — out-of-sync causes visual glitches

## UICollectionViewCompositionalLayout
- Layout configuration must match the `ItemsLayout` specification (Linear, Grid, or custom)
- `ItemsLayout` property changes require full layout reconfiguration — partial updates leave stale section configuration
- Group header/footer template changes must invalidate the correct section scope, not the entire layout

## Memory Management
- Use static callback patterns to avoid retain cycles between cells and their hosting handler
- Remove `NSNotificationCenter` observers in `DisconnectHandler`
- Weak references for long-lived observers of short-lived cells — cells are recycled and reused

## Regression Patterns
- Test across empty collection, single item, many items, and with grouping — a fix for one layout scenario routinely breaks another

## Items/ iOS (Deprecated)
- Files in `Handlers/Items/*.iOS.cs` are deprecated — prefer Items2/ for new work
- Only modify Items/ iOS code for explicit legacy maintenance or backward-compatibility fixes

> **Platform file extension rules** (`.ios.cs` vs `.maccatalyst.cs` compilation targets) are defined in `copilot-instructions.md` § Platform-Specific File Extensions. See also `collectionview-handler-detection.instructions.md` for which handler directory to target per platform.
