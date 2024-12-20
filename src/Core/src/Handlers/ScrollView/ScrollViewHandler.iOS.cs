using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, UIScrollView>, ICrossPlatformLayout
	{
		const nint ContentTag = 0x845fed;

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

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (PlatformView is MauiScrollView platformScrollView && view is IScrollView scrollView)
			{
				platformScrollView.View = scrollView;
			}
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

		static UIView? GetContentView(UIScrollView scrollView)
		{
			for (int i = 0; i < scrollView.Subviews.Length; i++)
			{
				if (scrollView.Subviews[i] is { Tag: ContentTag } contentView)
				{
					return contentView;
				}
			}

			return null;
		}

		static void UpdateContentView(IScrollView scrollView, IScrollViewHandler handler)
		{
			var platformView = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var mauiContext = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (GetContentView(platformView) is { } currentContentPlatformView)
			{
				currentContentPlatformView.RemoveFromSuperview();
			}

			if (scrollView.PresentedContent is { } content)
			{
				var platformContent = content.ToPlatform(mauiContext);
				platformContent.Tag = ContentTag;
				platformView.AddSubview(platformContent);
			}
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			//TODO: We might need to move the MauiScrollView measuring code here just to be overly careful
			// If someone is inheriting from this handler and implementing ICrossPlatformLayout themselves
			// then we need to keep this path active
			return (VirtualView as ICrossPlatformLayout)?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			//TODO: We might need to move the MauiScrollView measuring code here just to be overly careful
			// If someone is inheriting from this handler and implementing ICrossPlatformLayout themselves
			// then we need to keep this path active
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
