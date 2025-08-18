using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Optimized safe area handling for ContentViewGroup and LayoutViewGroup.
/// Caches hierarchy checks and minimizes repeated calculations.
/// </summary>
internal class SafeAreaHandler(View owner, Context context, Func<ICrossPlatformLayout?> getCrossPlatformLayout)
{
    readonly Context _context = context ?? throw new ArgumentNullException(nameof(context));
    readonly View _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    readonly Func<ICrossPlatformLayout?> _getCrossPlatformLayout = getCrossPlatformLayout ?? throw new ArgumentNullException(nameof(getCrossPlatformLayout));

    SafeAreaPadding _safeArea = SafeAreaPadding.Empty;
    bool _safeAreaInvalidated = true;

    SafeAreaPadding _keyboardInsets = SafeAreaPadding.Empty;
    bool _isKeyboardShowing;

    bool? _hasSafeAreaHandlingParent;
    bool _parentHierarchyChanged = true;

    internal IOnApplyWindowInsetsListener GetWindowInsetsListener() =>
        new WindowInsetsListener(this, () => _owner.RequestLayout());

    void InvalidateParentHierarchyCache()
    {
        _parentHierarchyChanged = true;
        _hasSafeAreaHandlingParent = null;
    }

    void InvalidateSafeArea() => _safeAreaInvalidated = true;

    void UpdateKeyboardState(SafeAreaPadding keyboardInsets, bool isKeyboardShowing)
    {
        if (_isKeyboardShowing != isKeyboardShowing)
        {
            _safeAreaInvalidated = true;
        }

        _keyboardInsets = keyboardInsets;
        _isKeyboardShowing = isKeyboardShowing;
    }

    static ISafeAreaView2? GetSafeAreaView2(object? layout)
    {
        if (layout is ISafeAreaView2 sav2)
        {
            return sav2;
        }

        if (layout is IElementHandler handler && handler.VirtualView is ISafeAreaView2 virtualSav2)
        {
            return virtualSav2;
        }

        return null;
    }

    static ISafeAreaView? GetSafeAreaView(object? layout)
    {
        if (layout is ISafeAreaView sav)
        {
            return sav;
        }

        if (layout is IElementHandler handler && handler.VirtualView is ISafeAreaView virtualSav)
        {
            return virtualSav;
        }

        return null;
    }

    internal bool RespondsToSafeArea()
    {
        // Removed high API level gate so that safe area handling (system bars, cutouts, IME)
        // can function on currently supported Android versions.
        var layout = _getCrossPlatformLayout();

        var safeAreaLayout = GetSafeAreaView2(layout);
        if (safeAreaLayout is null)
        {
            // Fallback: check for legacy ISafeAreaView
            var legacySafeAreaView = GetSafeAreaView(layout);
            if (legacySafeAreaView is not null)
            {
                return !legacySafeAreaView.IgnoreSafeArea && !HasSafeAreaHandlingParent();
            }
            return false;
        }

        // Check if this view needs to handle safe area on any edge
        // considering per-edge parent blocking
        bool respondsToAnyEdge = false;

        for (int edge = 0; edge < 4; edge++)
        {
            var region = safeAreaLayout.GetSafeAreaRegionsForEdge(edge);
            if (region != SafeAreaRegions.None)
            {
                // Check if a parent is handling safe area for this specific edge
                if (!CheckSafeAreaHandlingParentInHierarchyForEdge(edge))
                {
                    respondsToAnyEdge = true;
                    break; // At least one edge can be handled
                }
            }
        }

        if (respondsToAnyEdge)
        {
            _safeAreaInvalidated = true;
        }

        return respondsToAnyEdge;
    }

    internal bool HasSafeAreaHandlingParent()
    {
        if (!_parentHierarchyChanged && _hasSafeAreaHandlingParent.HasValue)
        {
            return _hasSafeAreaHandlingParent.Value;
        }

        _hasSafeAreaHandlingParent = CheckSafeAreaHandlingParentInHierarchy();
        _parentHierarchyChanged = false;
        return _hasSafeAreaHandlingParent.Value;
    }

