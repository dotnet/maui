using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using UIKit;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, UIScrollView>, ICrossPlatformLayout
	{
		const nint ContentPanelTag = 0x845fed;

		readonly ScrollEventProxy _eventProxy = new();

		public override bool NeedsContainer
		{
			get
			{
				//if we are being wrapped by a BorderView we need a container
				//so we can handle masks and clip shapes
				if (VirtualView?.Parent is IBorderView)
				{
					return true;
				}
				return base.NeedsContainer;
			}
		}

		internal ScrollToRequest? PendingScrollToRequest { get; private set; }

		protected override UIScrollView CreatePlatformView()
		{
			return new MauiScrollView();
		}

		protected override void ConnectHandler(UIScrollView platformView)
		{
			base.ConnectHandler(platformView);

			_eventProxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(UIScrollView platformView)
		{
			base.DisconnectHandler(platformView);

			PendingScrollToRequest = null;
			_eventProxy.Disconnect(platformView);
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.PlatformView == null || handler.MauiContext == null)
				return;

			if (handler is not ICrossPlatformLayout crossPlatformLayout)
			{
				return;
			}

			// We'll use the local cross-platform layout methods defined in our handler (which wrap the ScrollView's default methods)
			// so we can normalize the behavior of the scrollview to match the other platforms
			UpdateContentView(scrollView, handler, crossPlatformLayout);
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
			if (handler?.PlatformView is not UIScrollView uiScrollView)
			{
				return;
			}

			// If the UIScrollView hasn't been laid out yet, this will basically do nothing.
			// If it has been, we can just update the ContentSize here and get the new orientation working
			// without having to re-layout the ScrollView

			var fullContentSize = scrollView.PresentedContent?.DesiredSize ?? Size.Zero;

			var viewportSize = GetViewportSize(uiScrollView);
			var viewportWidth = viewportSize.Width;
			var viewportHeight = viewportSize.Height;

			SetContentSizeForOrientation(uiScrollView, viewportWidth, viewportHeight, scrollView.Orientation, fullContentSize);
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
				uiScrollView.SetContentOffset(new CoreGraphics.CGPoint(minScrollHorizontal, minScrollVertical), !request.Instant);

				if (request.Instant)
				{
					scrollView.ScrollFinished();
				}
			}
		}

		// Find the internal ContentView; it may not be Subviews[0] because of the scrollbars
		static ContentView? GetContentView(UIScrollView scrollView)
		{
			for (int n = 0; n < scrollView.Subviews.Length; n++)
			{
				if (scrollView.Subviews[n] is ContentView contentView)
				{
					if (contentView.Tag is nint tag && tag == ContentPanelTag)
					{
						return contentView;
					}
				}
			}

			return null;
		}

		static void UpdateContentView(IScrollView scrollView, IScrollViewHandler handler, ICrossPlatformLayout crossPlatformLayout)
		{
			if (scrollView.PresentedContent == null || handler.MauiContext == null)
			{
				return;
			}

			var platformScrollView = handler.PlatformView;
			var nativeContent = scrollView.PresentedContent.ToPlatform(handler.MauiContext);

			if (GetContentView(platformScrollView) is ContentView currentContentContainer)
			{
				if (currentContentContainer.Subviews.Length == 0 || currentContentContainer.Subviews[0] != nativeContent)
				{
					currentContentContainer.ClearSubviews();
					currentContentContainer.AddSubview(nativeContent);
					currentContentContainer.View = scrollView.PresentedContent;
				}
			}
			else
			{
				InsertContentView(platformScrollView, scrollView, nativeContent, crossPlatformLayout);
			}
		}

		static void InsertContentView(UIScrollView platformScrollView, IScrollView scrollView, UIView platformContent, ICrossPlatformLayout crossPlatformLayout)
		{
			if (scrollView.PresentedContent == null)
			{
				return;
			}

			var contentContainer = new ContentView()
			{
				View = scrollView.PresentedContent,
				Tag = ContentPanelTag
			};

			// This is where we normally would inject the cross-platform ScrollView's layout logic; instead, we're injecting the
			// methods from this handler so it can make some adjustments for things like Padding before the default logic is invoked
			contentContainer.CrossPlatformLayout = crossPlatformLayout;

			platformScrollView.ClearSubviews();
			contentContainer.AddSubview(platformContent);
			platformScrollView.AddSubview(contentContainer);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var virtualView = VirtualView;
			var crossPlatformLayout = virtualView as ICrossPlatformLayout;
			var platformView = PlatformView;

			if (platformView == null || virtualView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var padding = virtualView.Padding;

			// Account for the ScrollView Padding before measuring the content
			widthConstraint = AccountForPadding(widthConstraint, padding.HorizontalThickness);
			heightConstraint = AccountForPadding(heightConstraint, padding.VerticalThickness);

			var crossPlatformContentSize = crossPlatformLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);

			// Add the padding back in for the final size
			crossPlatformContentSize.Width += padding.HorizontalThickness;
			crossPlatformContentSize.Height += padding.VerticalThickness;

			var viewportWidth = Math.Min(crossPlatformContentSize.Width, widthConstraint);
			var viewportHeight = Math.Min(crossPlatformContentSize.Height, heightConstraint);

			// Since the UIScrollView might not be laid out yet, we can't rely on its Bounds for the viewport height/width
			// So we'll use the constraints instead.
			SetContentSizeForOrientation(platformView, widthConstraint, heightConstraint, virtualView.Orientation, crossPlatformContentSize);

			var finalWidth = ViewHandlerExtensions.ResolveConstraints(viewportWidth, virtualView.Width, virtualView.MinimumWidth, virtualView.MaximumWidth);
			var finalHeight = ViewHandlerExtensions.ResolveConstraints(viewportHeight, virtualView.Height, virtualView.MinimumHeight, virtualView.MaximumHeight);

			return new Size(finalWidth, finalHeight);
		}

		public override void PlatformArrange(Rect rect)
		{
			base.PlatformArrange(rect);

			// Ensure that the content container for the ScrollView gets arranged, and is large enough
			// to contain the ScrollView's content

			var contentView = GetContentView(PlatformView);

			if (contentView == null)
			{
				return;
			}

			var desiredSize = VirtualView.PresentedContent?.DesiredSize ?? Size.Zero;
			var scrollViewPadding = VirtualView.Padding;
			var platformViewBounds = PlatformView.Bounds;

			var contentBounds = new CGRect(0, 0,
				Math.Max(desiredSize.Width + scrollViewPadding.HorizontalThickness, platformViewBounds.Width),
				Math.Max(desiredSize.Height + scrollViewPadding.VerticalThickness, platformViewBounds.Height));

			contentView.Bounds = contentBounds;
			contentView.Center = new CGPoint(contentBounds.GetMidX(), contentBounds.GetMidY());

			if (PendingScrollToRequest != null)
			{
				VirtualView.RequestScrollTo(PendingScrollToRequest.HorizontalOffset, PendingScrollToRequest.VerticalOffset, PendingScrollToRequest.Instant);
				PendingScrollToRequest = null;
			}
		}

		static double AccountForPadding(double constraint, double padding)
		{
			// Remove the padding from the constraint, but don't allow it to go negative
			return Math.Max(0, constraint - padding);
		}

		static void SetContentSizeForOrientation(UIScrollView uiScrollView, double viewportWidth, double viewportHeight, ScrollOrientation orientation, Size contentSize)
		{
			if (orientation is ScrollOrientation.Vertical or ScrollOrientation.Neither)
			{
				contentSize.Width = Math.Min(contentSize.Width, viewportWidth);
			}

			if (orientation is ScrollOrientation.Horizontal or ScrollOrientation.Neither)
			{
				contentSize.Height = Math.Min(contentSize.Height, viewportHeight);
			}

			uiScrollView.ContentSize = contentSize;
		}

		static CGSize GetViewportSize(UIScrollView platformScrollView)
		{
			return platformScrollView.AdjustedContentInset.InsetRect(platformScrollView.Bounds).Size;
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var scrollView = VirtualView;
			var crossPlatformLayout = scrollView as ICrossPlatformLayout;
			var platformScrollView = PlatformView;

			var presentedContent = scrollView.PresentedContent;
			if (presentedContent == null)
			{
				return Size.Zero;
			}

			var viewportSize = GetViewportSize(platformScrollView);

			var padding = scrollView.Padding;

			if (widthConstraint == 0)
			{
				widthConstraint = viewportSize.Width;
			}

			if (heightConstraint == 0)
			{
				heightConstraint = viewportSize.Height;
			}

			// Account for the ScrollView Padding before measuring the content
			widthConstraint = AccountForPadding(widthConstraint, padding.HorizontalThickness);
			heightConstraint = AccountForPadding(heightConstraint, padding.VerticalThickness);

			// Now handle the actual cross-platform measurement of the ScrollView's content
			var result = crossPlatformLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);

			return result.AdjustForFill(new Rect(0, 0, widthConstraint, heightConstraint), presentedContent);
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			var scrollView = VirtualView;
			var crossPlatformLayout = scrollView as ICrossPlatformLayout;
			var platformScrollView = PlatformView;

			// The UIScrollView's bounds are available, so we can use them to make sure the ContentSize makes sense
			// for the ScrollView orientation
			var viewportSize = GetViewportSize(platformScrollView);

			// Get a Rect for doing the CrossPlatformArrange of the Content
			var viewportRect = new Rect(Graphics.Point.Zero, viewportSize.ToSize());

			var contentSize = crossPlatformLayout.CrossPlatformArrange(viewportRect);

			var viewportHeight = viewportSize.Height;
			var viewportWidth = viewportSize.Width;
			SetContentSizeForOrientation(platformScrollView, viewportWidth, viewportHeight, scrollView.Orientation, contentSize);

			var container = GetContentView(platformScrollView);

			if (container?.Superview is UIScrollView uiScrollView)
			{
				// Ensure the container is at least the size of the UIScrollView itself, so that the 
				// cross-platform layout logic makes sense and the contents don't arrange outside the 
				// container. (Everything will look correct if they do, but hit testing won't work properly.)
				var containerBounds = contentSize;

				container.Bounds = new CGRect(0, 0,
					Math.Max(containerBounds.Width, viewportSize.Width),
					Math.Max(containerBounds.Height, viewportSize.Height));

				container.Center = new CGPoint(container.Bounds.GetMidX(), container.Bounds.GetMidY());
			}

			return contentSize;
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

			void OnButtonTouchUpInside(object? sender, EventArgs e)
			{
				if (VirtualView is IButton virtualView)
				{
					virtualView.Released();
					virtualView.Clicked();
				}
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
