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
            public readonly Rect VisibleRect = new();
        }

        internal static void TrySetAppBarLiftTargetIfOnScreen(this View view)
        {
            // Guard: the view may have detached or been hidden between Post() and execution.
            if (!view.IsAttachedToWindow || view.Visibility != ViewStates.Visible)
            {
                return;
            }

            var state = s_stateTable.GetOrCreateValue(view);

            // When inside a CarouselView, ViewPager2 pre-loads adjacent off-screen pages,
            // so their ScrollViews also attach. Only the on-screen page's ScrollView should
            // claim the lift target. GetGlobalVisibleRect returns false if the view is
            // entirely outside the clipped viewport (e.g. a pre-loaded carousel page).
            if (!view.GetGlobalVisibleRect(state.VisibleRect))
            {
                // Off-screen — start listening for parent scroll changes so we
                // can claim the lift target when the page scrolls into view.
                StartListeningForParentScrollChanges(view, state);
                return;
            }

            TrySetAppBarLiftTarget(view, state);

            // Listen for parent scroll changes to detect when we go off-screen
            // (e.g. user swipes to another carousel page).
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
                state.LiftOnScrollAppBar.SetLifted(false);
            }

            state.LiftOnScrollAppBar = null;
        }

        static void TrySetAppBarLiftTarget(View view, AppBarLiftState state)
        {
            // Single ancestor walk: find the AppBarLayout while also checking
            // whether a MauiScrollView ancestor exists (which should own the
            // lift target instead of this view).
            var appBar = FindAppBarLayout(view, out bool hasAncestorScrollView);
            if (hasAncestorScrollView || appBar is null)
            {
                return;
            }

            if (view.Id == View.NoId)
            {
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
            if (!view.IsAttachedToWindow || view.Visibility != ViewStates.Visible)
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
            observer.AddOnScrollChangedListener(state.ScrollListener);
        }

        static void StopListeningForParentScrollChanges(View view, AppBarLiftState state)
        {
            if (state.ScrollListener is null)
            {
                return;
            }

            var observer = view.ViewTreeObserver;
            if (observer is not null && observer.IsAlive)
            {
                observer.RemoveOnScrollChangedListener(state.ScrollListener);
            }

            state.ScrollListener.Dispose();
            state.ScrollListener = null;
        }

        static AppBarLayout? FindAppBarLayout(View view, out bool hasAncestorScrollView)
        {
            // Single ancestor walk that both checks for a MauiScrollView ancestor
            // (which should own the lift target instead) AND finds the AppBarLayout.
            // NavigationPage uses Resource.Id.navigationlayout_appbar, but Shell creates
            // its AppBarLayout programmatically without an ID, so we match any AppBarLayout.
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
