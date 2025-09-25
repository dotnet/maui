using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Platform;

internal static class SafeAreaExtensions
{
    internal static ISafeAreaView2? GetSafeAreaView2(object? layout) =>
                layout switch
                {
                    ISafeAreaView2 sav2 => sav2,
                    IElementHandler { VirtualView: ISafeAreaView2 virtualSav2 } => virtualSav2,
                    _ => null
                };

    internal static ISafeAreaView? GetSafeAreaView(object? layout) =>
        layout switch
        {
            ISafeAreaView sav => sav,
            IElementHandler { VirtualView: ISafeAreaView virtualSav } => virtualSav,
            _ => null
        };


    internal static SafeAreaRegions GetSafeAreaRegionForEdge(int edge, ICrossPlatformLayout crossPlatformLayout)
    {
        var layout = crossPlatformLayout;
        var safeAreaView2 = GetSafeAreaView2(layout);

        if (safeAreaView2 is not null)
        {
            return safeAreaView2.GetSafeAreaRegionsForEdge(edge);
        }

        var safeAreaView = GetSafeAreaView(layout);
        return safeAreaView?.IgnoreSafeArea == false ? SafeAreaRegions.Container : SafeAreaRegions.None;
    }

    internal static WindowInsetsCompat? ApplyAdjustedSafeAreaInsetsPx(
        WindowInsetsCompat windowInsets,
        ICrossPlatformLayout crossPlatformLayout,
        Context context,
        View view)
    {
        WindowInsetsCompat? newWindowInsets;
        var baseSafeArea = windowInsets.ToSafeAreaInsetsPx(context);
        var keyboardInsets = windowInsets.GetKeyboardInsetsPx(context);
        var isKeyboardShowing = !keyboardInsets.IsEmpty;

        var layout = crossPlatformLayout;
        var safeAreaView2 = GetSafeAreaView2(layout);

        if (safeAreaView2 is not null)
        {
            // Apply safe area selectively per edge based on SafeAreaRegions
            var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0, layout), baseSafeArea.Left, 0, isKeyboardShowing, keyboardInsets);
            var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1, layout), baseSafeArea.Top, 1, isKeyboardShowing, keyboardInsets);
            var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2, layout), baseSafeArea.Right, 2, isKeyboardShowing, keyboardInsets);
            var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3, layout), baseSafeArea.Bottom, 3, isKeyboardShowing, keyboardInsets);

            if (isKeyboardShowing &&
                context.GetActivity()?.Window is Window window &&
                window?.Attributes is WindowManagerLayoutParams attr)
            {
                // If the window is panned from the keyboard being open
                // and there isn't a bottom inset to apply then just don't touch anything
                var softInputMode = attr.SoftInputMode;
                if (softInputMode == SoftInput.AdjustPan
                    && bottom == 0
                )
                {
                    return WindowInsetsCompat.Consumed;
                }
            }

            var globalWindowInsetsListener = context.GetGlobalWindowInsetListener();
            bool hasTrackedViews = globalWindowInsetsListener?.HasTrackedView == true;

            // Check intersection with view bounds to determine which edges actually need padding
            // If we don't have any tracked views yet we will find the first view to pad
            // in order to limit duplicate measures
            if ((view.MeasuredHeight > 0 && view.MeasuredWidth > 0) || !hasTrackedViews)
            {
                if (left > 0 || right > 0 || top > 0 || bottom > 0)
                {
                    if (view.GetParent()?.GetParentOfType<MauiScrollView>() is not null)
                    {
                        return WindowInsetsCompat.Consumed;
                    }

                    if (view.GetParent()?.GetParentOfType<AppBarLayout>() is not null)
                    {
                        return WindowInsetsCompat.Consumed;
                    }
                }
                else
                {
                    return windowInsets;
                }

                // Get view's position on screen
                var viewLocation = new int[2];
                view.GetLocationOnScreen(viewLocation);
                var viewLeft = viewLocation[0];
                var viewTop = viewLocation[1];
                var viewRight = viewLeft + view.Width;
                var viewBottom = viewTop + view.Height;

                // Get actual screen dimensions (including system UI)
                var windowManager = context.GetSystemService(Context.WindowService) as IWindowManager;
                if (windowManager?.DefaultDisplay is not null)
                {
                    var realMetrics = new global::Android.Util.DisplayMetrics();
                    windowManager.DefaultDisplay.GetRealMetrics(realMetrics);
                    var screenWidth = realMetrics.WidthPixels;
                    var screenHeight = realMetrics.HeightPixels;

                    // Calculate actual overlap for each edge
                    // Top: how much the view extends into the top safe area
                    // If the viewTop is < 0 that means that it's most likely
                    // panned off the top of the screen so we don't want to apply any top inset
                    if (top > 0 && viewTop < top && viewTop >= 0)
                    {
                        // Calculate the actual overlap amount
                        top = Math.Min(top - viewTop, top);
                    }
                    else
                    {
                        if (view.MeasuredHeight > 0 || hasTrackedViews)
                            top = 0;
                    }

                    // Bottom: how much the view extends into the bottom safe area
                    if (bottom > 0 && viewBottom > (screenHeight - bottom))
                    {
                        // Calculate the actual overlap amount
                        var bottomEdge = screenHeight - bottom;
                        bottom = Math.Min(viewBottom - bottomEdge, bottom);
                    }
                    else
                    {
                        // if the view height is zero because it hasn't done the first pass
                        // and we don't have any tracked views yet then we will apply the bottom inset
                        if (view.MeasuredHeight > 0 || hasTrackedViews)
                            bottom = 0;
                    }

                    // Left: how much the view extends into the left safe area
                    if (left > 0 && viewLeft < left)
                    {
                        // Calculate the actual overlap amount
                        left = Math.Min(left - viewLeft, left);
                    }
                    else
                    {
                        if (view.MeasuredWidth > 0 || hasTrackedViews)
                            left = 0;
                    }

                    // Right: how much the view extends into the right safe area
                    if (right > 0 && viewRight > (screenWidth - right))
                    {
                        // Calculate the actual overlap amount
                        var rightEdge = screenWidth - right;
                        right = Math.Min(viewRight - rightEdge, right);
                    }
                    else
                    {
                        if (view.MeasuredWidth > 0 || hasTrackedViews)
                            right = 0;
                    }
                }

                // Build new window insets with unconsumed values
                var builder = new WindowInsetsCompat.Builder(windowInsets);

                // Get original insets for each type
                var systemBars = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars());
                var displayCutout = windowInsets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());
                var ime = windowInsets.GetInsets(WindowInsetsCompat.Type.Ime());

                // Calculate what's left after consumption
                // For system bars and display cutout, only consume what we're using
                if (systemBars is not null)
                {
                    var newSystemBarsLeft = left > 0 ? 0 : systemBars.Left;
                    var newSystemBarsTop = top > 0 ? 0 : systemBars.Top;
                    var newSystemBarsRight = right > 0 ? 0 : systemBars.Right;
                    var newSystemBarsBottom = (bottom > 0 || isKeyboardShowing) ? 0 : systemBars.Bottom;

                    builder.SetInsets(WindowInsetsCompat.Type.SystemBars(),
                        AndroidX.Core.Graphics.Insets.Of(newSystemBarsLeft, newSystemBarsTop, newSystemBarsRight, newSystemBarsBottom));
                }

                if (displayCutout is not null)
                {
                    var newCutoutLeft = left > 0 ? 0 : displayCutout.Left;
                    var newCutoutTop = top > 0 ? 0 : displayCutout.Top;
                    var newCutoutRight = right > 0 ? 0 : displayCutout.Right;
                    var newCutoutBottom = (bottom > 0 || isKeyboardShowing) ? 0 : displayCutout.Bottom;

                    builder.SetInsets(WindowInsetsCompat.Type.DisplayCutout(),
                        AndroidX.Core.Graphics.Insets.Of(newCutoutLeft, newCutoutTop, newCutoutRight, newCutoutBottom));
                }

                // For keyboard (IME), only consume if we're handling it
                if (ime is not null && isKeyboardShowing)
                {
                    var newImeBottom = (bottom > 0 && bottom >= keyboardInsets.Bottom) ? 0 : ime.Bottom;
                    builder.SetInsets(WindowInsetsCompat.Type.Ime(),
                        AndroidX.Core.Graphics.Insets.Of(0, 0, 0, newImeBottom));
                }

                newWindowInsets = builder.Build();

                // Apply all insets to content view group
                view.SetPadding((int)left, (int)top, (int)right, (int)bottom);
                if (left > 0 || right > 0 || top > 0 || bottom > 0)
                {
                    globalWindowInsetsListener?.TrackView(view);
                }
            }
            else
            {
                newWindowInsets = windowInsets;
            }
        }
        else
        {
            newWindowInsets = windowInsets;            
        }

        // Fallback: return the base safe area for legacy views
        return newWindowInsets;
    }

    internal static double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea, int edge, bool isKeyboardShowing, SafeAreaPadding keyBoardInsets)
    {
        // Edge-to-edge content - no safe area padding
        if (safeAreaRegion == SafeAreaRegions.None)
        {
            return 0;
        }

        // Handle SoftInput specifically - only apply keyboard insets for bottom edge when keyboard is showing
        if (isKeyboardShowing && edge == 3)
        {
            if (SafeAreaEdges.IsSoftInput(safeAreaRegion))
                return keyBoardInsets.Bottom;

            // if they keyboard is showing then we will just return 0 for the bottom inset
            // because that part of the view is covered by the keyboard so we don't want to pad the view
            return 0;
        }

        // All other regions respect safe area in some form
        // This includes:
        // - Default: Platform default behavior
        // - All: Obey all safe area insets  
        // - Container: Content flows under keyboard but stays out of bars/notch
        // - Any combination of the above flags
        return originalSafeArea;
    }
}
