using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Platform;
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
			return new MauiScrollView();
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
			if (GetContentView(handler.PlatformView) is ContentView currentContentContainer)
			{
				currentContentContainer.SetNeedsLayout();
			}
		}

		public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is ScrollToRequest request)
			{
				var uiScrollView = handler.PlatformView;
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

			contentContainer.CrossPlatformArrange = ArrangeScrollViewContent(scrollView.CrossPlatformArrange, contentContainer, platformScrollView, scrollView);

			platformScrollView.ClearSubviews();
			contentContainer.AddSubview(platformContent);
			platformScrollView.AddSubview(contentContainer);
		}

		static Func<Rect, Size> ArrangeScrollViewContent(Func<Rect, Size> internalArrange, ContentView container, UIScrollView platformScrollView, IScrollView scrollView)
		{
			return (rect) =>
			{
				if (container.Superview is UIScrollView uiScrollView)
				{
					// Ensure the container is at least the size of the UIScrollView itself, so that the 
					// cross-platform layout logic makes sense and the contents don't arrange outside the 
					// container. (Everything will look correct if they do, but hit testing won't work properly.)

					var scrollViewBounds = uiScrollView.Bounds;
					var containerBounds = container.Bounds;

					container.Bounds = new CGRect(0, 0,
						Math.Max(containerBounds.Width, scrollViewBounds.Width),
						Math.Max(containerBounds.Height, scrollViewBounds.Height));
					container.Center = new CGPoint(container.Bounds.GetMidX(), container.Bounds.GetMidY());
				}
				var contentSize = internalArrange(rect);
				var size = SetContentSizeForOrientation(platformScrollView, scrollView.Orientation, contentSize);
				return size;
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

			var scrollViewBounds = platformScrollView.Bounds;
			var padding = scrollView.Padding;

			if (widthConstraint == 0)
			{
				widthConstraint = scrollViewBounds.Width;
			}

			if (heightConstraint == 0)
			{
				heightConstraint = scrollViewBounds.Height;
			}

			// Account for the ScrollView Padding before measuring the content
			widthConstraint = AccountForPadding(widthConstraint, padding.HorizontalThickness);
			heightConstraint = AccountForPadding(heightConstraint, padding.VerticalThickness);

			var result = internalMeasure.Invoke(widthConstraint, heightConstraint);

			return result.AdjustForFill(new Rect(0, 0, widthConstraint, heightConstraint), presentedContent);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var virtualView = VirtualView;
			var platformView = PlatformView;

			if (platformView == null || virtualView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var padding = virtualView.Padding;

			// Account for the ScrollView Padding before measuring the content
			widthConstraint = AccountForPadding(widthConstraint, padding.HorizontalThickness);
			heightConstraint = AccountForPadding(heightConstraint, padding.VerticalThickness);

			var crossPlatformSize = virtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);

			// Add the padding back in for the final size
			crossPlatformSize.Width += padding.HorizontalThickness;
			crossPlatformSize.Height += padding.VerticalThickness;

			var size = SetContentSizeForOrientation(platformView, virtualView.Orientation, crossPlatformSize);

			var finalWidth = ViewHandlerExtensions.ResolveConstraints(size.Width, virtualView.Width, virtualView.MinimumWidth, virtualView.MaximumWidth);
			var finalHeight = ViewHandlerExtensions.ResolveConstraints(size.Height, virtualView.Height, virtualView.MinimumHeight, virtualView.MaximumHeight);

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
		}

		static double AccountForPadding(double constraint, double padding)
		{
			// Remove the padding from the constraint, but don't allow it to go negative
			return Math.Max(0, constraint - padding);
		}

		static Size SetContentSizeForOrientation(UIScrollView platformScrollView, ScrollOrientation orientation, Size crossPlatformSize)
		{
			CGRect scrollViewBounds = platformScrollView.Bounds;
			var contentSize = AccountForOrientation(crossPlatformSize, scrollViewBounds.Width, scrollViewBounds.Height, orientation);
			platformScrollView.ContentSize = contentSize;
			return contentSize;
		}

		internal static Size AccountForOrientation(Size size, double widthConstraint, double heightConstraint, ScrollOrientation orientation)
		{
			if (orientation is ScrollOrientation.Vertical or ScrollOrientation.Neither)
			{
				size.Width = widthConstraint;
			}

			if (orientation is ScrollOrientation.Horizontal or ScrollOrientation.Neither)
			{
				size.Height = heightConstraint;
			}

			return size;
		}
	}
}
