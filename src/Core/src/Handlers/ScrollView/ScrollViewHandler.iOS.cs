using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, UIScrollView>, ICrossPlatformLayout, IScrollViewportProvider
	{
		readonly ScrollEventProxy _eventProxy = new();

		internal ScrollToRequest? PendingScrollToRequest { get; private set; }

		Thickness IScrollViewportProvider.ViewportInsets
		{
			get
			{
				if (PlatformView is not { } platformView)
				{
					return default;
				}

				// The safe area MauiScrollView bakes into the content obscures the viewport just
				// like a UIKit inset but never appears in AdjustedContentInset (issue #36801)
				var inset = platformView.AdjustedContentInset;
				var baked = GetSafeAreaBakedIntoContent(platformView);
				return new Thickness(
					inset.Left + baked.Left,
					inset.Top + baked.Top,
					inset.Right + baked.Right,
					inset.Bottom + baked.Bottom);
			}
		}

		Thickness IScrollViewportProvider.ContentCoordinateInsets
		{
			get
			{
				if (PlatformView is not { } platformView)
				{
					return default;
				}

				var baked = GetSafeAreaBakedIntoContent(platformView);
				return new Thickness(baked.Left, baked.Top, baked.Right, baked.Bottom);
			}
		}

		static SafeAreaPadding GetSafeAreaBakedIntoContent(UIScrollView platformView) =>
			(platformView as MauiScrollView)?.SafeAreaBakedIntoContent ?? SafeAreaPadding.Empty;

		protected override UIScrollView CreatePlatformView()
		{
			return new MauiScrollView();
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			
			if (PlatformView is MauiScrollView mauiScrollView)
				mauiScrollView.View = view;
				
		}

		protected override void ConnectHandler(UIScrollView platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is ICrossPlatformLayoutBacking platformScrollView)
			{
				platformScrollView.CrossPlatformLayout = this;
			}

			_eventProxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(UIScrollView platformView)
		{
			if (platformView is ICrossPlatformLayoutBacking platformScrollView)
			{
				platformScrollView.CrossPlatformLayout = null;
			}

			base.DisconnectHandler(platformView);

			if (PendingScrollToRequest is not null)
			{
				VirtualView?.ScrollFinished();
				PendingScrollToRequest = null;
			}
			_eventProxy.Disconnect(platformView);
		}

		internal void ProcessPendingScrollRequest()
		{
			if (PendingScrollToRequest is { } pending)
			{
				MapRequestScrollTo(this, VirtualView, pending);
				PendingScrollToRequest = null;
			}
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.PlatformView == null || handler.MauiContext == null)
				return;

			// We'll use the local cross-platform layout methods defined in our handler (which wrap the ScrollView's default methods)
			// so we can normalize the behavior of the scrollview to match the other platforms
			UpdateContentView(scrollView, handler);
		}

		// We don't actually have this mapped because we don't need it, but we can't remove it because it's public
		public static void MapContentSize(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView?.UpdateContentSize(scrollView.ContentSize);
		}

		public static void MapIsEnabled(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView?.UpdateIsEnabled(scrollView);
		}

		public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView?.UpdateHorizontalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView?.UpdateVerticalScrollBarVisibility(scrollView.VerticalScrollBarVisibility);
		}

		public static void MapOrientation(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler?.PlatformView is not { } platformView)
			{
				return;
			}

			platformView.UpdateIsEnabled(scrollView);
			platformView.InvalidateMeasure(scrollView);
		}

		public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is ScrollToRequest request)
			{
				var uiScrollView = handler.PlatformView;

				if (uiScrollView.ContentSize == CGSize.Empty && handler is ScrollViewHandler scrollViewHandler)
				{
					// If the ContentSize of the UIScrollView has not yet been defined,
					// we create a pending scroll request that we will launch after performing the Layout and sizing process.
					scrollViewHandler.PendingScrollToRequest = request;
					return;
				}

				var target = GetTargetContentOffset(uiScrollView, request);

				// Compare at device-pixel resolution: UIKit rounds resting offsets to physical
				// pixels while the inset-derived target is fractional, so an exact comparison can
				// miss by a sub-pixel amount — and an animated SetContentOffset for a sub-pixel
				// delta may never raise ScrollAnimationEnded, leaving the caller's task pending
				// forever (there is no timeout anywhere in that chain).
				var pixelTolerance = 1 / (uiScrollView.Window?.Screen ?? UIScreen.MainScreen).Scale;
				bool alreadyAtTarget = uiScrollView.ContentOffset.IsCloseTo(target, pixelTolerance);

				if (!alreadyAtTarget)
				{
					uiScrollView.SetContentOffset(target, !request.Instant);
				}

				if (request.Instant || alreadyAtTarget)
				{
					scrollView.ScrollFinished();
				}
			}
		}

		// Cross-platform scroll coordinates treat (0,0) as the top of the content, but with content
		// insets (e.g. a scroll view consuming the safe area) the native rest offset is
		// (-adjustedInset.Left, -adjustedInset.Top) and the native maximum extends past
		// ContentSize - Bounds by the trailing insets. Translate the request into native offset
		// space and clamp against the inset-aware range (issue #36801).
		static CGPoint GetTargetContentOffset(UIScrollView uiScrollView, ScrollToRequest request)
		{
			var adjustedInset = uiScrollView.AdjustedContentInset;
			var bounds = uiScrollView.Bounds;

			// MauiScrollView reports the extent to clamp against, since only it knows when its
			// arrange baked safe-area padding into ContentSize that UIKit is also applying
			// through AdjustedContentInset
			var contentSize = (uiScrollView as MauiScrollView)?.ScrollableContentSize ?? uiScrollView.ContentSize;
			var contentWidth = (double)contentSize.Width;
			var contentHeight = (double)contentSize.Height;

			var minScrollHorizontal = -(double)adjustedInset.Left;
			var minScrollVertical = -(double)adjustedInset.Top;
			var maxScrollHorizontal = Math.Max(minScrollHorizontal, contentWidth + adjustedInset.Right - bounds.Width);
			var maxScrollVertical = Math.Max(minScrollVertical, contentHeight + adjustedInset.Bottom - bounds.Height);

			return new CGPoint(
				Math.Clamp(request.HorizontalOffset - (double)adjustedInset.Left, minScrollHorizontal, maxScrollHorizontal),
				Math.Clamp(request.VerticalOffset - (double)adjustedInset.Top, minScrollVertical, maxScrollVertical));
		}

		static void UpdateContentView(IScrollView scrollView, IScrollViewHandler handler)
		{
			bool changed = false;
			var platformView = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var mauiContext = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (platformView.GetContentView() is { } currentContentPlatformView)
			{
				currentContentPlatformView.RemoveFromSuperview();
				changed = true;
			}

			if (scrollView.PresentedContent is { } content)
			{
				var platformContent = content.ToPlatform(mauiContext);
				platformContent.Tag = MauiScrollView.ContentTag;
				platformView.AddSubview(platformContent);
				changed = true;
			}

			if (changed)
			{
				platformView.InvalidateMeasure();
			}
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			if (VirtualView is not { } scrollView)
			{
				return Size.Zero;
			}

			var scrollOrientation = scrollView.Orientation;
			var contentWidthConstraint = scrollOrientation is ScrollOrientation.Horizontal or ScrollOrientation.Both ? double.PositiveInfinity : widthConstraint;
			var contentHeightConstraint = scrollOrientation is ScrollOrientation.Vertical or ScrollOrientation.Both ? double.PositiveInfinity : heightConstraint;
			var contentSize = MeasureContent(scrollView, scrollView.Padding, contentWidthConstraint, contentHeightConstraint);

			// Our target size is the smaller of it and the constraints
			var width = contentSize.Width <= widthConstraint ? contentSize.Width : widthConstraint;
			var height = contentSize.Height <= heightConstraint ? contentSize.Height : heightConstraint;

			width = ViewHandlerExtensions.ResolveConstraints(width, scrollView.Width, scrollView.MinimumWidth, scrollView.MaximumWidth);
			height = ViewHandlerExtensions.ResolveConstraints(height, scrollView.Height, scrollView.MinimumHeight, scrollView.MaximumHeight);

			return new Size(width, height);
		}

		static Size MeasureContent(IContentView contentView, Thickness inset, double widthConstraint, double heightConstraint)
		{
			var content = contentView.PresentedContent;

			var contentSize = Size.Zero;

			if (!double.IsInfinity(widthConstraint) && Dimension.IsExplicitSet(contentView.Width))
			{
				widthConstraint = contentView.Width;
			}

			if (!double.IsInfinity(heightConstraint) && Dimension.IsExplicitSet(contentView.Height))
			{
				heightConstraint = contentView.Height;
			}

			if (content is not null)
			{
				contentSize = content.Measure(widthConstraint - inset.HorizontalThickness,
					heightConstraint - inset.VerticalThickness);
			}

			return new Size(contentSize.Width + inset.HorizontalThickness, contentSize.Height + inset.VerticalThickness);
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			return (VirtualView as ICrossPlatformLayout)?.CrossPlatformArrange(bounds) ?? Size.Zero;
		}

		class ScrollEventProxy
		{
			WeakReference<IScrollView>? _virtualView;

			IScrollView? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(IScrollView virtualView, UIScrollView platformView)
			{
				_virtualView = new(virtualView);

				platformView.Scrolled += Scrolled;
				platformView.ScrollAnimationEnded += ScrollAnimationEnded;
			}

			public void Disconnect(UIScrollView platformView)
			{
				_virtualView = null;

				platformView.Scrolled -= Scrolled;
				platformView.ScrollAnimationEnded -= ScrollAnimationEnded;
			}

			void ScrollAnimationEnded(object? sender, EventArgs e)
			{
				VirtualView?.ScrollFinished();
			}

			void Scrolled(object? sender, EventArgs e)
			{
				if (sender is UIScrollView platformView)
				{
					PublishScrollOffsets(VirtualView, platformView);
				}
			}
		}

		void IScrollViewportProvider.NotifyInsetsChanged()
		{
			if (PlatformView is not { } platformView || VirtualView is not { } virtualView)
			{
				return;
			}

			var (horizontalOffset, verticalOffset) = GetContentCoordinateOffsets(platformView);

			// Nothing scrolled — the derived offsets moved because the inset did — so refresh
			// the values without letting the view raise a scrolled notification for it
			if (virtualView is IScrollOffsetReceiver receiver)
			{
				receiver.UpdateScrollOffsets(horizontalOffset, verticalOffset);
			}
			else
			{
				virtualView.HorizontalOffset = horizontalOffset;
				virtualView.VerticalOffset = verticalOffset;
			}
		}

		// Report offsets in cross-platform content coordinates: with content insets the native
		// rest offset is (-adjustedInset.Left, -adjustedInset.Top), which maps to (0,0)
		// cross-platform so ScrollToAsync(ScrollX, ScrollY, ...) round-trips (issue #36801).
		static void PublishScrollOffsets(IScrollView? virtualView, UIScrollView platformView)
		{
			if (virtualView is null)
			{
				return;
			}

			(virtualView.HorizontalOffset, virtualView.VerticalOffset) = GetContentCoordinateOffsets(platformView);
		}

		static (double HorizontalOffset, double VerticalOffset) GetContentCoordinateOffsets(UIScrollView platformView)
		{
			var adjustedInset = platformView.AdjustedContentInset;
			return (platformView.ContentOffset.X + adjustedInset.Left, platformView.ContentOffset.Y + adjustedInset.Top);
		}
	}
}
