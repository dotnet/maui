using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Shared safe area handling logic with optimizations for both ContentViewGroup and LayoutViewGroup.
/// Provides caching and performance optimizations to avoid repeated hierarchy traversals.
/// </summary>
internal class SafeAreaHandler
{
    readonly Context _context;
    readonly View _owner;
    readonly Func<ICrossPlatformLayout?> _getCrossPlatformLayout;

    SafeAreaPadding _safeArea = SafeAreaPadding.Empty;
    bool _safeAreaInvalidated = true;

    // Keyboard tracking
    SafeAreaPadding _keyboardInsets = SafeAreaPadding.Empty;
    bool _isKeyboardShowing;

    // Cache for HasSafeAreaHandlingParent to avoid repeated hierarchy traversals
    bool? _hasSafeAreaHandlingParent;
    bool _parentHierarchyChanged = true;

    public SafeAreaHandler(View owner, Context context, Func<ICrossPlatformLayout?> getCrossPlatformLayout)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _getCrossPlatformLayout = getCrossPlatformLayout ?? throw new ArgumentNullException(nameof(getCrossPlatformLayout));
    }

    internal IOnApplyWindowInsetsListener GetWindowInsetsListener()
    {
        return new WindowInsetsListener(this, () => _owner.RequestLayout());
    }

    void InvalidateParentHierarchyCache()
    {
        _parentHierarchyChanged = true;
        _hasSafeAreaHandlingParent = null;
    }

    void InvalidateSafeArea()
    {
        _safeAreaInvalidated = true;
    }

    void UpdateKeyboardState(SafeAreaPadding keyboardInsets, bool isKeyboardShowing)
    {
        var wasKeyboardShowing = _isKeyboardShowing;
        _keyboardInsets = keyboardInsets;
        _isKeyboardShowing = isKeyboardShowing;

        // If keyboard state changed, invalidate safe area
        if (wasKeyboardShowing != _isKeyboardShowing)
        {
            _safeAreaInvalidated = true;
        }
    }

    internal bool RespondsToSafeArea()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(36))
        {
            return false; // Safe area handling only supported on Android 12 (API 31) and above
        }

        var crossPlatformLayout = _getCrossPlatformLayout();

        // Early exit: If this view doesn't implement safe area interfaces, skip entirely
        if (crossPlatformLayout is not ISafeAreaView2 safeAreaLayout)
        {
            return false;
        }

        // Early exit: Check if any edge needs safe area handling before expensive parent check
        bool hasAnySafeAreaEdge = false;
        for (int edge = 0; edge < 4; edge++)
        {
            var region = safeAreaLayout.GetSafeAreaRegionsForEdge(edge);
            if (region != SafeAreaRegions.None)
            {
                hasAnySafeAreaEdge = true;
                break;
            }
        }

        // If no edges need safe area, return false immediately
        if (!hasAnySafeAreaEdge)
        {
            return false;
        }

        // Only now check if parent handles safe area (expensive operation)
        if (HasSafeAreaHandlingParent())
        {
            return false;
        }

        // Final check for specific regions that require safe area handling
        for (int edge = 0; edge < 4; edge++)
        {
            var region = safeAreaLayout.GetSafeAreaRegionsForEdge(edge);
            if (region != SafeAreaRegions.None)
            {
                _safeAreaInvalidated = true;
                return true;
            }
        }

        return false;
    }

    internal bool HasSafeAreaHandlingParent()
    {
        // Use cached result if hierarchy hasn't changed
        if (!_parentHierarchyChanged && _hasSafeAreaHandlingParent.HasValue)
        {
            return _hasSafeAreaHandlingParent.Value;
        }

        // Perform the hierarchy check and cache the result
        _hasSafeAreaHandlingParent = CheckSafeAreaHandlingParentInHierarchy();
        _parentHierarchyChanged = false;

        return _hasSafeAreaHandlingParent.Value;
    }

    bool CheckSafeAreaHandlingParentInHierarchy()
    {
        // Walk up the view hierarchy to check if any parent is handling safe area
        var parent = _owner.Parent;
        int maxDepth = 15; // Increased depth to catch ContentPage
        int currentDepth = 0;

        while (parent is not null && currentDepth < maxDepth)
        {
            currentDepth++;

            if (parent is ICrossPlatformLayoutBacking backing && backing.CrossPlatformLayout is ISafeAreaView2 parentLayout)
            {
                // Check if parent has any explicit (non-Default, non-None) safe area regions
                // Default values shouldn't prevent children from handling safe area
                bool hasExplicitSafeArea = false;
                for (int edge = 0; edge < 4; edge++)
                {
                    var parentRegion = parentLayout.GetSafeAreaRegionsForEdge(edge);
                    // Only consider it as "handling" if it's explicitly set to something other than None or Default
                    if (parentRegion != SafeAreaRegions.None && parentRegion != SafeAreaRegions.Default)
                    {
                        hasExplicitSafeArea = true;
                        break;
                    }
                }
                
                if (hasExplicitSafeArea)
                {
                    return true;
                }
            }

            // Move up to next parent - need to check if it's a View first
            if (parent is View parentView)
            {
                parent = parentView.Parent;
            }
            else
            {
                break;
            }
        }

        return false;
    }

    SafeAreaRegions GetSafeAreaRegionForEdge(int edge)
    {
        var crossPlatformLayout = _getCrossPlatformLayout();

        if (crossPlatformLayout is ISafeAreaView2 safeAreaPage)
        {
            return safeAreaPage.GetSafeAreaRegionsForEdge(edge);
        }

        // Fallback to legacy ISafeAreaView behavior
        if (crossPlatformLayout is ISafeAreaView sav)
        {
            return sav.IgnoreSafeArea ? SafeAreaRegions.None : SafeAreaRegions.Container;
        }

        return SafeAreaRegions.None;
    }

    double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea, int edge)
    {
        // Edge-to-edge content - no safe area padding
        if (safeAreaRegion == SafeAreaRegions.None)
        {
            return 0;
        }

        // Default behavior - apply Android platform defaults
        // On Android, the platform default is to respect system bars (status bar, navigation bar)
        // which is equivalent to applying safe area insets
        if (safeAreaRegion == SafeAreaRegions.Default)
        {
            return originalSafeArea;
        }

        // All should respect all safe area insets
        if (safeAreaRegion == SafeAreaRegions.All)
        {
            return originalSafeArea;
        }

        // Container region - content flows under keyboard but stays out of bars/notch
        if (SafeAreaEdges.IsContainer(safeAreaRegion))
        {
            return originalSafeArea;
        }

        // SoftInput region - special handling for keyboard behavior
        if (SafeAreaEdges.IsSoftInput(safeAreaRegion))
        {
            // For SoftInput: when keyboard is showing, use keyboard inset; when not showing, use navigation bar inset
            if (_isKeyboardShowing)
            {
                // When keyboard is showing, use keyboard insets instead of system bars for affected edges
                if (edge == 3) // Bottom edge - most commonly affected by keyboard
                {
                    // Use the keyboard inset directly - this represents the distance from the bottom of the screen
                    // to the top of the keyboard, which is exactly what we want for bottom padding
                    return _keyboardInsets.Bottom;
                }
                // For other edges, still use original safe area (status bar, etc.)
                return originalSafeArea;
            }
            else
            {
                // When keyboard is not showing, behave like normal safe area
                return originalSafeArea;
            }
        }

        // Any other combination of flags - respect safe area
        return originalSafeArea;
    }

    internal Graphics.Rect AdjustForSafeArea(Graphics.Rect bounds)
    {
        ValidateSafeArea();

        if (_safeArea.IsEmpty)
        {
            return bounds;
        }

        return _safeArea.InsetRectF(bounds);
    }

    SafeAreaPadding GetAdjustedSafeAreaInsets()
    {
        // Get WindowInsets if available
        var rootView = _owner.RootView;
        if (rootView is null)
        {
            return SafeAreaPadding.Empty;
        }

        var windowInsets = ViewCompat.GetRootWindowInsets(rootView);
        if (windowInsets is null)
        {
            return SafeAreaPadding.Empty;
        }

        var baseSafeArea = windowInsets.ToSafeAreaInsets(_context);
        var crossPlatformLayout = _getCrossPlatformLayout();

        // Apply safe area selectively per edge based on SafeAreaRegions
        if (crossPlatformLayout is ISafeAreaView2 safeAreaView2)
        {
            var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0), baseSafeArea.Left, 0);
            var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1), baseSafeArea.Top, 1);
            var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2), baseSafeArea.Right, 2);
            var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3), baseSafeArea.Bottom, 3);

            return new SafeAreaPadding(left, right, top, bottom);
        }

        // Legacy ISafeAreaView handling
        if (crossPlatformLayout is ISafeAreaView sav && sav.IgnoreSafeArea)
        {
            return SafeAreaPadding.Empty;
        }

        return baseSafeArea;
    }

    bool ValidateSafeArea()
    {
        if (!_safeAreaInvalidated)
        {
            return true;
        }

        _safeAreaInvalidated = false;

        var oldSafeArea = _safeArea;
        _safeArea = GetAdjustedSafeAreaInsets();

        return oldSafeArea == _safeArea;
    }

    /// <summary>
    /// Shared WindowInsets listener that can be used by both ContentViewGroup and LayoutViewGroup
    /// </summary>
    public class WindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        private readonly SafeAreaHandler _safeAreaHandler;
        private readonly Action _requestLayout;

        public WindowInsetsListener(SafeAreaHandler safeAreaHandler, Action requestLayout)
        {
            _safeAreaHandler = safeAreaHandler ?? throw new ArgumentNullException(nameof(safeAreaHandler));
            _requestLayout = requestLayout ?? throw new ArgumentNullException(nameof(requestLayout));
        }

        public WindowInsetsCompat? OnApplyWindowInsets(View? v, WindowInsetsCompat? insets)
        {
            _safeAreaHandler.InvalidateSafeArea();

            // Track keyboard state (matching iOS approach)
            if (insets is not null)
            {
                var keyboardInsets = insets.GetKeyboardInsets(_safeAreaHandler._context);
                var wasKeyboardShowing = _safeAreaHandler._isKeyboardShowing;
                var isKeyboardShowing = !keyboardInsets.IsEmpty;

                _safeAreaHandler.UpdateKeyboardState(keyboardInsets, isKeyboardShowing);

                // If keyboard state changed, trigger layout update
                if (wasKeyboardShowing != isKeyboardShowing)
                {
                    _requestLayout();
                }
            }

            // Invalidate parent hierarchy cache when insets change, as this might indicate view hierarchy changes
            _safeAreaHandler.InvalidateParentHierarchyCache();

            _requestLayout();
            return insets;
        }
    }
}