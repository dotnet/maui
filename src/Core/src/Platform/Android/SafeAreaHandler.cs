using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
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
    bool _safeAreaInvalidated = true;
    WindowInsetsCompat? _lastReceivedInsets;
    SafeAreaPadding _keyboardInsets = SafeAreaPadding.Empty;
    bool _isKeyboardShowing;
    // Cached values for performance
    ICrossPlatformLayout? _cachedLayout;
    ISafeAreaView2? _cachedSafeAreaView2;
    ISafeAreaView? _cachedSafeAreaView;
    bool _layoutCacheValid;
    internal IOnApplyWindowInsetsListener GetWindowInsetsListener() =>
        new WindowInsetsListener(this, () => _owner.RequestLayout());
    void InvalidateSafeArea()
    {
        _safeAreaInvalidated = true;
        _layoutCacheValid = false;
    }

    void UpdateKeyboardState(SafeAreaPadding keyboardInsets, bool isKeyboardShowing)
    {
        if (_isKeyboardShowing != isKeyboardShowing)
        {
            _safeAreaInvalidated = true;
        }
        _keyboardInsets = keyboardInsets;
        _isKeyboardShowing = isKeyboardShowing;
    }

    void EnsureCachedLayout()
    {
        if (_layoutCacheValid)
        {
            return;
        }

        _cachedLayout = _getCrossPlatformLayout();
        _cachedSafeAreaView2 = GetSafeAreaView2(_cachedLayout);
        _cachedSafeAreaView = _cachedSafeAreaView2 == null ? GetSafeAreaView(_cachedLayout) : null;
        _layoutCacheValid = true;
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
        EnsureCachedLayout();
        if (_cachedSafeAreaView2 is not null)
        {
            // Check if any edge has non-None regions (optimized loop)
            for (int edge = 0; edge < 4; edge++)
            {
                if (_cachedSafeAreaView2.GetSafeAreaRegionsForEdge(edge) != SafeAreaRegions.None)
                {
                    _safeAreaInvalidated = true;
                    return true;
                }
            }
            return false;
        }
        if (_cachedSafeAreaView is not null)
        {
            return !_cachedSafeAreaView.IgnoreSafeArea;
        }
        return false;
    }

    SafeAreaRegions GetSafeAreaRegionForEdge(int edge)
    {
        EnsureCachedLayout();
        return _cachedSafeAreaView2?.GetSafeAreaRegionsForEdge(edge)
            ?? (_cachedSafeAreaView?.IgnoreSafeArea == false ? SafeAreaRegions.Container : SafeAreaRegions.None);
    }

    double GetSafeAreaForEdge(SafeAreaRegions region, double original, int edge)
    {
        if (region == SafeAreaRegions.None)
        {
            return 0;
        }

        if (SafeAreaEdges.IsSoftInput(region) && _isKeyboardShowing && edge == 3)
        {
            return _keyboardInsets.Bottom;
        }
        return original;
    }

    internal Rect AdjustForSafeArea(Rect bounds)
    {
        ValidateSafeArea();
        return _safeArea.IsEmpty ? bounds : _safeArea.InsetRectF(bounds);
    }

    SafeAreaPadding GetAdjustedSafeAreaInsets()
    {
        // Use the insets that were actually received by this view's OnApplyWindowInsets
        // If no insets were received, or they were consumed by a parent, return empty
        if (_lastReceivedInsets is null)
            return SafeAreaPadding.Empty;
        var baseSafeArea = _lastReceivedInsets.ToSafeAreaInsets(_context);
        EnsureCachedLayout();
        if (_cachedSafeAreaView2 is not null)
        {
            return new SafeAreaPadding(
                GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0), baseSafeArea.Left, 0),
                GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2), baseSafeArea.Right, 2),
                GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1), baseSafeArea.Top, 1),
                GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3), baseSafeArea.Bottom, 3)
            );
        }
        return _cachedSafeAreaView?.IgnoreSafeArea == true ? SafeAreaPadding.Empty : baseSafeArea;
    }

    bool ValidateSafeArea()
    {
        if (!_safeAreaInvalidated)
            return true;
        _safeAreaInvalidated = false;
        var oldSafeArea = _safeArea;
        _safeArea = GetAdjustedSafeAreaInsets();
        return oldSafeArea == _safeArea;
    }

