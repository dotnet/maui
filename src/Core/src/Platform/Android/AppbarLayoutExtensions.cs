using System;
using System.Runtime.CompilerServices;
using Android.Graphics;
using Android.Views;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Platform
{
    // Manages pinning the lift-on-scroll target of a MAUI navigation AppBarLayout
    // to a specific scrollable view (e.g. MauiScrollView or RecyclerView).
    // Shared between MauiScrollView and MauiRecyclerView to avoid duplicating
    // the same ancestor-walk / attach / detach logic.
    //
    // When the scrollable view is inside a CarouselView/ViewPager2, adjacent
    // off-screen pages are pre-cached and their views stay attached without
    // receiving visibility callbacks during page swipes.  A ViewTreeObserver
    // scroll-changed listener detects these transitions and transfers the
    // lift target to whichever page's scrollable view is currently on-screen.
    //
    // Per-view state is stored in a ConditionalWeakTable so that it is
    // automatically cleaned up when the View is garbage-collected.
    internal static class AppbarLayoutExtensions
    {
        static readonly ConditionalWeakTable<View, AppBarLiftState> s_stateTable = new();

        sealed class AppBarLiftState
        {
            public AppBarLayout? LiftOnScrollAppBar;
            public ScrollChangedListener? ScrollListener;
            public ViewTreeObserver? ScrollListenerObserver;
            public readonly Rect VisibleRect = new();
        }

        internal static void TrySetAppBarLiftTargetIfOnScreen(this View view)
        {
            // Guard: the view may have detached or been hidden between Post() and execution.
            if (!view.IsAlive() || !view.IsAttachedToWindow || view.Visibility != ViewStates.Visible)
            {
                return;
            }

            var state = s_stateTable.GetOrCreateValue(view);

            // Single ancestor walk up front — bail early if no AppBarLayout exists.
            // This avoids registering a ViewTreeObserver listener on pages without an app bar.
            var appBar = FindAppBarLayout(view, out bool hasAncestorScrollView);
            if (hasAncestorScrollView || appBar is null)
            {
                return;
            }

            // When inside a CarouselView, ViewPager2 pre-loads adjacent off-screen pages,
            // so their ScrollViews also attach. Only the on-screen page's ScrollView should
            // claim the lift target. GetGlobalVisibleRect returns false if the view is
            // entirely outside the clipped viewport (e.g. a pre-loaded carousel page).
            if (view.GetGlobalVisibleRect(state.VisibleRect))
            {
                SetAppBarLiftTarget(view, state, appBar);
            }

            // Listen for parent scroll changes to detect carousel page transitions.
            // We only reach here when an AppBarLayout was found above.
            StartListeningForParentScrollChanges(view, state);
        }

        internal static void ClearAppBarLiftTarget(this View view)
        {
            if (!s_stateTable.TryGetValue(view, out var state))
            {
                return;
            }

            StopListeningForParentScrollChanges(view, state);
            ClearAppBarLiftTargetCore(view, state);
        }

        static void ClearAppBarLiftTargetCore(View view, AppBarLiftState state)
        {
            if (state.LiftOnScrollAppBar is null)
            {
                return;
            }

            // Only clear if we're still the current target; avoid stomping on another scroll view
            // that may have been set as the target after us.
            if (state.LiftOnScrollAppBar.LiftOnScrollTargetViewId == view.Id)
            {
                state.LiftOnScrollAppBar.LiftOnScrollTargetViewId = View.NoId;
            }

            state.LiftOnScrollAppBar = null;
        }

        static void TrySetAppBarLiftTarget(View view, AppBarLiftState state)
        {
            // Ancestor walk to find the AppBarLayout while also checking
            // whether a MauiScrollView ancestor exists (which should own the
            // lift target instead of this view).
            var appBar = FindAppBarLayout(view, out bool hasAncestorScrollView);
            if (hasAncestorScrollView || appBar is null)
            {
                return;
            }

            SetAppBarLiftTarget(view, state, appBar);
        }

        static void SetAppBarLiftTarget(View view, AppBarLiftState state, AppBarLayout appBar)
        {
            if (view.Id == View.NoId)
            {
                // LiftOnScrollTargetViewId requires a non-NoId view id.
                // Intentionally assigning a generated id here; the view will
                // keep this id for the rest of its lifetime, which is fine
                // because MauiScrollView / MauiRecyclerView are not looked up
                // by id by any other host code.
                view.Id = View.GenerateViewId();
            }

            state.LiftOnScrollAppBar = appBar;
            appBar.LiftOnScrollTargetViewId = view.Id;

            // Force the AppBar to reflect this view's current scroll position.
            // After a carousel swipe or visibility toggle the AppBar may be stuck
            // in the wrong state; CanScrollVertically(-1) is true when the view
            // has been scrolled down from the top.
            appBar.SetLifted(view.CanScrollVertically(-1));
        }

        static void OnParentScrollChanged(View view, AppBarLiftState state)
        {
            if (!view.IsAlive() || !view.IsAttachedToWindow || view.Visibility != ViewStates.Visible)
            {
                return;
            }

            bool isOnScreen = view.GetGlobalVisibleRect(state.VisibleRect);
            bool ownsTarget = state.LiftOnScrollAppBar is not null;

            if (isOnScreen && !ownsTarget)
            {
                TrySetAppBarLiftTarget(view, state);
            }
            else if (!isOnScreen && ownsTarget)
            {
                // Release without stopping the listener — we still need to
                // detect when the carousel swipes back to this page.
                ClearAppBarLiftTargetCore(view, state);
            }
        }

        static void StartListeningForParentScrollChanges(View view, AppBarLiftState state)
        {
            if (state.ScrollListener is not null)
            {
                return;
            }

            var observer = view.ViewTreeObserver;
            if (observer is null || !observer.IsAlive)
            {
                return;
            }

            state.ScrollListener = new ScrollChangedListener(view, state);
            state.ScrollListenerObserver = observer;
            observer.AddOnScrollChangedListener(state.ScrollListener);
        }

        static void StopListeningForParentScrollChanges(View view, AppBarLiftState state)
        {
            if (state.ScrollListener is null)
            {
                return;
            }

            // Remove from the same observer instance used when adding.
            var observer = state.ScrollListenerObserver;
            if (observer is not null && observer.IsAlive)
            {
                observer.RemoveOnScrollChangedListener(state.ScrollListener);
            }
            else
            {
                // Fallback for safety if observer rotated between add/remove.
                observer = view.ViewTreeObserver;
                if (observer is not null && observer.IsAlive)
                {
                    observer.RemoveOnScrollChangedListener(state.ScrollListener);
                }
            }

            state.ScrollListener.Dispose();
            state.ScrollListener = null;
            state.ScrollListenerObserver = null;
        }

        static AppBarLayout? FindAppBarLayout(View view, out bool hasAncestorScrollView)
        {
            // Single ancestor walk that both checks for a MauiScrollView ancestor
            // (which should own the lift target instead) AND finds the AppBarLayout.
            // NavigationPage uses Resource.Id.navigationlayout_appbar, but Shell creates
            // its AppBarLayout programmatically without an ID, so we match any AppBarLayout.
            //
            // Nested-host note: Shell-inside-NavigationPage (or vice versa) is not a
            // supported MAUI configuration, so there is only ever one relevant AppBarLayout
            // in the ancestor/sibling chain for any given scroll view.  The walk returns the
            // first one found, which is the correct one for all supported layouts.
            hasAncestorScrollView = false;
            var parent = view.Parent;

            while (parent is View parentView)
            {
                if (parentView is MauiScrollView)
                {
                    hasAncestorScrollView = true;
                    return null;
                }

                // Stop the MauiScrollView check once we reach the AppBarLayout level —
                // anything above that isn't "inside the page".
                if (parentView is AppBarLayout directAppBar)
                {
                    return directAppBar;
                }

                if (parentView is ViewGroup group)
                {
                    for (int i = 0; i < group.ChildCount; i++)
                    {
                        if (group.GetChildAt(i) is AppBarLayout siblingAppBar)
                        {
                            return siblingAppBar;
                        }
                    }
                }

                parent = parentView.Parent;
            }

            return null;
        }

        // Lightweight Java-side listener that forwards ViewTreeObserver scroll
        // changes back to the static extension for carousel page-change detection.
        sealed class ScrollChangedListener : Java.Lang.Object, ViewTreeObserver.IOnScrollChangedListener
        {
            readonly View _view;
            readonly AppBarLiftState _state;

            public ScrollChangedListener(View view, AppBarLiftState state)
            {
                _view = view;
                _state = state;
            }

            public void OnScrollChanged()
            {
                OnParentScrollChanged(_view, _state);
            }
        }
    }
}