    bool CheckSafeAreaHandlingParentInHierarchy()
    {
        var parent = _owner.Parent;
        int depth = 0, maxDepth = 15;

        while (parent != null && depth++ < maxDepth)
        {
            if (parent is ICrossPlatformLayoutBacking backing &&
                GetSafeAreaView2(backing.CrossPlatformLayout) is ISafeAreaView2 parentLayout)
            {
                // Check if the parent has any non-None safe area regions
                // But allow children to handle their own safe area when parent is None for all edges
                bool hasAnyNonNoneEdge = false;
                for (int edge = 0; edge < 4; edge++)
                {
                    var region = parentLayout.GetSafeAreaRegionsForEdge(edge);
                    if (region != SafeAreaRegions.None)
                    {
                        hasAnyNonNoneEdge = true;
                        break;
                    }
                }

                // Only return true if parent has at least one non-None edge
                // This preserves the original logic but allows better debugging
                if (hasAnyNonNoneEdge)
                {
                    return true;
                }
            }
            parent = parent is View pv ? pv.Parent : null;
        }
        return false;
    }

    bool CheckSafeAreaHandlingParentInHierarchyForEdge(int edge)
    {
        var parent = _owner.Parent;
        int depth = 0, maxDepth = 15;

        while (parent != null && depth++ < maxDepth)
        {
            if (parent is ICrossPlatformLayoutBacking backing &&
                GetSafeAreaView2(backing.CrossPlatformLayout) is ISafeAreaView2 parentLayout)
            {
                var parentRegion = parentLayout.GetSafeAreaRegionsForEdge(edge);

                // If parent has None for this edge, it allows children to handle their own safe area
                if (parentRegion == SafeAreaRegions.None)
                {
                    parent = parent is View parentView ? parentView.Parent : null;
                    continue;
                }

                // If parent has any non-None region for this edge, it's handling safe area
                if (parentRegion != SafeAreaRegions.None)
                {
                    return true;
                }
            }
            parent = parent is View parentView2 ? parentView2.Parent : null;
        }
        return false;
    }

    SafeAreaRegions GetSafeAreaRegionForEdge(int edge)
    {
        var layout = _getCrossPlatformLayout();
        var safeAreaPage = GetSafeAreaView2(layout);
        if (safeAreaPage is not null)
        {
            return safeAreaPage.GetSafeAreaRegionsForEdge(edge);
        }

        if (layout is ISafeAreaView sav)
        {
            return sav.IgnoreSafeArea ? SafeAreaRegions.None : SafeAreaRegions.Container;
        }

        return SafeAreaRegions.None;
    }

    double GetSafeAreaForEdge(SafeAreaRegions region, double original, int edge)
    {
        if (region == SafeAreaRegions.None)
        {
            return 0;
        }

        if (region == SafeAreaRegions.Default || region == SafeAreaRegions.All || SafeAreaEdges.IsContainer(region))
        {
            return original;
        }

        if (SafeAreaEdges.IsSoftInput(region))
        {
            if (_isKeyboardShowing && edge == 3)
            {
                return _keyboardInsets.Bottom;
            }

            return original;
        }
        return original;
    }

    double GetSafeAreaForEdgeWithParentCheck(SafeAreaRegions region, double original, int edge)
    {
        // If a parent is handling safe area for this edge, don't apply safe area here
        if (CheckSafeAreaHandlingParentInHierarchyForEdge(edge))
        {
            return 0;
        }

        return GetSafeAreaForEdge(region, original, edge);
    }

    internal Rect AdjustForSafeArea(Rect bounds)
    {
        ValidateSafeArea();
        return _safeArea.IsEmpty ? bounds : _safeArea.InsetRectF(bounds);
    }

