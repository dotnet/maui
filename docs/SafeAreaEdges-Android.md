# SafeAreaEdges Android Implementation

This document describes the Android implementation of SafeAreaEdges functionality for .NET MAUI, which provides per-edge safe area control matching the functionality introduced for iOS in PR #30337.

## Overview

The Android SafeAreaEdges implementation allows developers to control how content respects system UI elements like status bars, navigation bars, display cutouts, and the keyboard on a per-edge basis.

## Implementation Details

### Core Components

1. **SafeAreaPadding.cs** - Android equivalent of iOS SafeAreaPadding, handles inset calculations
2. **ContentViewGroup.cs** - Updated to apply SafeAreaEdges logic during layout
3. **LayoutViewGroup.cs** - Updated to apply SafeAreaEdges logic during layout
4. **WindowInsetsExtensions** - Helper methods to convert Android WindowInsets to SafeAreaPadding

### SafeAreaRegions Behavior on Android

- **None**: Content goes edge-to-edge, ignoring all system UI elements
- **All**: Content respects all safe area insets (status bar, navigation bar, display cutouts, keyboard)
- **Container**: Content flows under keyboard but stays out of status/navigation bars and display cutouts
- **SoftInput**: Always pad to avoid keyboard overlap
- **Default**: Platform default behavior (currently same as Container)

### Android-Specific Considerations

#### WindowInsets Integration
- Uses AndroidX.Core.View.WindowInsetsCompat for consistent API across Android versions
- Supports WindowInsetsCompat.Type.SystemBars() for status/navigation bars
- Supports WindowInsetsCompat.Type.DisplayCutout() for display cutouts (API 28+)
- Supports WindowInsetsCompat.Type.Ime() for keyboard insets (API 30+)

#### Pixel Density Handling
- All insets are converted from Android pixels to device-independent units using Context.GetDisplayDensity()
- This ensures consistent behavior across different screen densities

#### WindowInsets Listener
- Each view group sets up a WindowInsetsListener to receive inset changes
- When insets change, the view invalidates and triggers a layout update
- This ensures dynamic updates when keyboard appears/disappears or device orientation changes

### Edge-Specific Logic

The implementation processes each edge (Left, Top, Right, Bottom) independently:

1. **SafeAreaRegions.None**: Returns 0 padding (edge-to-edge)
2. **SafeAreaRegions.SoftInput**: Returns max of original safe area and keyboard inset
3. **SafeAreaRegions.Container**: Returns original safe area (ignores keyboard)
4. **SafeAreaRegions.All/Default**: Returns original safe area

### Layout Integration

Both ContentViewGroup and LayoutViewGroup apply SafeAreaEdges in their OnLayout methods:

1. Check if the cross-platform layout implements ISafeAreaView2
2. Calculate adjusted safe area insets based on SafeAreaEdges configuration
3. Apply insets to the layout bounds before arranging child content

## Usage Examples

### Edge-to-Edge Content
```xml
<Grid SafeAreaEdges="None">
    <!-- Content goes under status bar and navigation bar -->
</Grid>
```

### Respect All Safe Areas
```xml
<Grid SafeAreaEdges="All">
    <!-- Content respects status bar, navigation bar, cutouts, and keyboard -->
</Grid>
```

### Per-Edge Control
```xml
<Grid SafeAreaEdges="{x:Static SafeAreaEdges.None}">
    <!-- Programmatic per-edge control -->
</Grid>
```

```csharp
// Set bottom edge to handle keyboard, keep other edges edge-to-edge
myGrid.SafeAreaEdges = new SafeAreaEdges(
    SafeAreaRegions.None,      // Left
    SafeAreaRegions.None,      // Top  
    SafeAreaRegions.None,      // Right
    SafeAreaRegions.SoftInput  // Bottom
);
```

## Testing

### Unit Tests
- Issue28986Android.cs provides Android-specific UI tests
- Tests verify edge-to-edge vs safe area positioning
- Tests verify keyboard interaction with SoftInput regions

### Manual Testing
- SafeAreaAndroidTest.xaml provides a comprehensive test page
- Includes buttons to test all SafeAreaRegions combinations
- Provides visual feedback for safe area behavior
- Entry field for testing keyboard/SoftInput behavior

## Platform Differences from iOS

1. **System UI Elements**: Android has status bar and navigation bar instead of iOS notch/home indicator
2. **Keyboard Behavior**: Android WindowInsets provide more granular keyboard information
3. **Display Cutouts**: Android supports various cutout shapes beyond iOS notch
4. **Edge Cases**: Android handles orientation changes and foldable devices differently

## Future Enhancements

1. **WindowInsetsAnimation**: Could be integrated for smooth keyboard animations
2. **Navigation Bar Behavior**: Could distinguish between gesture navigation and button navigation
3. **Foldable Support**: Could handle foldable device specific insets
4. **Performance**: Could cache inset calculations for better performance