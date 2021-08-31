using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, UIScrollView>
	{
		protected override UIScrollView CreateNativeView()
		{
			return new UIScrollView();
		}

		protected override void ConnectHandler(UIScrollView nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.Scrolled += Scrolled;
			nativeView.ScrollAnimationEnded += ScrollAnimationEnded;
		}

		protected override void DisconnectHandler(UIScrollView nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.Scrolled -= Scrolled;
			nativeView.ScrollAnimationEnded -= ScrollAnimationEnded;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var explicitWidth = VirtualView.Width;
			var explicitHeight = VirtualView.Height;
			var hasExplicitWidth = explicitWidth >= 0;
			var hasExplicitHeight = explicitHeight >= 0;

			var sizeThatFits = nativeView.SizeThatFits(new CGSize((float)widthConstraint, (float)heightConstraint));

			var size = new Size(
				sizeThatFits.Width > 0 ? sizeThatFits.Width : NativeView.ContentSize.Width,
				sizeThatFits.Height > 0 ? sizeThatFits.Height : NativeView.ContentSize.Height);

			return new Size(hasExplicitWidth ? explicitWidth : size.Width,
				hasExplicitHeight ? explicitHeight : size.Height);
		}

		void ScrollAnimationEnded(object? sender, EventArgs e)
		{
			VirtualView.ScrollFinished();
		}

		void Scrolled(object? sender, EventArgs e)
		{
			VirtualView.HorizontalOffset = NativeView.ContentOffset.X;
			VirtualView.VerticalOffset = NativeView.ContentOffset.Y;
		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.NativeView == null || handler.MauiContext == null || scrollView.PresentedContent == null)
				return;

			handler.NativeView.UpdateContent(scrollView.PresentedContent.ToNative(handler.MauiContext));
		}

		public static void MapContentSize(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView.UpdateContentSize(scrollView.ContentSize);
		}

		public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView?.UpdateHorizontalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView?.UpdateVerticalScrollBarVisibility(scrollView.VerticalScrollBarVisibility);
		}

		public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView)
		{
			// Nothing to do here for now, but we might need to make adjustments for FlowDirection when the orientation is set to Horizontal
		}

		public static void MapRequestScrollTo(ScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is ScrollToRequest request)
			{
				handler.NativeView.SetContentOffset(new CoreGraphics.CGPoint(request.HoriztonalOffset, request.VerticalOffset), !request.Instant);

				if (request.Instant)
				{
					scrollView.ScrollFinished();
				}
			}
		}
	}
}
