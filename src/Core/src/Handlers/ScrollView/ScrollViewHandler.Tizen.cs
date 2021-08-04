using System;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using EContainer = ElmSharp.Container;
using EcoreMainloop = ElmSharp.EcoreMainloop;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollView>
	{
		EContainer? _scrollCanvas;

		Box? Canvas => (Box?)_scrollCanvas;

		protected override ScrollView CreateNativeView()
		{
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null");
			var scrollView = new ScrollView(NativeParent);
			_scrollCanvas = new Box(scrollView);
			scrollView.SetContent(_scrollCanvas);
			return scrollView;
		}

		protected override void ConnectHandler(ScrollView nativeView)
		{
			base.ConnectHandler(nativeView);
			_ = Canvas ?? throw new InvalidOperationException($"{nameof(Canvas)} cannot be null");

			nativeView.Scrolled += OnScrolled;
			Canvas.LayoutUpdated += OnContentLayoutUpdated;
		}

		protected override void DisconnectHandler(ScrollView nativeView)
		{
			base.DisconnectHandler(nativeView);
			_ = Canvas ?? throw new InvalidOperationException($"{nameof(Canvas)} cannot be null");

			nativeView.Scrolled -= OnScrolled;
			Canvas.LayoutUpdated -= OnContentLayoutUpdated;
		}

		void ScrollAnimationEnded(object? sender, EventArgs e)
		{
			VirtualView.ScrollFinished();
		}

		void OnScrolled(object? sender, EventArgs e)
		{
			var region = NativeView.CurrentRegion.ToDP();
			VirtualView.HorizontalOffset = region.X;
			VirtualView.VerticalOffset = region.Y;
		}

		void OnContentLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			UpdateContentSize();
		}

		void UpdateContentSize()
		{
			_ = Canvas ?? throw new InvalidOperationException($"{nameof(Canvas)} cannot be null");

			// TODO: should consider Padding.HorizontalThickness/VerticalThickness here.
			Canvas.MinimumWidth = VirtualView.ContentSize.Width.ToScaledPixel();
			Canvas.MinimumHeight = VirtualView.ContentSize.Height.ToScaledPixel();

			// elm-scroller updates the CurrentRegion after render
			EcoreMainloop.Post(() =>
			{
				if (NativeView != null)
				{
					OnScrolled(NativeView, EventArgs.Empty);
				}
			});
		}

		public static void MapContent(ScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null || scrollView.Content == null || handler.Canvas == null)
			{
				return;
			}

			handler.Canvas.UnPackAll();
			handler.Canvas.PackEnd(scrollView.Content.ToNative(handler.MauiContext));
			handler.UpdateContentSize();
		}

		public static void MapContentSize(ScrollViewHandler handler, IScrollView scrollView)
		{
			handler.UpdateContentSize();
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
			handler.NativeView?.UpdateOrientation(scrollView.Orientation);
		}

		public static void MapRequestScrollTo(ScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is ScrollToRequest request)
			{
				var x = request.HoriztonalOffset;
				var y = request.VerticalOffset;

				var region = new Rectangle(x, y, scrollView.Width, scrollView.Height).ToEFLPixel();
				handler.NativeView.ScrollTo(region, true);

				if (request.Instant)
				{
					scrollView.ScrollFinished();
				}
			}
		}
	}
}
