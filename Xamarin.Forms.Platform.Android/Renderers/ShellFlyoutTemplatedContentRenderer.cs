using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.ComponentModel;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android
{
    public class ShellFlyoutTemplatedContentRenderer : Java.Lang.Object, IShellFlyoutContentRenderer
		, AppBarLayout.IOnOffsetChangedListener
    {
        #region IShellFlyoutContentRenderer

        public AView AndroidView => _rootView;

        #endregion IShellFlyoutContentRenderer

        IShellContext _shellContext;
        bool _disposed;
        HeaderContainer _headerView;
        AView _rootView;
        Drawable _defaultBackground;

        public ShellFlyoutTemplatedContentRenderer(IShellContext shellContext)
        {
            _shellContext = shellContext;

            LoadView(shellContext);
        }

        protected virtual void LoadView(IShellContext shellContext)
        {
            var context = shellContext.AndroidContext;

			// Android designer can't load fragments or resources from layouts
			if (context.IsDesignerContext())
			{
				_rootView = new FrameLayout(context);
				return;
			}

			var coordinator = LayoutInflater.FromContext(context).Inflate(Resource.Layout.FlyoutContent, null);
			var recycler = coordinator.FindViewById<RecyclerView>(Resource.Id.flyoutcontent_recycler);
			var appBar = coordinator.FindViewById<AppBarLayout>(Resource.Id.flyoutcontent_appbar);

			_rootView = coordinator;

			appBar.AddOnOffsetChangedListener(this);

            int actionBarHeight = (int)context.ToPixels(56);

            _headerView = new HeaderContainer(context, ((IShellController)shellContext.Shell).FlyoutHeader)
            {
                MatchWidth = true
            };
            _headerView.SetMinimumHeight(actionBarHeight);
            _headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
            {
                ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll
            };
            appBar.AddView(_headerView);

            var adapter = new ShellFlyoutRecyclerAdapter(shellContext, OnElementSelected);
            recycler.SetPadding(0, (int)context.ToPixels(20), 0, 0);
            recycler.SetClipToPadding(false);
            recycler.SetLayoutManager(new LinearLayoutManager(context, (int)Orientation.Vertical, false));
            recycler.SetAdapter(adapter);

            var metrics = context.Resources.DisplayMetrics;
            var width = Math.Min(metrics.WidthPixels, metrics.HeightPixels);

			using (TypedValue tv = new TypedValue())
			{
				if (context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ActionBarSize, tv, true))
				{
					actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, metrics);
				}
			}

            width -= actionBarHeight;

            coordinator.LayoutParameters = new LP(width, LP.MatchParent);

            UpdateFlyoutHeaderBehavior();
            _shellContext.Shell.PropertyChanged += OnShellPropertyChanged;

            UpdateFlyoutBackgroundColor();
        }

        protected void OnElementSelected(Element element)
        {
            ((IShellController)_shellContext.Shell).OnFlyoutItemSelected(element);
        }

        protected virtual void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Shell.FlyoutHeaderBehaviorProperty.PropertyName)
                UpdateFlyoutHeaderBehavior();
            else if (e.PropertyName == Shell.FlyoutBackgroundColorProperty.PropertyName)
                UpdateFlyoutBackgroundColor();
        }

        protected virtual void UpdateFlyoutBackgroundColor()
        {
            var color = _shellContext.Shell.FlyoutBackgroundColor;
            if (_defaultBackground == null && color.IsDefault)
                return;

            if (_defaultBackground == null)
                _defaultBackground = _rootView.Background;

            if (color.IsDefault)
                _rootView.Background = _defaultBackground;
            else
                _rootView.Background = new ColorDrawable(color.ToAndroid());
        }

        protected virtual void UpdateFlyoutHeaderBehavior()
        {
            switch (_shellContext.Shell.FlyoutHeaderBehavior)
            {
                case FlyoutHeaderBehavior.Default:
                case FlyoutHeaderBehavior.Fixed:
                    _headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
                    {
                        ScrollFlags = 0
                    };
                    break;
                case FlyoutHeaderBehavior.Scroll:
                    _headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
                    {
                        ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll
                    };
                    break;
                case FlyoutHeaderBehavior.CollapseOnScroll:
                    _headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
                    {
                        ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagExitUntilCollapsed |
                            AppBarLayout.LayoutParams.ScrollFlagScroll
                    };
                    break;
            }
        }

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            var headerBehavior = _shellContext.Shell.FlyoutHeaderBehavior;
            if (headerBehavior != FlyoutHeaderBehavior.CollapseOnScroll)
                return;

            _headerView.SetPadding(0, -verticalOffset, 0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _shellContext.Shell.PropertyChanged -= OnShellPropertyChanged;
                    _headerView.Dispose();
                    _rootView.Dispose();
                    _defaultBackground?.Dispose();
                }

                _defaultBackground = null;
                _rootView = null;
                _headerView = null;
                _shellContext = null;
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        // This view lets us use the top padding to "squish" the content down
        public class HeaderContainer : ContainerView
        {
            public HeaderContainer(Context context, View view) : base(context, view)
            {
            }

            public HeaderContainer(Context context, IAttributeSet attribs) : base(context, attribs)
            {
            }

            public HeaderContainer(Context context, IAttributeSet attribs, int defStyleAttr) : base(context, attribs, defStyleAttr)
            {
            }

            protected HeaderContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }

            protected override void LayoutView(double x, double y, double width, double height)
            {
                var context = Context;
                var paddingLeft = context.FromPixels(PaddingLeft);
                var paddingTop = context.FromPixels(PaddingTop);
                var paddingRight = context.FromPixels(PaddingRight);
                var paddingBottom = context.FromPixels(PaddingBottom);

                width -= paddingLeft + paddingRight;
                height -= paddingTop + paddingBottom;

                View.Layout(new Rectangle(paddingLeft, paddingTop, width, height));
            }
        }
    }
}