    SafeAreaPadding GetAdjustedSafeAreaInsets()
    {
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
        var layout = _getCrossPlatformLayout();
        if (GetSafeAreaView2(layout) is ISafeAreaView2 safeAreaLayout)
        {
            return new SafeAreaPadding(
                GetSafeAreaForEdgeWithParentCheck(GetSafeAreaRegionForEdge(0), baseSafeArea.Left, 0),
                GetSafeAreaForEdgeWithParentCheck(GetSafeAreaRegionForEdge(2), baseSafeArea.Right, 2),
                GetSafeAreaForEdgeWithParentCheck(GetSafeAreaRegionForEdge(1), baseSafeArea.Top, 1),
                GetSafeAreaForEdgeWithParentCheck(GetSafeAreaRegionForEdge(3), baseSafeArea.Bottom, 3)
            );
        }

        var sav = GetSafeAreaView(layout);
        if (sav is not null && sav.IgnoreSafeArea)
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
    /// WindowInsets listener for ContentViewGroup and LayoutViewGroup.
    /// </summary>
    public class WindowInsetsListener(SafeAreaHandler handler, Action requestLayout) : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        readonly SafeAreaHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        readonly Action _requestLayout = requestLayout ?? throw new ArgumentNullException(nameof(requestLayout));

        public WindowInsetsCompat? OnApplyWindowInsets(View? v, WindowInsetsCompat? insets)
        {
            _handler.InvalidateSafeArea();

            if (insets != null)
            {
                var keyboardInsets = insets.GetKeyboardInsets(_handler._context);
                var wasKeyboardShowing = _handler._isKeyboardShowing;
                var isKeyboardShowing = !keyboardInsets.IsEmpty;

                _handler.UpdateKeyboardState(keyboardInsets, isKeyboardShowing);

                if (wasKeyboardShowing != isKeyboardShowing)
                {
                    _requestLayout();
                }
            }

            _handler.InvalidateParentHierarchyCache();
            _requestLayout();

            // Consume insets if this view responds to safe area to prevent propagation to children
            if (insets != null && _handler.RespondsToSafeArea())
            {
                var layout = _handler._getCrossPlatformLayout();
                if (GetSafeAreaView2(layout) is ISafeAreaView2 safeAreaLayout)
                {
                    // Check which edges we're handling and consume those insets
                    var systemBarsInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
                    var displayCutoutInsets = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());
                    var imeInsets = insets.GetInsets(WindowInsetsCompat.Type.Ime());

                    var leftRegion = safeAreaLayout.GetSafeAreaRegionsForEdge(0);
                    var topRegion = safeAreaLayout.GetSafeAreaRegionsForEdge(1);
                    var rightRegion = safeAreaLayout.GetSafeAreaRegionsForEdge(2);
                    var bottomRegion = safeAreaLayout.GetSafeAreaRegionsForEdge(3);

                    // Calculate which insets to consume based on SafeAreaRegions
                    var consumeLeft = (leftRegion != SafeAreaRegions.None) ?
                        (systemBarsInsets?.Left ?? 0) + (displayCutoutInsets?.Left ?? 0) : 0;
                    var consumeTop = (topRegion != SafeAreaRegions.None) ?
                        (systemBarsInsets?.Top ?? 0) + (displayCutoutInsets?.Top ?? 0) : 0;
                    var consumeRight = (rightRegion != SafeAreaRegions.None) ?
                        (systemBarsInsets?.Right ?? 0) + (displayCutoutInsets?.Right ?? 0) : 0;
                    var consumeBottom = (bottomRegion != SafeAreaRegions.None) ?
                        (systemBarsInsets?.Bottom ?? 0) + (displayCutoutInsets?.Bottom ?? 0) +
                        (SafeAreaEdges.IsSoftInput(bottomRegion) ? (imeInsets?.Bottom ?? 0) : 0) : 0;                    // Create new insets with consumed values subtracted
                    var consumedInsets = AndroidX.Core.Graphics.Insets.Of(consumeLeft, consumeTop, consumeRight, consumeBottom);

                    // Use Inset method to consume the insets
                    return insets.Inset(consumedInsets);
                }
            }

            return insets;
        }
    }
}
