using System;
using System.Collections.Generic;
using System.Text;
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

		void ScrollAnimationEnded(object sender, EventArgs e)
		{
			VirtualView.ScrollFinished();
		}

		void Scrolled(object sender, EventArgs e)
		{
			VirtualView.HorizontalOffset = NativeView.ContentOffset.X;
			VirtualView.VerticalOffset = NativeView.ContentOffset.Y;
		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null)
			{
				return;
			}

			handler.NativeView.UpdateContent(scrollView.Content.ToNative(handler.MauiContext));
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