#nullable disable
    /// <summary>
    /// WindowInsets listener for ContentViewGroup and LayoutViewGroup.
    /// </summary>
    public class WindowInsetsListener(SafeAreaHandler handler, Action requestLayout) : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        readonly SafeAreaHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        readonly Action _requestLayout = requestLayout ?? throw new ArgumentNullException(nameof(requestLayout));
        public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets)
        {
            _handler.InvalidateSafeArea();
            // Store the insets that this specific view received
            _handler._lastReceivedInsets = insets;
            if (insets is not null)
            {
                var keyboardInsets = insets.GetKeyboardInsets(_handler._context);
                var isKeyboardShowing = !keyboardInsets.IsEmpty;
                var wasKeyboardShowing = _handler._isKeyboardShowing;
                _handler.UpdateKeyboardState(keyboardInsets, isKeyboardShowing);
                if (wasKeyboardShowing != isKeyboardShowing)
                {
                    _requestLayout();
                }
            }

            _requestLayout();
            // Early return for null insets
            if (insets is null)
            {
                return insets;
            }

            // Early return if not responding to safe area
            if (!_handler.RespondsToSafeArea())
            {
                return insets;
            }

            _handler.EnsureCachedLayout();
            // Handle ISafeAreaView2 with optimized inset consumption
            if (_handler._cachedSafeAreaView2 is ISafeAreaView2 safeAreaLayout)
            {
                var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
                var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());
                var ime = insets.GetInsets(WindowInsetsCompat.Type.Ime());
                // Get regions once and reuse
                var regions = new SafeAreaRegions[4];
                for (int i = 0; i < 4; i++)
                {
                    regions[i] = safeAreaLayout.GetSafeAreaRegionsForEdge(i);
                }
                // Create new insets based on consumed edges
                var newSystemBars = CreateConsumedInsets(systemBars, regions) ?? AndroidX.Core.Graphics.Insets.None;
                var newDisplayCutout = CreateConsumedInsets(displayCutout, regions) ?? AndroidX.Core.Graphics.Insets.None;
                // Handle IME separately for bottom edge
                var newIme = AndroidX.Core.Graphics.Insets.Of(
                    ime?.Left ?? 0,
                    ime?.Top ?? 0,
                    ime?.Right ?? 0,
                    SafeAreaEdges.IsSoftInput(regions[3]) ? 0 : (ime?.Bottom ?? 0)
                );
                return new WindowInsetsCompat.Builder(insets)
                    .SetInsets(WindowInsetsCompat.Type.SystemBars(), newSystemBars)
                    .SetInsets(WindowInsetsCompat.Type.DisplayCutout(), newDisplayCutout)
                    .SetInsets(WindowInsetsCompat.Type.Ime(), newIme)
                    .Build();
            }

            // For other cases, consume all insets
            return new WindowInsetsCompat.Builder(insets)
                .SetInsets(WindowInsetsCompat.Type.SystemBars(), AndroidX.Core.Graphics.Insets.None)
                .SetInsets(WindowInsetsCompat.Type.DisplayCutout(), AndroidX.Core.Graphics.Insets.None)
                .SetInsets(WindowInsetsCompat.Type.Ime(), AndroidX.Core.Graphics.Insets.None)
                .Build();
        }

        static AndroidX.Core.Graphics.Insets CreateConsumedInsets(AndroidX.Core.Graphics.Insets original, SafeAreaRegions[] regions)
        {
            if (original is null)
            {
                return AndroidX.Core.Graphics.Insets.None;
            }
            return AndroidX.Core.Graphics.Insets.Of(
                regions[0] != SafeAreaRegions.None ? 0 : original.Left,   // Left
                regions[1] != SafeAreaRegions.None ? 0 : original.Top,    // Top
                regions[2] != SafeAreaRegions.None ? 0 : original.Right,  // Right
                regions[3] != SafeAreaRegions.None ? 0 : original.Bottom  // Bottom
            );
        }
    }
}
