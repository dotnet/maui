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
			if (handler.MauiContext == null || scrollView.Content == null)
			{
				return;
			}

			handler.NativeView.UpdateContent(scrollView.Content.ToNative(handler.MauiContext));
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
