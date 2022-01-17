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

		public override ElmSharp.Rect GetNativeContentGeometry()
		{
			return Canvas?.Geometry ?? NativeView.Geometry;
		}

		protected override void DisconnectHandler(ScrollView nativeView)
		{
			base.DisconnectHandler(nativeView);
			_ = Canvas ?? throw new InvalidOperationException($"{nameof(Canvas)} cannot be null");

			nativeView.Scrolled -= OnScrolled;
			Canvas.LayoutUpdated -= OnContentLayoutUpdated;
		}

		public override Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return VirtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);
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
			// It is workaround,
			// in some case, before set a size of ScrollView, if content was filled with sized items,
			// after size of ScrollView was updated, a content position was moved to somewhere.
			if (VirtualView != null && VirtualView.PresentedContent != null)
			{
				var frame = VirtualView.PresentedContent.Frame;
				VirtualView.PresentedContent.ToNative(MauiContext!)?.Move((int)e.Geometry.X + frame.X.ToScaledPixel(), (int)e.Geometry.Y + frame.Y.ToScaledPixel());
			}

			UpdateContentSize();
		}

		void UpdateContentSize()
		{
			_ = Canvas ?? throw new InvalidOperationException($"{nameof(Canvas)} cannot be null");

			if (VirtualView == null || VirtualView.PresentedContent == null)
			    return;
			    
			Canvas.MinimumWidth = (VirtualView.PresentedContent.Margin.HorizontalThickness + VirtualView.PresentedContent.Frame.Width + VirtualView.Padding.HorizontalThickness).ToScaledPixel();
			Canvas.MinimumHeight = (VirtualView.PresentedContent.Margin.VerticalThickness + VirtualView.PresentedContent.Frame.Height + VirtualView.Padding.VerticalThickness).ToScaledPixel();

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
			if (handler.MauiContext == null || scrollView.PresentedContent == null || handler.Canvas == null)
			{
				return;
			}

			handler.Canvas.UnPackAll();
			handler.Canvas.PackEnd(scrollView.PresentedContent.ToNative(handler.MauiContext));
			if (scrollView.PresentedContent.Handler is INativeViewHandler thandler)
			{
				thandler?.SetParent(handler);
			}
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

				var region = new ElmSharp.Rect
				{
					X = x.ToScaledPixel(),
					Y = y.ToScaledPixel(),
					Width = handler.NativeView!.Geometry.Width,
					Height = handler.NativeView!.Geometry.Height
				};
				handler.NativeView.ScrollTo(region, !request.Instant);

				if (request.Instant)
				{
					scrollView.ScrollFinished();
				}
			}
		}
	}
}
