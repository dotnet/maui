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
    internal class AppBarLiftTargetHelper
    {
        readonly View _view;
        readonly Rect _visibleRect = new Rect();
        AppBarLayout? _liftOnScrollAppBar;
        ScrollChangedListener? _scrollChangedListener;

        internal AppBarLiftTargetHelper(View view)
        {
            _view = view;
        }

        internal void TrySetIfOnScreen()
        {
            // Guard: the view may have detached or been hidden between Post() and execution.
            if (!_view.IsAttachedToWindow || _view.Visibility != ViewStates.Visible)
            {
                return;
            }

            // When inside a CarouselView, ViewPager2 pre-loads adjacent off-screen pages,
            // so their ScrollViews also attach. Only the on-screen page's ScrollView should
            // claim the lift target. GetGlobalVisibleRect returns false if the view is
            // entirely outside the clipped viewport (e.g. a pre-loaded carousel page).
            if (!_view.GetGlobalVisibleRect(_visibleRect))
            {
                // Off-screen — start listening for parent scroll changes so we
                // can claim the lift target when the page scrolls into view.
                StartListeningForParentScrollChanges();
                return;
            }

            TrySetAppBarLiftTarget();

            // Listen for parent scroll changes to detect when we go off-screen
            // (e.g. user swipes to another carousel page).
            StartListeningForParentScrollChanges();
        }

        internal void Clear()
        {
            StopListeningForParentScrollChanges();
            ClearAppBarLiftTarget();
        }

        void ClearAppBarLiftTarget()
        {
            if (_liftOnScrollAppBar is null)
            {
                return;
            }

            // Only clear if we're still the current target; avoid stomping on another scroll view
            // that may have been set as the target after us.
            if (_liftOnScrollAppBar.LiftOnScrollTargetViewId == _view.Id)
            {
                _liftOnScrollAppBar.LiftOnScrollTargetViewId = View.NoId;
                _liftOnScrollAppBar.SetLifted(false);
            }

            _liftOnScrollAppBar = null;
        }

        void TrySetAppBarLiftTarget()
        {
            // If a MauiScrollView ancestor exists, it should own the lift target instead.
            // The outermost scroll view in the page is the one whose scroll offset should
            // drive the AppBar's lifted state.
            if (HasAncestorMauiScrollView())
            {
                return;
            }

            var appBar = FindAppBarLayout();
            if (appBar is null)
            {
                return;
            }

            if (_view.Id == View.NoId)
            {
                _view.Id = View.GenerateViewId();
            }

            _liftOnScrollAppBar = appBar;
            appBar.LiftOnScrollTargetViewId = _view.Id;

            // Force the AppBar to reflect this view's current scroll position.
            // After a carousel swipe or visibility toggle the AppBar may be stuck
            // in the wrong state; CanScrollVertically(-1) is true when the view
            // has been scrolled down from the top.
            appBar.SetLifted(_view.CanScrollVertically(-1));
        }

        void OnParentScrollChanged()
        {
            if (!_view.IsAttachedToWindow || _view.Visibility != ViewStates.Visible)
            {
                return;
            }

            bool isOnScreen = _view.GetGlobalVisibleRect(_visibleRect);
            bool ownsTarget = _liftOnScrollAppBar is not null;

            if (isOnScreen && !ownsTarget)
            {
                TrySetAppBarLiftTarget();
            }
            else if (!isOnScreen && ownsTarget)
            {
                // Release without stopping the listener — we still need to
                // detect when the carousel swipes back to this page.
                ClearAppBarLiftTarget();
            }
        }

        void StartListeningForParentScrollChanges()
        {
            if (_scrollChangedListener is not null)
            {
                return;
            }

            var observer = _view.ViewTreeObserver;
            if (observer is null || !observer.IsAlive)
            {
                return;
            }

            _scrollChangedListener = new ScrollChangedListener(this);
            observer.AddOnScrollChangedListener(_scrollChangedListener);
        }

        void StopListeningForParentScrollChanges()
        {
            if (_scrollChangedListener is null)
            {
                return;
            }

            var observer = _view.ViewTreeObserver;
            if (observer is not null && observer.IsAlive)
            {
                observer.RemoveOnScrollChangedListener(_scrollChangedListener);
            }

            _scrollChangedListener.Dispose();
            _scrollChangedListener = null;
        }

        bool HasAncestorMauiScrollView()
        {
            var parent = _view.Parent;
            while (parent is View parentView)
            {
                if (parentView is MauiScrollView)
                {
                    return true;
                }

                // Stop once we reach the AppBarLayout level — anything above that isn't "inside the page".
                if (parentView is AppBarLayout ||
                    (parentView.Id != View.NoId && parentView.Id == Resource.Id.navigationlayout_appbar))
                {
                    return false;
                }

                parent = parentView.Parent;
            }

            return false;
        }

        AppBarLayout? FindAppBarLayout()
        {
            // Walk up the ancestry looking for an AppBarLayout that is a sibling
            // of the content view hosting this scroll view.
            // NavigationPage uses Resource.Id.navigationlayout_appbar, but Shell creates
            // its AppBarLayout programmatically without an ID, so we match any AppBarLayout.
            var parent = _view.Parent;
            while (parent is View parentView)
            {
                if (parentView is ViewGroup group)
                {
                    for (int i = 0; i < group.ChildCount; i++)
                    {
                        if (group.GetChildAt(i) is AppBarLayout appBar)
                        {
                            return appBar;
                        }
                    }
                }

                parent = parentView.Parent;
            }

            return null;
        }

        // Lightweight Java-side listener that forwards ViewTreeObserver scroll
        // changes back to the managed helper for carousel page-change detection.
        sealed class ScrollChangedListener : Java.Lang.Object, ViewTreeObserver.IOnScrollChangedListener
        {
            readonly AppBarLiftTargetHelper _helper;

            public ScrollChangedListener(AppBarLiftTargetHelper helper)
            {
                _helper = helper;
            }

            public void OnScrollChanged()
            {
                _helper.OnParentScrollChanged();
            }
        }
    }
}
