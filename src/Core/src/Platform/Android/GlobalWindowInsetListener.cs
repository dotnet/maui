using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
    public class GlobalWindowInsetListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        readonly HashSet<AView> _trackedViews = new();

        public WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
        {
            if (insets is null || !insets.HasInsets || v is null)
            {
                return insets;
            }

            // Track this view
            TrackView(v);

            // Handle custom inset views first
            if (v is IHandleWindowInsets customHandler)
            {
                return customHandler.HandleWindowInsets(v, insets);
            }

            // Apply default window insets for standard views
            return ApplyDefaultWindowInsets(v, insets);
        }

        WindowInsetsCompat? ApplyDefaultWindowInsets(AView v, WindowInsetsCompat insets)
        {
            var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
            var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

            var leftInset = Math.Max(systemBars?.Left ?? 0, displayCutout?.Left ?? 0);
            var topInset = Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0);
            var rightInset = Math.Max(systemBars?.Right ?? 0, displayCutout?.Right ?? 0);
            var bottomInset = Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0);

            // Handle special cases
            var appBarLayout = v.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
            var hasNavigationBar = (appBarLayout?.GetChildAt(0) as MaterialToolbar)?.LayoutParameters?.Height > 0;
            if (appBarLayout is not null)
            {
                TrackView(appBarLayout);
                if (hasNavigationBar)
                {
                    appBarLayout.SetPadding(0, topInset, 0, 0);
                }
                else
                {
                    appBarLayout.SetPadding(0, 0, 0, 0);
                }
            }

            // Apply side and bottom insets to root view, but not top
            TrackView(v);

            if (hasNavigationBar)
            {
                v.SetPadding(leftInset, 0, rightInset, bottomInset);
            }
            else
            {
                v.SetPadding(leftInset, 0, rightInset, 0);
            }

            // Create new insets with consumed values
            var newSystemBars = Insets.Of(
                systemBars?.Left ?? 0,
                hasNavigationBar ? 0 : systemBars?.Top ?? 0,
                systemBars?.Right ?? 0,
                hasNavigationBar ? 0 : systemBars?.Bottom ?? 0
            ) ?? Insets.None;

            var newDisplayCutout = Insets.Of(
                displayCutout?.Left ?? 0,
                hasNavigationBar ? 0 : displayCutout?.Top ?? 0,
                displayCutout?.Right ?? 0,
                hasNavigationBar ? 0 : displayCutout?.Bottom ?? 0
            ) ?? Insets.None;

            return new WindowInsetsCompat.Builder(insets)
                ?.SetInsets(WindowInsetsCompat.Type.SystemBars(), newSystemBars)
                ?.SetInsets(WindowInsetsCompat.Type.DisplayCutout(), newDisplayCutout)
                ?.Build() ?? insets;
        }

        void TrackView(AView view)
        {
            _trackedViews.Add(view);
        }

        public void ResetView(AView view)
        {
            if (view is IHandleWindowInsets customHandler)
            {
                customHandler.ResetWindowInsets(view);
            }

            _trackedViews.Remove(view);
        }

        public void ResetAllViews()
        {
            var viewsToReset = new List<AView>(_trackedViews); // Create a copy to avoid modification during enumeration
            foreach (var view in viewsToReset)
            {
                ResetView(view);
            }
        }

        /// <summary>
        /// Resets all tracked descendant views of the specified parent view to their original padding.
        /// This should be called before applying new insets when SafeArea settings change.
        /// </summary>
        /// <param name="parentView">The parent view whose descendants should be reset</param>
        public void ResetDescendantsOfView(AView parentView)
        {
            // Find all tracked views that are descendants of the parent view and reset them
            foreach (var trackedView in _trackedViews.ToArray()) // Use ToArray to avoid modification during enumeration
            {
                if (trackedView != parentView && IsDescendantOf(trackedView, parentView))
                {
                    // Reset to original padding but don't remove from tracking
                    if (trackedView is IHandleWindowInsets customHandler)
                    {
                        customHandler.ResetWindowInsets(trackedView);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a view is a descendant of a parent view
        /// </summary>
        static bool IsDescendantOf(AView? child, AView parent)
        {
            if (child is null || parent is null)
                return false;

            var currentParent = child.Parent;
            while (currentParent is not null)
            {
                if (currentParent == parent)
                    return true;
                currentParent = currentParent.Parent;
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ResetAllViews();
            }
            base.Dispose(disposing);
        }
    }
}

#pragma warning disable RS0016
public static class SafeAreaExtension
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

    internal static SafeAreaPadding GetAdjustedSafeAreaInsets(WindowInsetsCompat windowInsets, ICrossPlatformLayout crossPlatformLayout, Context context)
    {
        var baseSafeArea = windowInsets.ToSafeAreaInsets(context);
        var keyboardInsets = windowInsets.GetKeyboardInsets(context);
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

            return new SafeAreaPadding(left, right, top, bottom);
        }

        // Fallback: return the base safe area for legacy views
        return baseSafeArea;
    }

    internal static double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea, int edge, bool isKeyboardShowing, SafeAreaPadding keyBoardInsets)
    {
        // Edge-to-edge content - no safe area padding
        if (safeAreaRegion == SafeAreaRegions.None)
        {
            return 0;
        }

        // Handle SoftInput specifically - only apply keyboard insets for bottom edge when keyboard is showing
        if (SafeAreaEdges.IsSoftInput(safeAreaRegion) && isKeyboardShowing && edge == 3)
        {
            return keyBoardInsets.Bottom;
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

/// <summary>
/// Extension methods to access the shared GlobalWindowInsetListener instance.
/// </summary>
public static class GlobalWindowInsetListenerExtensions
{
    /// <summary>
    /// Gets the shared GlobalWindowInsetListener instance from the current MauiAppCompatActivity.
    /// </summary>
    /// <param name="context">The Android context</param>
    /// <returns>The shared GlobalWindowInsetListener instance, or null if not available</returns>
    public static GlobalWindowInsetListener? GetGlobalWindowInsetListener(this Context context)
    {
        return context.GetActivity() as MauiAppCompatActivity is MauiAppCompatActivity activity
            ? activity.GlobalWindowInsetListener
            : null;
    }

    /// <summary>
    /// Sets the shared GlobalWindowInsetListener on the specified view.
    /// This ensures all views use the same listener instance for coordinated inset management.
    /// </summary>
    /// <param name="view">The Android view to set the listener on</param>
    /// <param name="context">The Android context to get the listener from</param>
    public static void SetGlobalWindowInsetListener(this View view, Context context)
    {
        var listener = context.GetGlobalWindowInsetListener();
        if (listener is not null)
        {
            ViewCompat.SetOnApplyWindowInsetsListener(view, listener);
        }
    }

    /// <summary>
    /// Resets tracked descendant views and requests fresh window insets.
    /// Call this when SafeArea settings change to avoid double padding issues.
    /// </summary>
    /// <param name="view">The view whose descendants should be reset</param>
    /// <param name="context">The Android context to get the listener from</param>
    public static void ResetDescendantsAndRequestInsets(this View view, Context context)
    {
        var listener = context.GetGlobalWindowInsetListener();
        listener?.ResetDescendantsOfView(view);

        // Request fresh insets to be applied
        ViewCompat.RequestApplyInsets(view);
    }
}