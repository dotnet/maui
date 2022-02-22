using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, UIScrollView>
	{
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

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var platformView = this.ToPlatform();

			if (platformView == null || VirtualView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			VirtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);

			var explicitWidth = VirtualView.Width;
			var explicitHeight = VirtualView.Height;
			var hasExplicitWidth = explicitWidth >= 0;
			var hasExplicitHeight = explicitHeight >= 0;

			var sizeThatFits = platformView.SizeThatFits(new CGSize((float)widthConstraint, (float)heightConstraint));

			var size = new Size(
				sizeThatFits.Width > 0 ? sizeThatFits.Width : PlatformView.ContentSize.Width,
				sizeThatFits.Height > 0 ? sizeThatFits.Height : PlatformView.ContentSize.Height);

			return new Size(hasExplicitWidth ? explicitWidth : size.Width,
				hasExplicitHeight ? explicitHeight : size.Height);
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

			handler.PlatformView.UpdateContent(scrollView.PresentedContent, handler.MauiContext);
		}

		public static void MapContentSize(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.UpdateContentSize(scrollView.ContentSize);
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
	}
}
