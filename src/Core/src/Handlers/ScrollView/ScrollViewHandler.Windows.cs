#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollViewer>
	{
		protected override ScrollViewer CreateNativeView()
		{
			return new ScrollViewer();
		}

		protected override void ConnectHandler(ScrollViewer nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.ViewChanged += ViewChanged;
		}

		public override void NativeArrange(Rectangle rect)
		{
			// Re-measure the content too, otherwise winui will try and keep the scrollviewer' size >= the content size
			NativeView?.Measure(new Windows.Foundation.Size(rect.Size.Width, rect.Size.Height));

			base.NativeArrange(rect);
		}

		protected override void DisconnectHandler(ScrollViewer nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.ViewChanged -= ViewChanged;
		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null)
			{
				return;
			}

			handler.NativeView.Content = scrollView.Content.ToNative(handler.MauiContext);
		}

		public static void MapHorizontalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView?.UpdateScrollBarVisibility(scrollView.Orientation, scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView.VerticalScrollBarVisibility = scrollView.VerticalScrollBarVisibility.ToWindowsScrollBarVisibility();
		}

		public static void MapOrientation(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.NativeView?.UpdateScrollBarVisibility(scrollView.Orientation, scrollView.HorizontalScrollBarVisibility);
		}

		void ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
		{
			VirtualView.VerticalOffset = NativeView.VerticalOffset;
			VirtualView.HorizontalOffset = NativeView.HorizontalOffset;

			if (e.IsIntermediate == false)
			{
				VirtualView.ScrollFinished();
			}
		}
	}
}