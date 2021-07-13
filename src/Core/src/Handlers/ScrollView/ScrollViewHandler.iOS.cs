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
		}

		protected override void DisconnectHandler(UIScrollView nativeView)
		{
			base.DisconnectHandler(nativeView);
		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null)
			{
				return;
			}

			// TODO ezhart implement
		}

		public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			//handler.NativeView?.UpdateScrollBarVisibility(scrollView.Orientation, scrollView.HorizontalScrollBarVisibility);
			// TODO ezhart implement
		}

		public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			//handler.NativeView.VerticalScrollBarVisibility = scrollView.VerticalScrollBarVisibility.ToWindowsScrollBarVisibility();
			// TODO ezhart implement
		}

		public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView)
		{
			//handler.NativeView?.UpdateScrollBarVisibility(scrollView.Orientation, scrollView.HorizontalScrollBarVisibility);
			// TODO ezhart implement
		}

		public static void MapRequestScrollTo(ScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is ScrollToRequest request)
			{
				handler.NativeView.SetContentOffset(new CoreGraphics.CGPoint(request.HoriztonalOffset, request.VerticalOffset), !request.Instant);
			}
		}
	}
}
