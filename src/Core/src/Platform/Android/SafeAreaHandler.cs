using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
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

        // Properties for accessing safe area state
        public SafeAreaPadding SafeArea => _safeArea;
        public bool IsKeyboardShowing => _isKeyboardShowing;
        public SafeAreaPadding KeyboardInsets => _keyboardInsets;

        public AndroidX.Core.View.IOnApplyWindowInsetsListener GetWindowInsetsListener()
        {
            return new WindowInsetsListener(this, () => _owner.RequestLayout());
        }

        public void InvalidateParentHierarchyCache()
        {
            _parentHierarchyChanged = true;
            _hasSafeAreaHandlingParent = null;
        }

        public void InvalidateSafeArea()
        {
            _safeAreaInvalidated = true;
        }

        public void UpdateKeyboardState(SafeAreaPadding keyboardInsets, bool isKeyboardShowing)
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

        public bool RespondsToSafeArea()
        {
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
                if (region != SafeAreaRegions.None && region != SafeAreaRegions.Default)
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
                if (SafeAreaEdges.IsContainer(region))
                {
                    _safeAreaInvalidated = true;
                    return true;
                }

                if (SafeAreaEdges.IsSoftInput(region))
                {
                    _safeAreaInvalidated = true;
                    return true;
                }
            }

            return false;
        }

        public bool HasSafeAreaHandlingParent()
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
            int maxDepth = 10; // Limit traversal depth to prevent excessive hierarchy walking
            int currentDepth = 0;

            while (parent != null && currentDepth < maxDepth)
            {
                currentDepth++;

                // Check if parent is a ContentViewGroup or LayoutViewGroup that responds to safe area
                if (parent is ContentViewGroup parentContentGroup)
                {
                    if (parentContentGroup.CrossPlatformLayout is ISafeAreaView2 parentLayout)
                    {
                        // Quick check: if parent has any non-None region, it handles safe area
                        for (int edge = 0; edge < 4; edge++)
                        {
                            if (parentLayout.GetSafeAreaRegionsForEdge(edge) != SafeAreaRegions.None)
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (parent is LayoutViewGroup parentLayoutGroup)
                {
                    if (parentLayoutGroup.CrossPlatformLayout is ISafeAreaView2 parentLayoutView)
                    {
                        // Quick check: if parent has any non-None region, it handles safe area
                        for (int edge = 0; edge < 4; edge++)
                        {
                            if (parentLayoutView.GetSafeAreaRegionsForEdge(edge) != SafeAreaRegions.None)
                            {
                                return true;
                            }
                        }
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

        static double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea)
        {
            // Edge-to-edge content - no safe area padding (Default behaves like None)
            if (safeAreaRegion == SafeAreaRegions.None || safeAreaRegion == SafeAreaRegions.Default)
            {
                return 0;
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

            // SoftInput region - handled separately in GetAdjustedSafeAreaInsets for keyboard-specific logic
            if (SafeAreaEdges.IsSoftInput(safeAreaRegion))
            {
                return originalSafeArea;
            }

            // Any other combination of flags - respect safe area
            return originalSafeArea;
        }

        public Graphics.Rect AdjustForSafeArea(Graphics.Rect bounds)
        {
            ValidateSafeArea();

            if (_safeArea.IsEmpty)
            {
                return bounds;
            }

            return _safeArea.InsetRectF(bounds);
        }

        public SafeAreaPadding GetAdjustedSafeAreaInsets()
        {
            // Get WindowInsets if available
            var rootView = _owner.RootView;
            if (rootView == null)
            {
                return SafeAreaPadding.Empty;
            }

            var windowInsets = ViewCompat.GetRootWindowInsets(rootView);
            if (windowInsets == null)
            {
                return SafeAreaPadding.Empty;
            }

            var baseSafeArea = windowInsets.ToSafeAreaInsets(_context);
            var crossPlatformLayout = _getCrossPlatformLayout();

            // Check if keyboard-aware safe area adjustments are needed (matching iOS logic)
            if (crossPlatformLayout is ISafeAreaView2 safeAreaPage && _isKeyboardShowing)
            {
                // Check if any edge has SafeAreaRegions.SoftInput set
                var needsKeyboardAdjustment = false;
                for (int edge = 0; edge < 4; edge++)
                {
                    var safeAreaRegion = safeAreaPage.GetSafeAreaRegionsForEdge(edge);
                    if (SafeAreaEdges.IsSoftInput(safeAreaRegion))
                    {
                        needsKeyboardAdjustment = true;
                        break;
                    }
                }

                if (needsKeyboardAdjustment)
                {
                    // For SafeAreaRegions.SoftInput: Always pad so content doesn't go under the keyboard
                    // Bottom edge is most commonly affected by keyboard
                    var bottomEdgeRegion = safeAreaPage.GetSafeAreaRegionsForEdge(3); // 3 = bottom edge
                    if (SafeAreaEdges.IsSoftInput(bottomEdgeRegion))
                    {
                        // Use the larger of the current bottom safe area or the keyboard height
                        var adjustedBottom = Math.Max(baseSafeArea.Bottom, _keyboardInsets.Bottom);
                        baseSafeArea = new SafeAreaPadding(baseSafeArea.Left, baseSafeArea.Right, baseSafeArea.Top, adjustedBottom);
                    }
                }
            }

            // Apply safe area selectively per edge based on SafeAreaRegions
            if (crossPlatformLayout is ISafeAreaView2)
            {
                var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0), baseSafeArea.Left);
                var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1), baseSafeArea.Top);
                var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2), baseSafeArea.Right);
                var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3), baseSafeArea.Bottom);

                return new SafeAreaPadding(left, right, top, bottom);
            }

            // Legacy ISafeAreaView handling
            if (crossPlatformLayout is ISafeAreaView sav && sav.IgnoreSafeArea)
            {
                return SafeAreaPadding.Empty;
            }

            return baseSafeArea;
        }

        public bool ValidateSafeArea()
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
        public class WindowInsetsListener : Java.Lang.Object, AndroidX.Core.View.IOnApplyWindowInsetsListener
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
                if (insets != null)
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
}
