using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
    internal class GlobalWindowInsetListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        readonly HashSet<AView> _trackedViews = [];

        public WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
        {
            if (insets is null || !insets.HasInsets || v is null)
            {
                return insets;
            }

            // Handle custom inset views first
            if (v is IHandleWindowInsets customHandler)
            {
                return customHandler.HandleWindowInsets(v, insets);
            }

            // Apply default window insets for standard views
            return ApplyDefaultWindowInsets(v, insets);
        }

        static WindowInsetsCompat? ApplyDefaultWindowInsets(AView v, WindowInsetsCompat insets)
        {
            var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
            var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

            var leftInset = Math.Max(systemBars?.Left ?? 0, displayCutout?.Left ?? 0);
            var topInset = Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0);
            var rightInset = Math.Max(systemBars?.Right ?? 0, displayCutout?.Right ?? 0);
            var bottomInset = Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0);

            // Handle special cases
            var appBarLayout = v.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);

            if (appBarLayout is null && v is ViewGroup group)
            {
                if (group.ChildCount > 0 && group.GetChildAt(0) is AppBarLayout firstChildAppBar)
                {
                    appBarLayout = firstChildAppBar;
                }
                else if (group.ChildCount > 1 && group.GetChildAt(1) is AppBarLayout secondChildAppBar)
                {
                    appBarLayout = secondChildAppBar;
                }
            }

            var appBarLayoutContainsSomething = appBarLayout?.GetChildAt(0)?.MeasuredHeight > 0;
            if (appBarLayout is not null)
            {
                if (appBarLayoutContainsSomething)
                {
                    appBarLayout.SetPadding(0, topInset, 0, 0);
                }
                else
                {
                    appBarLayout.SetPadding(0, 0, 0, 0);
                }
            }

            // Create new insets with consumed values
            var newSystemBars = Insets.Of(
                systemBars?.Left ?? 0,
                appBarLayoutContainsSomething ? 0 : systemBars?.Top ?? 0,
                systemBars?.Right ?? 0,
                systemBars?.Bottom ?? 0
            ) ?? Insets.None;

            var newDisplayCutout = Insets.Of(
                displayCutout?.Left ?? 0,
                appBarLayoutContainsSomething ? 0 : displayCutout?.Top ?? 0,
                displayCutout?.Right ?? 0,
                displayCutout?.Bottom ?? 0
            ) ?? Insets.None;

            return new WindowInsetsCompat.Builder(insets)
                ?.SetInsets(WindowInsetsCompat.Type.SystemBars(), newSystemBars)
                ?.SetInsets(WindowInsetsCompat.Type.DisplayCutout(), newDisplayCutout)
                ?.Build() ?? insets;
        }

        public void TrackView(AView view)
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
        /// <param name="view">The parent view whose descendants should be reset</param>
        public void ResetAppliedSafeAreas(AView view)
        {
            ResetView(view);

            // Find all tracked views that are descendants of the parent view and reset them
            foreach (var trackedView in _trackedViews.ToArray()) // Use ToArray to avoid modification during enumeration
            {
                if (IsDescendantOf(trackedView, view))
                {
                    ResetView(trackedView);
                }
            }
        }

        /// <summary>
        /// Checks if a view is a descendant of a parent view
        /// </summary>
        static bool IsDescendantOf(AView? child, AView parent)
        {
            if (child is null || parent is null)
            {
                return false;
            }

            var currentParent = child.Parent;
            while (currentParent is not null)
            {
                if (currentParent == parent)
                {
                    return true;
                }

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

/// <summary>
/// Extension methods to access the shared GlobalWindowInsetListener instance.
/// </summary>
internal static class GlobalWindowInsetListenerExtensions
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
    /// Removes the GlobalWindowInsetListener from the specified view and resets its tracked state.
    /// This should be called when a view is being detached to ensure proper cleanup.
    /// </summary>
    /// <param name="view">The Android view to remove the listener from</param>
    /// <param name="context">The Android context to get the listener from</param>
    public static void RemoveGlobalWindowInsetListener(this View view, Context context)
    {
        var listener = context.GetGlobalWindowInsetListener();
        listener?.ResetView(view);
        ViewCompat.SetOnApplyWindowInsetsListener(view, null);
    }
}