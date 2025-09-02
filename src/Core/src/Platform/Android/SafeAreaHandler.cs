using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Optimized safe area handling for ContentViewGroup and LayoutViewGroup.
/// Uses the CONSUME approach: when a parent handles safe area insets for specific edges,
/// it consumes those insets to prevent them from propagating to children.
/// This eliminates the need for complex parent hierarchy checks.
/// </summary>
internal class SafeAreaHandler(View owner, Context context, Func<ICrossPlatformLayout?> getCrossPlatformLayout)
{
    readonly Context _context = context ?? throw new ArgumentNullException(nameof(context));
    readonly View _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    readonly Func<ICrossPlatformLayout?> _getCrossPlatformLayout = getCrossPlatformLayout ?? throw new ArgumentNullException(nameof(getCrossPlatformLayout));

    SafeAreaPadding _safeArea = SafeAreaPadding.Empty;
    WindowInsetsCompat? _lastReceivedInsets;
    SafeAreaPadding _keyboardInsets = SafeAreaPadding.Empty;
    bool _isKeyboardShowing;
    bool? _scrollViewDescendant;

    internal IOnApplyWindowInsetsListener GetWindowInsetsListener() =>
        new WindowInsetsListener(this, () => _owner.RequestLayout());

    /// <summary>
    /// Forces a re-application of window insets when safe area configuration changes.
    /// This ensures OnApplyWindowInsets is called before measure and arrange.
    /// </summary>
    internal void InvalidateWindowInsets()
    {
        if (_lastReceivedInsets is not null)
        {
            // Re-apply the last received insets to trigger OnApplyWindowInsets
            ViewCompat.DispatchApplyWindowInsets(_owner, _lastReceivedInsets);
        }
        else
        {
            // Request fresh insets from the system
            ViewCompat.RequestApplyInsets(_owner);
        }
    }

    void UpdateKeyboardState(SafeAreaPadding keyboardInsets, bool isKeyboardShowing)
    {
        _keyboardInsets = keyboardInsets;
        _isKeyboardShowing = isKeyboardShowing;
    }

    internal Rect AdjustForSafeArea(Rect bounds)
    {
        ValidateSafeArea();
        return _safeArea.IsEmpty ? bounds : _safeArea.InsetRectF(bounds);
    }

    static ISafeAreaView2? GetSafeAreaView2(object? layout) =>
        layout switch
        {
            ISafeAreaView2 sav2 => sav2,
            IElementHandler { VirtualView: ISafeAreaView2 virtualSav2 } => virtualSav2,
            _ => null
        };

    static ISafeAreaView? GetSafeAreaView(object? layout) =>
        layout switch
        {
            ISafeAreaView sav => sav,
            IElementHandler { VirtualView: ISafeAreaView virtualSav } => virtualSav,
            _ => null
        };

    internal bool RespondsToSafeArea()
    {
        // Cache the ScrollView descendant check for performance
        if (!_scrollViewDescendant.HasValue)
        {
            _scrollViewDescendant = _owner.Parent?.GetParentOfType<NestedScrollView>() is not null;
        }

        // ScrollView descendants don't respond to safe area
        return !_scrollViewDescendant.Value;
    }


    SafeAreaRegions GetSafeAreaRegionForEdge(int edge)
    {
        var layout = _getCrossPlatformLayout();
        var safeAreaView2 = GetSafeAreaView2(layout);

        if (safeAreaView2 is not null)
        {
            return safeAreaView2.GetSafeAreaRegionsForEdge(edge);
        }

        var safeAreaView = GetSafeAreaView(layout);
        return safeAreaView?.IgnoreSafeArea == false ? SafeAreaRegions.Container : SafeAreaRegions.None;
    }

