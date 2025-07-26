using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, UIScrollView>, ICrossPlatformLayout
	{
		readonly ScrollEventProxy _eventProxy = new();

		internal ScrollToRequest? PendingScrollToRequest { get; private set; }

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

			PendingScrollToRequest = null;
			_eventProxy.Disconnect(platformView);
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

				var availableScrollHeight = uiScrollView.ContentSize.Height - uiScrollView.Frame.Height;
				var availableScrollWidth = uiScrollView.ContentSize.Width - uiScrollView.Frame.Width;
				var minScrollHorizontal = Math.Min(request.HorizontalOffset, availableScrollWidth);
				var minScrollVertical = Math.Min(request.VerticalOffset, availableScrollHeight);
				uiScrollView.SetContentOffset(new CGPoint(minScrollHorizontal, minScrollVertical), !request.Instant);

				if (request.Instant)
				{
					scrollView.ScrollFinished();
				}
			}
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
				if (VirtualView == null)
				{
					return;
				}

				if (sender is not UIScrollView platformView)
				{
					return;
				}

				VirtualView.HorizontalOffset = platformView.ContentOffset.X;
				VirtualView.VerticalOffset = platformView.ContentOffset.Y;
			}
		}
	}
}
