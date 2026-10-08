using Android.Views;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Layouts.LayoutExtensions;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, MauiScrollView>, ICrossPlatformLayout
	{
		const string InsetPanelTag = "MAUIContentInsetPanel";

		protected override MauiScrollView CreatePlatformView()
		{
			var scrollView = new MauiScrollView(
				new ContextThemeWrapper(MauiContext!.Context, Resource.Style.scrollViewTheme), null!,
					Resource.Attribute.scrollViewStyle)
			{
				ClipToOutline = true,
				FillViewport = true
			};

			return scrollView;
		}

		protected override void ConnectHandler(MauiScrollView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.ScrollChange += ScrollChange;
		}

		protected override void DisconnectHandler(MauiScrollView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.ScrollChange -= ScrollChange;
		}
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			PlatformView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var Context = MauiContext?.Context;
			var platformView = PlatformView;
			var virtualView = VirtualView;

			if (platformView == null || virtualView == null || Context == null)
			{
				return Size.Zero;
			}

			// Create a spec to handle the native measure
			var widthSpec = Context.CreateMeasureSpec(widthConstraint, virtualView.Width, virtualView.MinimumWidth, virtualView.MaximumWidth);
			var heightSpec = Context.CreateMeasureSpec(heightConstraint, virtualView.Height, virtualView.MinimumHeight, virtualView.MaximumHeight);

			if (platformView.FillViewport)
			{
				/*	With FillViewport active, the Android ScrollView will measure the content at least once; if it is 
					smaller than the ScrollView's viewport, it measure a second time at the size of the viewport
					so that the content can properly fill the whole viewport. But it will only do this if the measurespec
					is set to Exactly. So if we want our ScrollView to Fill the space in the scroll direction, we need to
					adjust the MeasureSpec accordingly. If the ScrollView is not set to Fill, we can just leave the spec
					alone and the ScrollView will size to its content as usual. */

				var orientation = virtualView.Orientation;

				if (!double.IsInfinity(widthConstraint))
					widthSpec = AdjustSpecForAlignment(widthSpec, virtualView.HorizontalLayoutAlignment);

				if (!double.IsInfinity(heightConstraint))
					heightSpec = AdjustSpecForAlignment(heightSpec, virtualView.VerticalLayoutAlignment);
			}

			platformView.Measure(widthSpec, heightSpec);

			// Convert back to xplat sizes for the return value
			return Context.FromPixels(platformView.MeasuredWidth, platformView.MeasuredHeight);
		}

		static int AdjustSpecForAlignment(int measureSpec, Primitives.LayoutAlignment alignment)
		{
			if (alignment == Primitives.LayoutAlignment.Fill && measureSpec.GetMode() == MeasureSpecMode.AtMost)
			{
				return MeasureSpecMode.Exactly.MakeMeasureSpec(measureSpec.GetSize());
			}

			return measureSpec;
		}

		void ScrollChange(object? sender, AndroidX.Core.Widget.NestedScrollView.ScrollChangeEventArgs e)
		{
			var platformView = sender as MauiScrollView;

			if (platformView?.Context is null)
			{
				return;
			}

			int scrollX = e.ScrollX;
			int scrollY = e.ScrollY;

			if (VirtualView.Orientation == ScrollOrientation.Both)
			{
				if (scrollX == 0)
				{
					// Need to pass the native HorizontalScrollView's ScrollX position to the virtual view to resolve
					// the zero scroll offset issue since the framework returns an improper ScrollX value.
					scrollX = platformView.HorizontalScrollOffset;
				}

				if (scrollY == 0)
				{
					// Pass the native ScrollView's ScrollY to the virtual view to maintain the correct vertical offset.
					scrollY = platformView.ScrollY;
				}
			}

			VirtualView.HorizontalOffset = platformView.Context.FromPixels(scrollX);
			VirtualView.VerticalOffset = platformView.Context.FromPixels(scrollY);
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.PlatformView == null || handler.MauiContext == null)
				return;

			if (handler is not ICrossPlatformLayout crossPlatformLayout)
			{
				return;
			}

			UpdateInsetView(scrollView, handler, crossPlatformLayout);
		}

		public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.SetHorizontalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.SetVerticalScrollBarVisibility(scrollView.VerticalScrollBarVisibility);
		}

		public static void MapOrientation(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.SetOrientation(scrollView.Orientation);
		}

		internal static void MapFlowDirection(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.PlatformView is MauiScrollView mauiScrollView && scrollView is IView view)
			{
				mauiScrollView.UpdateFlowDirection(view);
			}
		}

		public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is not ScrollToRequest request)
			{
				return;
			}

			var context = handler.PlatformView.Context;

			if (context == null)
			{
				return;
			}

			// A view that has been laid out at least once has geometry worth scrolling against: the
			// platform clamps the offsets to the last completed layout. IsLayoutRequested must not gate
			// this decision - see WaitToScrollOnLayout for why waiting on it never ends.
			if (!handler.PlatformView.IsLaidOut)
			{
				WaitToScrollOnLayout(handler, request);

				return;
			}

			ServeScrollTo(handler, request);
		}

		// Serves the request against the platform view's current geometry, without consulting the layout
		// flags: the platform clamps the offsets to what it can measure, so serving is safe - and, unlike
		// deferring, terminating - even for a view that has never been laid out.
		static void ServeScrollTo(IScrollViewHandler handler, ScrollToRequest request)
		{
			var context = handler.PlatformView.Context;

			if (context == null)
			{
				return;
			}

			var horizontalOffsetDevice = (int)context.ToPixels(request.HorizontalOffset);
			var verticalOffsetDevice = (int)context.ToPixels(request.VerticalOffset);

			handler.PlatformView.ScrollTo(horizontalOffsetDevice, verticalOffsetDevice,
				request.Instant, () =>
				{
					if (handler.IsConnected())
					{
						handler.VirtualView.ScrollFinished();
					}
				});
		}

		const int LayoutWaitTimeoutMillis = 4000;

		// Waits for the platform view's first layout, then serves 'request' - once, and only once.
		//
		// Event-driven and bounded, unlike the flag-polled re-post it replaces: ViewRootImpl swallows
		// requestLayout() while it is handling a layout-inside-layout request (the
		// mHandlingLayoutInLayoutRequest guard), which leaves IsLayoutRequested set with no traversal
		// ever scheduled again. A retry loop gated on that flag therefore spins at full looper
		// throughput - the livelock - where an event listener cannot, because it runs exactly once per
		// traversal and stops being registered at all as soon as it fires.
		//
		// Deferral is bounded so that it can never park a request the way the retry loop did: whichever
		// signal arrives first serves the request, and a view that never lays out is served by the
		// timeout. Nothing re-enters MapRequestScrollTo from here, so a deferred request cannot re-arm
		// the wait behind itself.
		static void WaitToScrollOnLayout(IScrollViewHandler handler, ScrollToRequest request)
		{
			var platformView = handler.PlatformView;
			var observer = platformView.ViewTreeObserver;
			var waiter = new LayoutWaiter(platformView, handler, request);

			if (observer is { IsAlive: true })
			{
				observer.AddOnGlobalLayoutListener(waiter);
			}

			platformView.PostDelayed(waiter.OnTimeout, LayoutWaitTimeoutMillis);

			// Arm the layout this is waiting for. A view that has never been laid out normally has a
			// traversal on its way already; a latched IsLayoutRequested flag is not one, so it is left
			// alone - requesting again would only re-latch it, which is how the retry loop got stuck.
			if (!platformView.IsLayoutRequested)
			{
				platformView.RequestLayout();
			}
		}

		sealed class LayoutWaiter : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
		{
			readonly MauiScrollView _view;
			readonly IScrollViewHandler _handler;
			readonly ScrollToRequest _request;
			int _fired;

			public LayoutWaiter(MauiScrollView view, IScrollViewHandler handler, ScrollToRequest request)
			{
				_view = view;
				_handler = handler;
				_request = request;
			}

			public void OnGlobalLayout()
			{
				// Fires for every layout of the window, not just ours: the wait is only satisfied once
				// the view that deferred has itself been laid out.
				if (_view.IsLaidOut)
				{
					Fire();
				}
			}

			public void OnTimeout() =>
				Fire();

			void Fire()
			{
				// One-shot: the timeout is still pending when the layout event wins the race (and vice
				// versa), and the request must be served exactly once either way.
				if (System.Threading.Interlocked.Exchange(ref _fired, 1) != 0)
				{
					return;
				}

				var observer = _view.ViewTreeObserver;

				if (observer is { IsAlive: true })
				{
					observer.RemoveOnGlobalLayoutListener(this);
				}

				if (_handler.IsConnected())
				{
					ServeScrollTo(_handler, _request);
				}
			}
		}

		/*
			Problem 1: Android treats Padding differently than what we want for MAUI; Padding creates space
			_around_ the scrollable area, rather than padding the content inside of it. 
			
			Problem 2: The Android ScrollView control will ignore the cross-platform Margin of its content when 
			making native Measure calls. The internal content size values recorded by the native ScrollView will 
			not account for the margin, and the control won't scroll all the way to the bottom of the content. 

			To handle both issues, we insert a container ContentViewGroup which always lays out at the origin but provides 
			both the Padding and the Margin for the content. The extra layer also provides cross-platform measurement and layout.
			The extra layer uses the native ContentViewGroup control (the same one we already use as the backing for ContentView, Page, etc.). 

			The methods below exist to support inserting/updating the extra padding/margin layer.
		*/

		static ContentViewGroup? FindInsetPanel(IScrollViewHandler handler)
		{
			return handler.PlatformView.FindViewWithTag(InsetPanelTag) as ContentViewGroup;
		}

		static void UpdateInsetView(IScrollView scrollView, IScrollViewHandler handler, ICrossPlatformLayout crossPlatformLayout)
		{
			if (handler.MauiContext is null)
			{
				return;
			}

			// Find existing inset panel once
			var currentPaddingLayer = FindInsetPanel(handler);

			// If PresentedContent is null, clean up any existing content and return
			if (scrollView.PresentedContent is null)
			{
				currentPaddingLayer?.RemoveAllViews();
				return;
			}

			var nativeContent = scrollView.PresentedContent.ToPlatform(handler.MauiContext);

			if (currentPaddingLayer is not null)
			{
				UpdateClipForShadow(currentPaddingLayer, handler.PlatformView, scrollView.PresentedContent);

				// Only update if content has changed or is missing
				if (currentPaddingLayer.ChildCount == 0 || currentPaddingLayer.GetChildAt(0) != nativeContent)
				{
					currentPaddingLayer.RemoveAllViews();
					currentPaddingLayer.AddView(nativeContent);
				}
			}
			else
			{
				InsertInsetView(handler, scrollView, nativeContent, crossPlatformLayout);
			}
		}

		static void InsertInsetView(IScrollViewHandler handler, IScrollView scrollView, View nativeContent, ICrossPlatformLayout crossPlatformLayout)
		{
			if (scrollView.PresentedContent == null || handler.MauiContext?.Context == null)
			{
				return;
			}

			var paddingShim = new ContentViewGroup(handler.MauiContext.Context)
			{
				CrossPlatformLayout = crossPlatformLayout,
				Tag = InsetPanelTag
			};

			UpdateClipForShadow(paddingShim, handler.PlatformView, scrollView.PresentedContent);

			handler.PlatformView.RemoveAllViews();
			paddingShim.AddView(nativeContent);
			handler.PlatformView.SetContent(paddingShim);
		}

		static void UpdateClipForShadow(ContentViewGroup paddingShim, MauiScrollView scrollView, IView? content)
		{
			bool hasShadow = content?.Shadow is not null;
			paddingShim.SetClipChildren(!hasShadow);
			paddingShim.SetClipToPadding(!hasShadow);
			scrollView.SetClipChildren(!hasShadow);
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			if (VirtualView is not { } scrollView)
			{
				return Size.Zero;
			}

			var padding = scrollView.Padding;

			if (scrollView.PresentedContent == null)
			{
				return new Size(padding.HorizontalThickness, padding.VerticalThickness);
			}

			var scrollOrientation = scrollView.Orientation;
			var contentWidthConstraint = scrollOrientation is ScrollOrientation.Horizontal or ScrollOrientation.Both ? double.PositiveInfinity : widthConstraint;
			var contentHeightConstraint = scrollOrientation is ScrollOrientation.Vertical or ScrollOrientation.Both ? double.PositiveInfinity : heightConstraint;
			var contentSize = scrollView.MeasureContent(scrollView.Padding, contentWidthConstraint, contentHeightConstraint, !double.IsInfinity(contentWidthConstraint), !double.IsInfinity(contentHeightConstraint));

			if (double.IsInfinity(widthConstraint))
			{
				widthConstraint = contentSize.Width;
			}

			if (double.IsInfinity(heightConstraint))
			{
				heightConstraint = contentSize.Height;
			}

			return contentSize.AdjustForFill(new Rect(0, 0, widthConstraint, heightConstraint), scrollView.PresentedContent);
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds) =>
			(VirtualView as ICrossPlatformLayout)?.CrossPlatformArrange(bounds) ?? Size.Zero;
	}
}