    SafeAreaPadding GetAdjustedSafeAreaInsets()
    {
        if (_lastReceivedInsets is null)
        {
            return SafeAreaPadding.Empty;
        }

        var baseSafeArea = _lastReceivedInsets.ToSafeAreaInsets(_context);
        var layout = _getCrossPlatformLayout();
        var safeAreaView2 = GetSafeAreaView2(layout);

        if (safeAreaView2 is not null)
        {
            // Apply safe area selectively per edge based on SafeAreaRegions
            var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0), baseSafeArea.Left, 0);
            var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1), baseSafeArea.Top, 1);
            var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2), baseSafeArea.Right, 2);
            var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3), baseSafeArea.Bottom, 3);

            return new SafeAreaPadding(left, right, top, bottom);
        }

        // Fallback: return the base safe area for legacy views
        return baseSafeArea;
    }

    double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea, int edge)
    {
        // Edge-to-edge content - no safe area padding
        if (safeAreaRegion == SafeAreaRegions.None)
        {
            return 0;
        }

        // Handle SoftInput specifically - only apply keyboard insets for bottom edge when keyboard is showing
        if (SafeAreaEdges.IsSoftInput(safeAreaRegion) && _isKeyboardShowing && edge == 3)
        {
            return _keyboardInsets.Bottom;
        }

        // All other regions respect safe area in some form
        // This includes:
        // - Default: Platform default behavior
        // - All: Obey all safe area insets  
        // - Container: Content flows under keyboard but stays out of bars/notch
        // - Any combination of the above flags
        return originalSafeArea;
    }

    bool ValidateSafeArea()
    {
        var oldSafeArea = _safeArea;
        _safeArea = GetAdjustedSafeAreaInsets();
        return oldSafeArea == _safeArea;
    }

    /// <summary>
    /// Updates safe area configuration and triggers window insets re-application if needed.
    /// Call this when safe area edge configuration changes.
    /// </summary>
    internal void UpdateSafeAreaConfiguration()
    {
        // Clear cached ScrollView descendant check
        _scrollViewDescendant = null;

        // Force re-calculation of safe area
        var oldSafeArea = _safeArea;
        _safeArea = GetAdjustedSafeAreaInsets();

        // Always invalidate insets when configuration changes, regardless of whether
        // the calculated safe area changed. This ensures proper handling of:
        // - Orientation changes where the same safe area values might apply differently
        // - Soft input behavior changes that need immediate re-evaluation
        // - Multiple sequential configuration changes that need consistent behavior
        InvalidateWindowInsets();
    }

    internal static bool HasAnyRegions(ISafeAreaView2 sav2)
    {
        for (int edge = 0; edge < 4; edge++)
        {
            if (sav2.GetSafeAreaRegionsForEdge(edge) != SafeAreaRegions.None)
            {
                return true;
            }
        }
        return false;
    }

#nullable disable
    /// <summary>
    /// WindowInsets listener for ContentViewGroup and LayoutViewGroup.
    /// </summary>
    public class WindowInsetsListener(SafeAreaHandler handler, Action requestLayout) : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        readonly WeakReference<SafeAreaHandler> _handlerRef = new(handler ?? throw new ArgumentNullException(nameof(handler)));
        readonly WeakReference<Action> _requestLayoutRef = new(requestLayout ?? throw new ArgumentNullException(nameof(requestLayout)));

        public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets)
        {
            // Early return for null insets
            if (insets is null)
            {
                return insets;
            }

            // Get handler and requestLayout from weak references
            if (!_handlerRef.TryGetTarget(out var handler) || !_requestLayoutRef.TryGetTarget(out var requestLayout))
            {
                // Handler or requestLayout has been garbage collected, return insets unchanged
                return insets;
            }

            handler._lastReceivedInsets = insets;

            // Handle keyboard state changes
            var keyboardInsets = insets.GetKeyboardInsets(handler._context);
            var isKeyboardShowing = !keyboardInsets.IsEmpty;
            var wasKeyboardShowing = handler._isKeyboardShowing;

            handler.UpdateKeyboardState(keyboardInsets, isKeyboardShowing);

            // Request layout if keyboard state changed
            if (wasKeyboardShowing != isKeyboardShowing)
            {
                requestLayout();
            }

            requestLayout();

            var crossPlatformLayout = handler._getCrossPlatformLayout();

            // Use RespondsToSafeArea to check if this view should handle safe area
            if (!handler.RespondsToSafeArea())
            {
                // For deeper descendants of ScrollView, consume all insets to prevent double-application
                return ConsumeAllInsets(insets);
            }

            if (handler._getCrossPlatformLayout() is ISafeAreaView2 sav2 && HasAnyRegions(sav2))
            {
                return ConsumeAllInsets(insets);
            }

            // This view is not a ScrollView descendant, pass insets through unchanged
            return insets;
        }

        static WindowInsetsCompat ConsumeAllInsets(WindowInsetsCompat insets)
        {
            // Consume all insets to prevent safe area handling for ScrollView descendants
            return new WindowInsetsCompat.Builder(insets)
                .SetInsets(WindowInsetsCompat.Type.SystemBars(), AndroidX.Core.Graphics.Insets.None)
                .SetInsets(WindowInsetsCompat.Type.DisplayCutout(), AndroidX.Core.Graphics.Insets.None)
                .SetInsets(WindowInsetsCompat.Type.Ime(), AndroidX.Core.Graphics.Insets.None)
                .Build();
        }
    }
}
