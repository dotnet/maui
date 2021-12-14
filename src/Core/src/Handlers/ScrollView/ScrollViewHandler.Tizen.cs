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

		LayoutCanvas? Canvas => (LayoutCanvas?)_scrollCanvas;

		protected override ScrollView CreatePlatformView()
		{
			var scrollView = new ScrollView(PlatformParent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1
			};
			_scrollCanvas = new LayoutCanvas(scrollView, VirtualView)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange,
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure
			};
			scrollView.SetContent(_scrollCanvas);
			return scrollView;
		}

		protected override void ConnectHandler(ScrollView platformView)
		{
			base.ConnectHandler(platformView);
			_ = Canvas ?? throw new InvalidOperationException($"{nameof(Canvas)} cannot be null");

			platformView.Scrolled += OnScrolled;
			Canvas.LayoutUpdated += OnContentLayoutUpdated;
		}

		public override ElmSharp.Rect GetPlatformContentGeometry()
		{
			return Canvas?.Geometry ?? PlatformView.Geometry;
		}

		protected override void DisconnectHandler(ScrollView platformView)
		{
			base.DisconnectHandler(platformView);
			_ = Canvas ?? throw new InvalidOperationException($"{nameof(Canvas)} cannot be null");

			platformView.Scrolled -= OnScrolled;
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
			var region = PlatformView.CurrentRegion.ToDP();
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
				VirtualView.PresentedContent.ToPlatform(MauiContext!)?.Move((int)e.Geometry.X + frame.X.ToScaledPixel(), (int)e.Geometry.Y + frame.Y.ToScaledPixel());
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
				if (PlatformView != null)
				{
					OnScrolled(PlatformView, EventArgs.Empty);
				}
			});
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null || scrollView.PresentedContent == null || handler is not ScrollViewHandler sHandler || sHandler.Canvas == null)
			{
				return;
			}

			sHandler.Canvas.UnPackAll();
			sHandler.Canvas.PackEnd(scrollView.PresentedContent.ToPlatform(handler.MauiContext));
			if (scrollView.PresentedContent.Handler is IPlatformViewHandler thandler)
			{
				thandler?.SetParent(sHandler);
			}
			sHandler.UpdateContentSize();
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
			handler.PlatformView?.UpdateOrientation(scrollView.Orientation);
		}

		public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is ScrollToRequest request)
			{
				var x = request.HoriztonalOffset;
				var y = request.VerticalOffset;

				var region = new ElmSharp.Rect
				{
					X = x.ToScaledPixel(),
					Y = y.ToScaledPixel(),
					Width = handler.PlatformView!.Geometry.Width,
					Height = handler.PlatformView!.Geometry.Height
				};
				handler.PlatformView.ScrollTo(region, !request.Instant);

				if (request.Instant)
				{
					scrollView.ScrollFinished();
				}
			}
		}
	}
}
