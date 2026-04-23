using Android.Graphics;
using Android.Views;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Platform
{
    // Manages pinning the lift-on-scroll target of a MAUI navigation AppBarLayout
    // to a specific scrollable view (e.g. MauiScrollView or RecyclerView).
    // Shared between MauiScrollView and MauiRecyclerView to avoid duplicating
    // the same ancestor-walk / attach / detach logic.
    internal class AppBarLiftTargetHelper
    {
        readonly View _view;
        AppBarLayout? _liftOnScrollAppBar;

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
            if (!_view.GetGlobalVisibleRect(new Rect()))
            {
                return;
            }

            TrySet();
        }

        internal void Clear()
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
                // Force the AppBar to re-evaluate its lifted state now that the
                // scrollable target is gone; otherwise it stays stuck in whatever
                // state it was last in.
                _liftOnScrollAppBar.SetLifted(false);
            }

            _liftOnScrollAppBar = null;
        }

        void TrySet()
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
    }
}
