using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// Registry entry for tracking CoordinatorLayout instances and their associated listeners
    /// </summary>
    internal record CoordinatorLayoutEntry(WeakReference<CoordinatorLayout> Layout, GlobalWindowInsetListener Listener);

    internal class GlobalWindowInsetListener : WindowInsetsAnimationCompat.Callback, IOnApplyWindowInsetsListener
    {
        readonly HashSet<AView> _trackedViews = [];
        bool IsImeAnimating { get; set; }

        AView? _pendingView;

        // Static tracking for CoordinatorLayouts that have local inset listeners
        // No locking needed since this runs on UI thread
        static readonly List<CoordinatorLayoutEntry> _registeredCoordinatorLayouts = new();

        /// <summary>
        /// Registers a CoordinatorLayout to use this local listener instead of the global one
        /// </summary>
        internal void RegisterCoordinatorLayout(CoordinatorLayout coordinatorLayout)
        {
            // Clean up dead references and check for existing registration
            for (int i = _registeredCoordinatorLayouts.Count - 1; i >= 0; i--)
            {
                var entry = _registeredCoordinatorLayouts[i];
                if (!entry.Layout.TryGetTarget(out var existingLayout))
                {
                    _registeredCoordinatorLayouts.RemoveAt(i);
                }
                else if (existingLayout == coordinatorLayout)
                {
                    // Already registered, no need to add again
                    return;
                }
            }

            // Add this layout to the registry
            _registeredCoordinatorLayouts.Add(new CoordinatorLayoutEntry(new WeakReference<CoordinatorLayout>(coordinatorLayout), this));
        }

        /// <summary>
        /// Unregisters a CoordinatorLayout from using this local listener
        /// </summary>
        internal static void UnregisterCoordinatorLayout(CoordinatorLayout coordinatorLayout)
        {
            for (int i = _registeredCoordinatorLayouts.Count - 1; i >= 0; i--)
            {
                if (_registeredCoordinatorLayouts[i].Layout.TryGetTarget(out var layout) && layout == coordinatorLayout)
                {
                    _registeredCoordinatorLayouts.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Finds the appropriate GlobalWindowInsetListener for a given view by checking
        /// if it's contained within any registered CoordinatorLayout
        /// </summary>
        internal static GlobalWindowInsetListener? FindListenerForView(AView view)
        {
            // Clean up dead references and find listener 
            for (int i = _registeredCoordinatorLayouts.Count - 1; i >= 0; i--)
            {
                var entry = _registeredCoordinatorLayouts[i];
                if (!entry.Layout.TryGetTarget(out var layout))
                {
                    _registeredCoordinatorLayouts.RemoveAt(i);
                }
                else if (IsViewContainedIn(view, layout))
                {
                    return entry.Listener;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a view is contained within the specified layout's hierarchy
        /// </summary>
        static bool IsViewContainedIn(AView view, ViewGroup layout)
        {
            var parent = view.Parent;
            while (parent is not null)
            {
                if (parent == layout)
                {
                    return true;
                }

                parent = parent.Parent;
            }
            return false;
        }

        /// <summary>
        /// Sets up a CoordinatorLayout to use this listener and handle attach/detach events
        /// </summary>
        internal static CoordinatorLayout SetupCoordinatorLayoutWithLocalListener(CoordinatorLayout coordinatorLayout, GlobalWindowInsetListener listener)
        {
            ViewCompat.SetOnApplyWindowInsetsListener(coordinatorLayout, listener);
            ViewCompat.SetWindowInsetsAnimationCallback(coordinatorLayout, listener);

            listener.RegisterCoordinatorLayout(coordinatorLayout);

            return coordinatorLayout;
        }

        /// <summary>
        /// Removes the local listener from a CoordinatorLayout and properly cleans up
        /// </summary>
        internal static void RemoveCoordinatorLayoutWithLocalListener(CoordinatorLayout coordinatorLayout, GlobalWindowInsetListener listener)
        {
            // Remove the listener from the coordinator layout
            ViewCompat.SetOnApplyWindowInsetsListener(coordinatorLayout, null);
            ViewCompat.SetWindowInsetsAnimationCallback(coordinatorLayout, null);

            // Unregister from the registry
            UnregisterCoordinatorLayout(coordinatorLayout);

            // Reset any tracked views within this coordinator layout
            listener.ResetAppliedSafeAreas(coordinatorLayout);
        }

        public GlobalWindowInsetListener() : base(DispatchModeStop)
        {
        }

        public WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
        {
            if (insets is null || !insets.HasInsets || v is null || IsImeAnimating)
            {
                if (IsImeAnimating)
                {
                    _pendingView = v;
                }

                return insets;
            }

            _pendingView = null;

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

            // Handle MaterialToolbar special case early
            if (v is MaterialToolbar)
            {
                v.SetPadding(displayCutout?.Left ?? 0, 0, displayCutout?.Right ?? 0, 0);
                return WindowInsetsCompat.Consumed;
            }

            // Find AppBarLayout - check direct child first, then first two children
            var appBarLayout = v.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
            if (appBarLayout is null && v is ViewGroup group)
            {
                if (group.ChildCount > 0 && group.GetChildAt(0) is AppBarLayout firstChild)
                {
                    appBarLayout = firstChild;
                }
                else if (group.ChildCount > 1 && group.GetChildAt(1) is AppBarLayout secondChild)
                {
                    appBarLayout = secondChild;
                }
            }

            // Check if AppBarLayout has meaningful content
            bool appBarHasContent = appBarLayout?.MeasuredHeight > 0;
            if (!appBarHasContent && appBarLayout is not null)
            {
                for (int i = 0; i < appBarLayout.ChildCount; i++)
                {
                    var child = appBarLayout.GetChildAt(i);
                    if (child?.MeasuredHeight > 0)
                    {
                        appBarHasContent = true;
                        break;
                    }
                }
            }

            // Apply padding to AppBarLayout based on content and system insets
            if (appBarLayout is not null)
            {
                if (appBarHasContent)
                {
                    var topInset = Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0);
                    appBarLayout.SetPadding(systemBars?.Left ?? 0, topInset, systemBars?.Right ?? 0, 0);
                }
                else
                {
                    appBarLayout.SetPadding(0, 0, 0, 0);
                }
            }

            // Handle bottom navigation
            var hasBottomNav = v.FindViewById(Resource.Id.navigationlayout_bottomtabs)?.MeasuredHeight > 0;
            if (hasBottomNav)
            {
                var bottomInset = Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0);
                v.SetPadding(0, 0, 0, bottomInset);
            }
            else
            {
                v.SetPadding(0, 0, 0, 0);
            }

            // Create new insets with consumed values
            var newSystemBars = Insets.Of(
                systemBars?.Left ?? 0,
                appBarHasContent ? 0 : systemBars?.Top ?? 0,
                systemBars?.Right ?? 0,
                hasBottomNav ? 0 : systemBars?.Bottom ?? 0
            ) ?? Insets.None;

            var newDisplayCutout = Insets.Of(
                displayCutout?.Left ?? 0,
                appBarHasContent ? 0 : displayCutout?.Top ?? 0,
                displayCutout?.Right ?? 0,
                hasBottomNav ? 0 : displayCutout?.Bottom ?? 0
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

        public bool HasTrackedView => _trackedViews.Count > 0;

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
            // Create a copy to avoid modification during enumeration
            var viewsToReset = _trackedViews.ToArray();
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
            if (child is null)
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

        public override void OnPrepare(WindowInsetsAnimationCompat? animation)
        {
            base.OnPrepare(animation);
            if (IsImeAnimation(animation))
            {
                IsImeAnimating = true;
            }
        }

        public override WindowInsetsAnimationCompat.BoundsCompat? OnStart(WindowInsetsAnimationCompat? animation, WindowInsetsAnimationCompat.BoundsCompat? bounds)
        {
            if (IsImeAnimation(animation))
            {
                IsImeAnimating = true;
            }

            return bounds;
        }

        public override WindowInsetsCompat? OnProgress(WindowInsetsCompat? insets, IList<WindowInsetsAnimationCompat>? runningAnimations)
        {
            if (insets is null || runningAnimations is null)
            {
                return insets;
            }

            // Process any IME animations
            foreach (var animation in runningAnimations)
            {
                if (IsImeAnimation(animation))
                {
                    var imeInsets = insets.GetInsets(WindowInsetsCompat.Type.Ime());
                    // IME height available as: imeInsets?.Bottom ?? 0
                    break; // Only need to process one IME animation
                }
            }
            return insets;
        }

        public override void OnEnd(WindowInsetsAnimationCompat? animation)
        {
            base.OnEnd(animation);

            if (IsImeAnimation(animation))
            {
                if (_pendingView is AView view)
                {
                    _pendingView = null;
                    view.Post(() =>
                    {
                        IsImeAnimating = false;
                        ViewCompat.RequestApplyInsets(view);
                    });
                }
                else
                {
                    IsImeAnimating = false;
                }
            }
        }

        /// <summary>
        /// Helper method to check if an animation involves the IME
        /// </summary>
        static bool IsImeAnimation(WindowInsetsAnimationCompat? animation) =>
            animation is not null && (animation.TypeMask & WindowInsetsCompat.Type.Ime()) != 0;
    }
}

/// <summary>
/// Extension methods to access the shared GlobalWindowInsetListener instance.
/// </summary>
internal static class GlobalWindowInsetListenerExtensions
{
    /// <summary>
    /// Sets the appropriate GlobalWindowInsetListener on the specified view.
    /// This prioritizes local coordinator layout listeners over global ones.
    /// </summary>
    /// <param name="view">The Android view to set the listener on</param>
    /// <param name="context">The Android context to get the listener from</param>
    public static bool TrySetGlobalWindowInsetListener(this View view, Context context)
    {
        // Check if this view is contained within a registered CoordinatorLayout first
        if (GlobalWindowInsetListener.FindListenerForView(view) is GlobalWindowInsetListener localListener)
        {
            ViewCompat.SetOnApplyWindowInsetsListener(view, localListener);
            ViewCompat.SetWindowInsetsAnimationCallback(view, localListener);
            return true;
        }

        // Skip setting listener on views inside nested scroll containers or AppBarLayout (except MaterialToolbar)
        if (view is not MaterialToolbar &&
            view.FindParent(parent => parent is NestedScrollView || parent is AppBarLayout || parent is MauiScrollView) is not null)
        {
            return false;
        }

        // If no listener available, this is likely a configuration issue but not critical
        return false;
    }

    /// <summary>
    /// Removes the GlobalWindowInsetListener from the specified view and resets its tracked state.
    /// This should be called when a view is being detached to ensure proper cleanup.
    /// </summary>
    /// <param name="view">The Android view to remove the listener from</param>
    /// <param name="context">The Android context to get the listener from</param>
    public static void RemoveGlobalWindowInsetListener(this View view, Context context)
    {
        // Clear the listeners first
        ViewCompat.SetOnApplyWindowInsetsListener(view, null);
        ViewCompat.SetWindowInsetsAnimationCallback(view, null);

        // Reset view state - prefer local listener if available, otherwise use global
        var listener = GlobalWindowInsetListener.FindListenerForView(view);
        listener?.ResetView(view);
    }
}