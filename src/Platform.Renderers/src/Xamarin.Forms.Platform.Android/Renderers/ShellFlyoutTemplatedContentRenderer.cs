using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.AppBar;
using Xamarin.Forms.Internals;
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
		ViewGroup _rootView;
		Drawable _defaultBackgroundColor;
		ImageView _bgImage;
		AppBarLayout _appBar;
		RecyclerView _recycler;
		ShellFlyoutRecyclerAdapter _adapter;
		View _flyoutHeader;
		int _actionBarHeight;
		ScrollLayoutManager _layoutManager;

		public ShellFlyoutTemplatedContentRenderer(IShellContext shellContext)
		{
			_shellContext = shellContext;

			LoadView(shellContext);
		}

		protected virtual void LoadView(IShellContext shellContext)
		{
			Profile.FrameBegin();

			var context = shellContext.AndroidContext;

			// Android designer can't load fragments or resources from layouts
			if (context.IsDesignerContext())
			{
				_rootView = new FrameLayout(context);
				return;
			}

			var coordinator = LayoutInflater.FromContext(context).Inflate(Resource.Layout.FlyoutContent, null);

			Profile.FramePartition("Find Recycler");
			_recycler = coordinator.FindViewById<RecyclerView>(Resource.Id.flyoutcontent_recycler);

			Profile.FramePartition("Find AppBar");
			_appBar = coordinator.FindViewById<AppBarLayout>(Resource.Id.flyoutcontent_appbar);

			_rootView = coordinator as ViewGroup;

			Profile.FramePartition("Add Listener");
			_appBar.AddOnOffsetChangedListener(this);

			Profile.FramePartition("Add HeaderView");
			_actionBarHeight = (int)context.ToPixels(56);
			UpdateFlyoutHeader();

			Profile.FramePartition("Recycler.SetAdapter");
			_adapter = new ShellFlyoutRecyclerAdapter(shellContext, OnElementSelected);
			_recycler.SetClipToPadding(false);
			_recycler.SetLayoutManager(_layoutManager = new ScrollLayoutManager(context, (int)Orientation.Vertical, false));
			_recycler.SetLayoutManager(new LinearLayoutManager(context, (int)Orientation.Vertical, false));
			_recycler.SetAdapter(_adapter);

			Profile.FramePartition("Initialize BgImage");
			var metrics = context.Resources.DisplayMetrics;
			var width = Math.Min(metrics.WidthPixels, metrics.HeightPixels);

			using (TypedValue tv = new TypedValue())
			{
				if (context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ActionBarSize, tv, true))
				{
					_actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, metrics);
				}
			}

			width -= _actionBarHeight;

			coordinator.LayoutParameters = new LP(width, LP.MatchParent);

			_bgImage = new ImageView(context)
			{
				LayoutParameters = new LP(coordinator.LayoutParameters)
			};

			Profile.FramePartition("UpdateFlyoutHeaderBehavior");
			UpdateFlyoutHeaderBehavior();
			_shellContext.Shell.PropertyChanged += OnShellPropertyChanged;

			Profile.FramePartition("UpdateFlyoutBackground");
			UpdateFlyoutBackground();

			Profile.FramePartition(nameof(UpdateVerticalScrollMode));
			UpdateVerticalScrollMode();
			Profile.FrameEnd();
		}

		void OnFlyoutHeaderMeasureInvalidated(object sender, EventArgs e)
		{
			if (_headerView != null)
				UpdateFlyoutHeaderBehavior();
		}

		protected void OnElementSelected(Element element)
		{
			((IShellController)_shellContext.Shell).OnFlyoutItemSelected(element);
		}

		protected virtual void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutHeaderBehaviorProperty.PropertyName)
				UpdateFlyoutHeaderBehavior();
			else if (e.IsOneOf(
				Shell.FlyoutBackgroundColorProperty,
				Shell.FlyoutBackgroundProperty,
				Shell.FlyoutBackgroundImageProperty,
				Shell.FlyoutBackgroundImageAspectProperty))
				UpdateFlyoutBackground();
			else if (e.Is(Shell.FlyoutVerticalScrollModeProperty))
				UpdateVerticalScrollMode();
			else if (e.IsOneOf(
				Shell.FlyoutHeaderProperty,
				Shell.FlyoutHeaderTemplateProperty))
				UpdateFlyoutHeader();
		}

		void UpdateFlyoutHeader()
		{
			if (_headerView != null)
			{
				var oldHeaderView = _headerView;
				_headerView = null;
				_appBar.RemoveView(oldHeaderView);
				oldHeaderView.Dispose();
			}

			if (_flyoutHeader != null)
			{
				_flyoutHeader.MeasureInvalidated -= OnFlyoutHeaderMeasureInvalidated;
			}

			_flyoutHeader = ((IShellController)_shellContext.Shell).FlyoutHeader;
			if (_flyoutHeader != null)
				_flyoutHeader.MeasureInvalidated += OnFlyoutHeaderMeasureInvalidated;

			_headerView = new HeaderContainer(_shellContext.AndroidContext, _flyoutHeader)
			{
				MatchWidth = true
			};
			_headerView.SetMinimumHeight(_actionBarHeight);
			_headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
			{
				ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll
			};
			_appBar.AddView(_headerView);
			UpdateFlyoutHeaderBehavior();
		}

		void UpdateVerticalScrollMode()
		{
			if (_layoutManager != null)
				_layoutManager.ScrollVertically = _shellContext.Shell.FlyoutVerticalScrollMode;
		}

		protected virtual void UpdateFlyoutBackground()
		{
			var brush = _shellContext.Shell.FlyoutBackground;

			if (Brush.IsNullOrEmpty(brush))
			{
				var color = _shellContext.Shell.FlyoutBackgroundColor;
				if (_defaultBackgroundColor == null)
					_defaultBackgroundColor = _rootView.Background;

				_rootView.Background = color.IsDefault ? _defaultBackgroundColor : new ColorDrawable(color.ToAndroid());
			}
			else
				_rootView.UpdateBackground(brush);

			UpdateFlyoutBgImageAsync();
		}

		async void UpdateFlyoutBgImageAsync()
		{
			var imageSource = _shellContext.Shell.FlyoutBackgroundImage;
			if (imageSource == null || !_shellContext.Shell.IsSet(Shell.FlyoutBackgroundImageProperty))
			{
				if (_rootView.IndexOfChild(_bgImage) != -1)
					_rootView.RemoveView(_bgImage);
				return;
			}
			using (var drawable = await _shellContext.AndroidContext.GetFormsDrawableAsync(imageSource) as BitmapDrawable)
			{
				if (_rootView.IsDisposed())
					return;

				if (drawable == null)
				{
					if (_rootView.IndexOfChild(_bgImage) != -1)
						_rootView.RemoveView(_bgImage);
					return;
				}

				_bgImage.SetImageDrawable(drawable);

				switch (_shellContext.Shell.FlyoutBackgroundImageAspect)
				{
					default:
					case Aspect.AspectFit:
						_bgImage.SetScaleType(ImageView.ScaleType.FitCenter);
						break;
					case Aspect.AspectFill:
						_bgImage.SetScaleType(ImageView.ScaleType.CenterCrop);
						break;
					case Aspect.Fill:
						_bgImage.SetScaleType(ImageView.ScaleType.FitXy);
						break;
				}

				if (_rootView.IndexOfChild(_bgImage) == -1)
				{
					if (_bgImage.SetElevation(float.MinValue))
						_rootView.AddView(_bgImage);
					else
						_rootView.AddView(_bgImage, 0);
				}
			}
		}

		protected virtual void UpdateFlyoutHeaderBehavior()
		{
			var context = _shellContext.AndroidContext;

			var margin = _flyoutHeader?.Margin ?? default(Thickness);

			var minimumHeight = Convert.ToInt32(_actionBarHeight + context.ToPixels(margin.Top) - context.ToPixels(margin.Bottom));
			_headerView.SetMinimumHeight(minimumHeight);

			switch (_shellContext.Shell.FlyoutHeaderBehavior)
			{
				case FlyoutHeaderBehavior.Default:
				case FlyoutHeaderBehavior.Fixed:
					_headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
					{
						ScrollFlags = 0,
						LeftMargin = (int)context.ToPixels(margin.Left),
						TopMargin = (int)context.ToPixels(margin.Top),
						RightMargin = (int)context.ToPixels(margin.Right),
						BottomMargin = (int)context.ToPixels(margin.Bottom)
					};
					break;
				case FlyoutHeaderBehavior.Scroll:
					_headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
					{
						ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll,
						LeftMargin = (int)context.ToPixels(margin.Left),
						TopMargin = (int)context.ToPixels(margin.Top),
						RightMargin = (int)context.ToPixels(margin.Right),
						BottomMargin = (int)context.ToPixels(margin.Bottom)
					};
					break;
				case FlyoutHeaderBehavior.CollapseOnScroll:
					_headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
					{
						ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagExitUntilCollapsed |
							AppBarLayout.LayoutParams.ScrollFlagScroll,
						LeftMargin = (int)context.ToPixels(margin.Left),
						TopMargin = (int)context.ToPixels(margin.Top),
						RightMargin = (int)context.ToPixels(margin.Right),
						BottomMargin = (int)context.ToPixels(margin.Bottom)
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
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_shellContext.Shell.PropertyChanged -= OnShellPropertyChanged;

				if (_flyoutHeader != null)
					_flyoutHeader.MeasureInvalidated -= OnFlyoutHeaderMeasureInvalidated;

				if (_appBar != null)
				{
					_appBar.RemoveOnOffsetChangedListener(this);
					_appBar.RemoveView(_headerView);
				}

				if (_recycler != null)
				{
					_recycler.SetLayoutManager(null);
					_recycler.SetAdapter(null);
					_recycler.Dispose();
				}

				_adapter?.Dispose();
				_headerView.Dispose();
				_rootView.Dispose();
				_layoutManager?.Dispose();
				_defaultBackgroundColor?.Dispose();
				_bgImage?.Dispose();

				_flyoutHeader = null;
				_rootView = null;
				_headerView = null;
				_shellContext = null;
				_appBar = null;
				_recycler = null;
				_adapter = null;
				_defaultBackgroundColor = null;
				_layoutManager = null;
				_bgImage = null;
			}

			base.Dispose(disposing);
		}

		// This view lets us use the top padding to "squish" the content down
		public class HeaderContainer : ContainerView
		{
			bool _isdisposed = false;
			public HeaderContainer(Context context, View view) : base(context, view)
			{
				Initialize(view);
			}

			public HeaderContainer(Context context, IAttributeSet attribs) : base(context, attribs)
			{
				Initialize(View);
			}

			public HeaderContainer(Context context, IAttributeSet attribs, int defStyleAttr) : base(context, attribs, defStyleAttr)
			{
				Initialize(View);
			}

			protected HeaderContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
			{
				Initialize(View);
			}

			void Initialize(View view)
			{
				if (view != null)
					view.PropertyChanged += OnViewPropertyChanged;
			}

			void OnViewPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == PlatformConfiguration.AndroidSpecific.VisualElement.ElevationProperty.PropertyName)
				{
					UpdateElevation();
				}
			}

			void UpdateElevation()
			{
				if (Parent is AView view)
					ElevationHelper.SetElevation(view, View);
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

				UpdateElevation();

				if (View != null)
					View.Layout(new Rectangle(paddingLeft, paddingTop, width, height));
			}

			protected override void Dispose(bool disposing)
			{
				if (_isdisposed)
					return;

				_isdisposed = true;
				if (disposing)
				{
					if (View != null)
						View.PropertyChanged -= OnViewPropertyChanged;
				}

				View = null;

				base.Dispose(disposing);
			}
		}
	}

	internal class ScrollLayoutManager : LinearLayoutManager
	{
		public ScrollMode ScrollVertically { get; set; } = ScrollMode.Auto;

		public ScrollLayoutManager(Context context, int orientation, bool reverseLayout) : base(context, orientation, reverseLayout)
		{
		}

		int GetVisibleChildCount()
		{
			var firstVisibleIndex = FindFirstCompletelyVisibleItemPosition();
			var lastVisibleIndex = FindLastCompletelyVisibleItemPosition();
			return lastVisibleIndex - firstVisibleIndex + 1;
		}

		public override bool CanScrollVertically()
		{
			switch (ScrollVertically)
			{
				case ScrollMode.Disabled:
					return false;
				case ScrollMode.Enabled:
					return true;
				default:
				case ScrollMode.Auto:
					return ChildCount > GetVisibleChildCount();
			}
		}
	}
}