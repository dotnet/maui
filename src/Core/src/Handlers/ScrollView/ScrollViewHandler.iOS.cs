using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using ObjCRuntime;
using UIKit;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, UIScrollView>
	{
		const nint ContentPanelTag = 0x845fed;

		protected override UIScrollView CreatePlatformView()
		{
			return new UIScrollView();
		}

		protected override void ConnectHandler(UIScrollView platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Scrolled += Scrolled;
			platformView.ScrollAnimationEnded += ScrollAnimationEnded;
		}

		protected override void DisconnectHandler(UIScrollView platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.Scrolled -= Scrolled;
			platformView.ScrollAnimationEnded -= ScrollAnimationEnded;
		}

		void ScrollAnimationEnded(object? sender, EventArgs e)
		{
			VirtualView.ScrollFinished();
		}

		void Scrolled(object? sender, EventArgs e)
		{
			VirtualView.HorizontalOffset = PlatformView.ContentOffset.X;
			VirtualView.VerticalOffset = PlatformView.ContentOffset.Y;
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.PlatformView == null || handler.MauiContext == null)
				return;

			UpdateContentView(scrollView, handler);
		}

		public static void MapContentSize(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.UpdateContentSize(scrollView.ContentSize);
		}

		public static void MapIsEnabled(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.UpdateIsEnabled(scrollView);
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
			// Nothing to do here for now, but we might need to make adjustments for FlowDirection when the orientation is set to Horizontal
		}

		public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is ScrollToRequest request)
			{
				handler.PlatformView.SetContentOffset(new CoreGraphics.CGPoint(request.HoriztonalOffset, request.VerticalOffset), !request.Instant);

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

		static void UpdateContentView(IScrollView scrollView, IScrollViewHandler handler)
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
				InsertContentView(platformScrollView, scrollView, nativeContent);
			}
		}

		static void InsertContentView(UIScrollView platformScrollView, IScrollView scrollView, UIView platformContent)
		{
			if (scrollView.PresentedContent == null)
			{
				return;
			}

			var contentContainer = new ContentView()
			{
				View = scrollView.PresentedContent,
				CrossPlatformMeasure = ConstrainToScrollView(scrollView.CrossPlatformMeasure, platformScrollView, scrollView),
				Tag = ContentPanelTag
			};

			contentContainer.CrossPlatformArrange = ArrangeScrollViewContent(scrollView.CrossPlatformArrange, contentContainer);

			platformScrollView.ClearSubviews();
			contentContainer.AddSubview(platformContent);
			platformScrollView.AddSubview(contentContainer);
		}

		static Func<Rect, Size> ArrangeScrollViewContent(Func<Rect, Size> internalArrange, ContentView container)
		{
			return (rect) =>
			{
				if (container.Superview is UIScrollView scrollView)
				{
					// Ensure the container is at least the size of the UIScrollView itself, so that the 
					// cross-platform layout logic makes sense and the contents don't arrange out side the 
					// container. (Everything will look correct if they do, but hit testing won't work properly.)

					var scrollViewBounds = scrollView.Bounds;
					var containerBounds = container.Bounds;

					container.Bounds = new CGRect(0, 0,
						Math.Max(containerBounds.Width, scrollViewBounds.Width),
						Math.Max(containerBounds.Height, scrollViewBounds.Height));
					container.Center = new CGPoint(container.Bounds.GetMidX(), container.Bounds.GetMidY());
				}

				return internalArrange(rect);
			};
		}

		static Func<double, double, Size> ConstrainToScrollView(Func<double, double, Size> internalMeasure, UIScrollView platformScrollView, IScrollView scrollView)
		{
			return (widthConstraint, heightConstraint) =>
			{
				return MeasureScrollViewContent(widthConstraint, heightConstraint, internalMeasure, platformScrollView, scrollView);
			};
		}

		static Size MeasureScrollViewContent(double widthConstraint, double heightConstraint, Func<double, double, Size> internalMeasure, UIScrollView platformScrollView, IScrollView scrollView)
		{
			var presentedContent = scrollView.PresentedContent;
			if (presentedContent == null)
			{
				return Size.Zero;
			}

			if (widthConstraint == 0)
			{
				widthConstraint = platformScrollView.Bounds.Width;
			}

			if (heightConstraint == 0)
			{
				heightConstraint = platformScrollView.Bounds.Height;
			}

			widthConstraint -= scrollView.Padding.HorizontalThickness;
			heightConstraint -= scrollView.Padding.VerticalThickness;

			var result = internalMeasure.Invoke(widthConstraint, heightConstraint);

			return result.AdjustForFill(new Rect(0, 0, widthConstraint, heightConstraint), presentedContent);
		}
	}
}
