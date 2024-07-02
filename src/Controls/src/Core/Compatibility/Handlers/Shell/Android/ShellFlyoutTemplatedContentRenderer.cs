#nullable disable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Hardware.Lights;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Layouts;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellFlyoutTemplatedContentRenderer : Java.Lang.Object, IShellFlyoutContentRenderer
		, AppBarLayout.IOnOffsetChangedListener
	{
		#region IShellFlyoutContentView

		public AView AndroidView => _rootView;

		#endregion IShellFlyoutContentView

		IShellContext _shellContext;
		bool _disposed;
		HeaderContainer _headerView;
		FrameLayout _headerFrameLayout;
		ViewGroup _rootView;
		Drawable _defaultBackgroundColor;
		ImageView _bgImage;
		AppBarLayout _appBar;
		AView _flyoutContentView;
		ShellViewRenderer _contentView;
		View _flyoutHeader;
		ContainerView _footerView;
		int _actionBarHeight;
		int _flyoutHeight;
		int _flyoutWidth;
		protected IMauiContext MauiContext => _shellContext.Shell.Handler.MauiContext;
		bool _initialLayoutChangeFired;
		IFlyoutView FlyoutView => _shellContext?.Shell;
		protected IShellContext ShellContext => _shellContext;
		protected AView FooterView => _footerView?.PlatformView;
		protected AView View => _rootView;


		public ShellFlyoutTemplatedContentRenderer(IShellContext shellContext)
		{
			_shellContext = shellContext;
			_shellContext.CurrentDrawerLayout.DrawerStateChanged += OnFlyoutStateChanging;
			LoadView(shellContext);
		}

		void OnFlyoutStateChanging(object sender, AndroidX.DrawerLayout.Widget.DrawerLayout.DrawerStateChangedEventArgs e)
		{
			if (e.NewState != DrawerLayout.StateIdle)
			{
				if (_flyoutContentView == null)
					UpdateFlyoutContent();

				_shellContext.CurrentDrawerLayout.DrawerStateChanged -= OnFlyoutStateChanging;
			}
		}

		protected virtual void LoadView(IShellContext shellContext)
		{
			var context = shellContext.AndroidContext;
			var layoutInflator = shellContext.Shell.FindMauiContext().GetLayoutInflater();
			var coordinator = (ViewGroup)layoutInflator.Inflate(Controls.Resource.Layout.flyoutcontent, null);

			_appBar = coordinator.FindViewById<AppBarLayout>(Controls.Resource.Id.flyoutcontent_appbar);

			(_appBar.LayoutParameters as CoordinatorLayout.LayoutParams)
				.Behavior = new AppBarLayout.Behavior();

			_rootView = coordinator as ViewGroup;

			_appBar.AddOnOffsetChangedListener(this);

			_actionBarHeight = context.GetActionBarHeight();

			UpdateFlyoutHeader();

			var metrics = context.Resources.DisplayMetrics;
			var width = Math.Min(metrics.WidthPixels, metrics.HeightPixels);

			width -= _actionBarHeight;

			coordinator.LayoutParameters = new LP(width, LP.MatchParent);

			_bgImage = new ImageView(context)
			{
				LayoutParameters = new LP(coordinator.LayoutParameters)
			};

			UpdateFlyoutHeaderBehavior();
			_shellContext.Shell.PropertyChanged += OnShellPropertyChanged;

			UpdateFlyoutBackground();

			UpdateVerticalScrollMode();

			UpdateFlyoutFooter();

			if (FlyoutView.FlyoutBehavior == FlyoutBehavior.Locked)
				OnFlyoutViewLayoutChanging();

			if (View is ShellFlyoutLayout sfl)
			{
				// The purpose of this code is to load the flyout content after
				// the details content is visible.
				// Loading the Recycler View the first time is expensive
				// so we want to delay loading to the latest possible point in time so
				// it doesn't delay initial startup.
				GenericGlobalLayoutListener ggll = null;
				ggll = new GenericGlobalLayoutListener(InitialLoad, sfl);

				void InitialLoad(GenericGlobalLayoutListener listener, AView view)
				{
					OnFlyoutViewLayoutChanging();

					if (_flyoutContentView == null || ggll == null)
						return;

					ggll = null;
					listener.Invalidate();
					sfl.LayoutChanging += OnFlyoutViewLayoutChanging;
				}
			}
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
			else if (e.IsOneOf(
				Shell.FlyoutFooterProperty,
				Shell.FlyoutFooterTemplateProperty))
				UpdateFlyoutFooter();
			else if (e.IsOneOf(
				Shell.FlyoutContentProperty,
				Shell.FlyoutContentTemplateProperty))
				UpdateFlyoutContent();
		}

		protected virtual void UpdateFlyoutContent()
		{
			if (!_rootView.IsAlive())
				return;

			var index = 0;
			if (_flyoutContentView != null)
			{
				index = _rootView.IndexOfChild(_flyoutContentView);
				_rootView.RemoveView(_flyoutContentView);
			}

			_flyoutContentView = CreateFlyoutContent(_rootView);
			if (_flyoutContentView == null)
				return;

			_rootView.AddView(_flyoutContentView, index);
			UpdateContentPadding();
		}


		void DisconnectRecyclerView()
		{
			if (_flyoutContentView.IsAlive() &&
				_flyoutContentView is RecyclerViewContainer rvc &&
				rvc.GetAdapter() is ShellFlyoutRecyclerAdapter sfra)
			{
				sfra.Disconnect();
			}
		}

		AView CreateFlyoutContent(ViewGroup rootView)
		{
			_rootView = rootView;
			if (_contentView != null)
			{
				var oldContentView = _contentView;
				_contentView = null;
				oldContentView.View = null;
			}

			DisconnectRecyclerView();

			var content = ((IShellController)ShellContext.Shell).FlyoutContent;
			if (content == null)
			{
				var lp = new CoordinatorLayout.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent);
				lp.Behavior = new AppBarLayout.ScrollingViewBehavior();
				var context = ShellContext.AndroidContext;
				var recyclerView = new RecyclerViewContainer(context)
				{
					LayoutParameters = lp
				};

				recyclerView.SetAdapter(new ShellFlyoutRecyclerAdapter(ShellContext, OnElementSelected));

				return recyclerView;
			}

			_contentView = new ShellViewRenderer(ShellContext.AndroidContext, content, MauiContext);

			_contentView.PlatformView.LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.MatchParent)
			{
				Behavior = new AppBarLayout.ScrollingViewBehavior()
			};

			return _contentView.PlatformView;
		}

		protected virtual void UpdateFlyoutHeader()
		{
			if (_headerView != null)
			{
				_headerView.LayoutChange -= OnHeaderViewLayoutChange;
				var oldHeaderView = _headerView;
				_headerView = null;
				_appBar.RemoveView(_headerFrameLayout);
				_headerFrameLayout.RemoveView(oldHeaderView);
				oldHeaderView.Dispose();
			}

			if (_flyoutHeader != null)
			{
				_flyoutHeader.MeasureInvalidated -= OnFlyoutHeaderMeasureInvalidated;
			}

			_flyoutHeader = ((IShellController)_shellContext.Shell).FlyoutHeader;
			if (_flyoutHeader != null)
			{
				_flyoutHeader.MeasureInvalidated += OnFlyoutHeaderMeasureInvalidated;

				_headerFrameLayout ??= new FrameLayout(_rootView.Context);

				_headerView = new HeaderContainer(_rootView.Context, _flyoutHeader, MauiContext)
				{
					MatchWidth = true
				};

				_headerView.LayoutChange += OnHeaderViewLayoutChange;

				_headerFrameLayout.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
				{
					ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll
				};

				_headerFrameLayout.AddView(_headerView);
				_appBar.AddView(_headerFrameLayout);

				UpdateFlyoutHeaderBehavior();
			}

			UpdateContentPadding();
		}

		void OnHeaderViewLayoutChange(object sender, AView.LayoutChangeEventArgs e)
		{
			// If the flyoutheader/footer have changed size then we need to 
			// remeasure the flyout content
			UpdateContentPadding();
		}

		protected virtual void UpdateFlyoutFooter()
		{
			if (_footerView != null)
			{
				var oldFooterView = _footerView;
				_rootView.RemoveView(_footerView);
				_footerView = null;
				oldFooterView.View = null;
			}

			var footer = ((IShellController)_shellContext.Shell).FlyoutFooter;

			if (footer == null)
				return;

			if (_flyoutWidth == 0)
				return;

			_footerView = new ContainerView(_shellContext.AndroidContext, footer, MauiContext)
			{
				MatchWidth = true
			};

			var footerViewLP = new CoordinatorLayout.LayoutParams(0, 0)
			{
				Gravity = (int)(GravityFlags.Bottom | GravityFlags.End)
			};

			UpdateFooterLayout(footerViewLP);
			_rootView.AddView(_footerView, footerViewLP);

			UpdateContentPadding();
		}

		void UpdateFooterLayout()
		{
			if (_flyoutWidth == 0 || _footerView == null)
			{
				return;
			}

			if (_footerView?.LayoutParameters is CoordinatorLayout.LayoutParams cl)
				UpdateFooterLayout(cl);
		}

		void UpdateFooterLayout(CoordinatorLayout.LayoutParams cl)
		{
			cl.Width = MeasureSpecMode.Exactly.MakeMeasureSpec(_flyoutWidth);
			cl.Height = MeasureSpecMode.Unspecified.MakeMeasureSpec(0);
		}

		int GetFooterViewTotalHeight()
		{
			var margin = Thickness.Zero;
			var measuredHeight = (FooterView?.MeasuredHeight ?? 0);
			if (_footerView?.View != null)
				margin = _footerView.View.Margin;

			return measuredHeight + (int)(_rootView.Context.ToPixels(margin.VerticalThickness));
		}

		bool UpdateContentPadding()
		{
			bool returnValue = false;
			var flyoutView = _flyoutContentView ?? _contentView?.PlatformView;

			if (flyoutView?.LayoutParameters is ViewGroup.MarginLayoutParams cl)
			{
				var viewMargin = _contentView?.View?.Margin ?? Thickness.Zero;
				var bottomOffset = GetFooterViewTotalHeight() + (int)_rootView.Context.ToPixels(viewMargin.Bottom);
				var headerViewHeight = _headerView?.MeasuredHeight ?? 0;

				// If the ScrollFlag on the LayoutParams are not set to zero than the content will automatically offset
				// By the height of the action bar so we can subtract that out of how much margin needs to be applied
				// to make room for the footer.
				//
				// The Flyout Content is already going to be offset by the app bar height
				// so we don't need to add that to the bottom margin
				if (headerViewHeight > 0 &&
					_headerFrameLayout?.LayoutParameters is AppBarLayout.LayoutParams alp &&
					alp.ScrollFlags != 0)
				{
					bottomOffset += _headerView?.MeasuredHeight ?? 0;

					var headerViewMinHeight = _headerView?.MinimumHeight ?? 0;
					if (bottomOffset > headerViewMinHeight)
						bottomOffset -= headerViewMinHeight;
				}

				if (cl.BottomMargin != bottomOffset)
				{
					cl.BottomMargin = bottomOffset;
					returnValue = true;
				}

				// For scrollable content we use padding so once it's all the way scrolled up
				// the bottom of the view isn't obscured by the footer view
				// If you try to use Margin the RecylcerView won't render anything.
				if (flyoutView is AndroidX.Core.View.IScrollingView &&
					flyoutView is ViewGroup vg)
				{
					if (vg.PaddingBottom != bottomOffset)
					{
						vg.SetPadding(0, 0, 0, bottomOffset);
						returnValue = true;
					}

					vg.SetClipToPadding(false);
				}

				// Set the XPLAT frame to the measured size of the platform
				// and propagate any margins set
				if (_contentView?.PlatformView != null)
				{
					var dpWidth = _rootView.Context.FromPixels(_contentView.PlatformView.MeasuredWidth);
					var dpHeight = _rootView.Context.FromPixels(_contentView.PlatformView.MeasuredHeight);
					_contentView.View.Frame = _contentView.View.ComputeFrame(new Graphics.Rect(0, 0, dpWidth, dpHeight));

					cl.LeftMargin = (int)_rootView.Context.ToPixels(viewMargin.Left);
					cl.TopMargin = (int)_rootView.Context.ToPixels(viewMargin.Top);
					cl.RightMargin = (int)_rootView.Context.ToPixels(viewMargin.Right);
					// bottom margin is set by the code above this
				}

			}

			return returnValue;
		}

		void OnFlyoutViewLayoutChanging()
		{
			// The second time this fires the non flyout part of the view
			// is visible to the user. I haven't found a better
			// mechanism to wire into in order to detect this
			if ((_initialLayoutChangeFired || FlyoutView.FlyoutBehavior == FlyoutBehavior.Locked) &&
				_flyoutContentView == null)
			{
				UpdateFlyoutContent();
			}

			_initialLayoutChangeFired = true;

			if (View?.MeasuredHeight > 0 &&
				View?.MeasuredWidth > 0 &&
				(_flyoutHeight != View.MeasuredHeight ||
				_flyoutWidth != View.MeasuredWidth)
			)
			{
				_flyoutHeight = View.MeasuredHeight;
				_flyoutWidth = View.MeasuredWidth;


				// We wait to instantiate the flyout footer until we know the WxH of the flyout container
				if (_footerView == null)
					UpdateFlyoutFooter();

				UpdateFooterLayout();
				UpdateContentPadding();
			}
		}

		void UpdateVerticalScrollMode()
		{
			if (_flyoutContentView is RecyclerView rv && rv.GetLayoutManager() is ScrollLayoutManager lm)
			{
				lm.ScrollVertically = _shellContext.Shell.FlyoutVerticalScrollMode;
			}
		}

		protected virtual void UpdateFlyoutBackground()
		{
			var brush = _shellContext.Shell.FlyoutBackground;

			if (Brush.IsNullOrEmpty(brush))
			{
				var color = _shellContext.Shell.FlyoutBackgroundColor;
				if (_defaultBackgroundColor == null)
					_defaultBackgroundColor = _rootView.Background;

				_rootView.Background = color == null ? _defaultBackgroundColor : new ColorDrawable(color.ToPlatform());
			}
			else
				_rootView.UpdateBackground(brush);

			UpdateFlyoutBgImageAsync();
		}

		void UpdateFlyoutBgImageAsync()
		{
			var imageSource = _shellContext.Shell.FlyoutBackgroundImage;

			if (imageSource == null || !_shellContext.Shell.IsSet(Shell.FlyoutBackgroundImageProperty))
			{
				if (_rootView.IndexOfChild(_bgImage) != -1)
					_rootView.RemoveView(_bgImage);
				return;
			}

			var services = MauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();

			_bgImage.Clear();
			imageSource.LoadImage(MauiContext, result =>
			{
				_bgImage.SetImageDrawable(result?.Value);

				if (!_rootView.IsAlive())
					return;

				if (result?.Value == null)
				{
					if (_rootView.IndexOfChild(_bgImage) != -1)
						_rootView.RemoveView(_bgImage);

					return;
				}

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
			});
		}

		protected virtual void UpdateFlyoutHeaderBehavior()
		{
			if (_headerView == null)
				return;

			_headerView.SetFlyoutHeaderBehavior(_shellContext.Shell.FlyoutHeaderBehavior);

			switch (_shellContext.Shell.FlyoutHeaderBehavior)
			{
				case FlyoutHeaderBehavior.Default:
				case FlyoutHeaderBehavior.Fixed:
					_headerFrameLayout.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
					{
						ScrollFlags = 0
					};
					break;
				case FlyoutHeaderBehavior.Scroll:
					_headerFrameLayout.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
					{
						ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll
					};
					break;
				case FlyoutHeaderBehavior.CollapseOnScroll:
					_headerFrameLayout.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
					{
						ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagExitUntilCollapsed |
							AppBarLayout.LayoutParams.ScrollFlagScroll
					};
					break;
			}
		}

		int _lastAppbarLayoutOffset;
		public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
		{
			if (_lastAppbarLayoutOffset == verticalOffset)
				return;

			_lastAppbarLayoutOffset = verticalOffset;

			if (_headerView == null)
				return;

			var headerBehavior = _shellContext.Shell.FlyoutHeaderBehavior;
			if (headerBehavior != FlyoutHeaderBehavior.CollapseOnScroll)
			{
				_headerView.SetParentTopPadding(0);
				return;
			}

			_headerView.SetParentTopPadding(-verticalOffset);

		}

		internal void Disconnect()
		{
			if (_shellContext?.Shell != null)
				_shellContext.Shell.PropertyChanged -= OnShellPropertyChanged;

			if (_flyoutHeader != null)
				_flyoutHeader.MeasureInvalidated -= OnFlyoutHeaderMeasureInvalidated;

			_flyoutHeader = null;

			if (_footerView != null)
				_footerView.View = null;

			_headerView?.Disconnect();
			DisconnectRecyclerView();

			if (_contentView != null)
				_contentView.View = null;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				Disconnect();

				if (_appBar != null)
				{
					_appBar.RemoveOnOffsetChangedListener(this);

					if (_headerFrameLayout != null)
						_appBar.RemoveView(_headerFrameLayout);
				}

				if (_rootView != null && _footerView != null)
					_rootView.RemoveView(_footerView);

				if (View != null && View is ShellFlyoutLayout sfl)
					sfl.LayoutChanging -= OnFlyoutViewLayoutChanging;

				if (_headerView != null)
					_headerView.LayoutChange -= OnHeaderViewLayoutChange;

				if (_contentView != null)
					_contentView.View = null;

				_flyoutContentView?.Dispose();
				_headerView?.Dispose();

				_rootView?.Dispose();
				_defaultBackgroundColor?.Dispose();
				_bgImage?.Dispose();

				_contentView = null;
				_rootView = null;
				_headerView = null;
				_shellContext = null;
				_appBar = null;
				_flyoutContentView = null;
				_defaultBackgroundColor = null;
				_bgImage = null;
				_footerView = null;
			}

			base.Dispose(disposing);
		}

		// This view lets us use the top padding to "squish" the content down
		public class HeaderContainer : ContainerView
		{
			bool _isdisposed = false;
			private FlyoutHeaderBehavior _flyoutHeaderBehavior;

			public HeaderContainer(Context context, View view, IMauiContext mauiContext) : base(context, view, mauiContext)
			{
				Initialize(view);
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

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				UpdateMinimumHeight();

				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

				if (Parent is AView parentFrameLayout)
				{
					var headerOffsetFromParentContainer = (int)(DesiredSize.Height - MeasuredHeight);
					parentFrameLayout.SetPadding(0, headerOffsetFromParentContainer, 0, 0);

					// If the headerview has a minimum height set then we'll use
					// that for the minimum height on the header views frame layout parent
					// container
					// This is really only relevant with CollapseOnScroll where
					// the min height on the header view gets set to 56
					parentFrameLayout.SetMinimumHeight((int)MinimumSize.Height);
				}
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				UpdateElevation();

				base.OnLayout(changed, l, t, r, b);
			}

			protected override void Dispose(bool disposing)
			{
				if (_isdisposed)
					return;

				_isdisposed = true;

				if (disposing)
					Disconnect();

				base.Dispose(disposing);
			}

			internal void Disconnect()
			{
				if (View != null)
				{
					View.PropertyChanged -= OnViewPropertyChanged;
					View = null;
				}
			}

			internal void SetFlyoutHeaderBehavior(FlyoutHeaderBehavior flyoutHeaderBehavior)
			{
				if (_flyoutHeaderBehavior == flyoutHeaderBehavior)
					return;

				_flyoutHeaderBehavior = flyoutHeaderBehavior;
				UpdateMinimumHeight();
			}

			void UpdateMinimumHeight()
			{
				var minHeight = 0;

				if (View?.MinimumHeightRequest > 0)
				{
					minHeight = (int)Context.ToPixels(View.MinimumHeightRequest);
				}
				else if (_flyoutHeaderBehavior == FlyoutHeaderBehavior.CollapseOnScroll)
				{
					minHeight = Context.GetActionBarHeight();
				}
				else
				{
					minHeight = 0;
				}

				if (MinimumHeight != minHeight)
					this.SetMinimumHeight(minHeight);

				if (Parent is AView frameLayoutView &&
					minHeight > frameLayoutView.MinimumHeight)
				{
					frameLayoutView.SetMinimumHeight(minHeight);
				}

				if (PlatformView.MinimumHeight != minHeight)
				{
					PlatformView.SetMinimumHeight(minHeight);
				}
			}
		}
	}

	class RecyclerViewContainer : RecyclerView
	{
		bool _disposed;
		ScrollLayoutManager _layoutManager;

		public RecyclerViewContainer(Context context) : base(context)
		{
			SetClipToPadding(false);
			SetLayoutManager(_layoutManager = new ScrollLayoutManager(context, (int)Orientation.Vertical, false));
			SetLayoutManager(new LinearLayoutManager(context, (int)Orientation.Vertical, false));
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (disposing)
			{
				SetLayoutManager(null);
				var adapter = this.GetAdapter();
				SetAdapter(null);
				adapter?.Dispose();
				_layoutManager?.Dispose();
				_layoutManager = null;
			}

			base.Dispose(disposing);
		}
	}

	internal sealed class ScrollLayoutManager : LinearLayoutManager
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
