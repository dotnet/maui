using System;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollView>
	{

		INativeViewHandler? _contentHandler;

		protected override ScrollView CreatePlatformView() => new ScrollView();

		protected override void ConnectHandler(ScrollView platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Scrolling += OnScrolled;
			platformView.ScrollAnimationEnded += ScrollAnimationEnded;

		}

		protected override void DisconnectHandler(ScrollView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.Scrolling -= OnScrolled;
			platformView.ScrollAnimationEnded -= ScrollAnimationEnded;
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
			var region = PlatformView.ScrollBound.ToDP();
			VirtualView.HorizontalOffset = region.X;
			VirtualView.VerticalOffset = region.Y;
		}

		void UpdateContentSize()
		{
			var width = Math.Max(VirtualView.ContentSize.Width.ToScaledPixel(), 100);
			var height = Math.Max(VirtualView.ContentSize.Height.ToScaledPixel(), 100);

			PlatformView.ContentContainer.SizeWidth = width;
			PlatformView.ContentContainer.SizeHeight = height;
		}

		void UpdateContent(INativeViewHandler? content)
		{
			if (_contentHandler != null)
			{
				PlatformView.ContentContainer.Remove(_contentHandler.PlatformView);
				_contentHandler.Dispose();
				_contentHandler = null;
			}
			_contentHandler = content;

			if (_contentHandler != null)
			{
				PlatformView.ContentContainer.Add(_contentHandler.PlatformView);
				UpdateContentSize();
			} 
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null || scrollView.PresentedContent == null)
			{
				return;
			}
			scrollView.PresentedContent.ToPlatform(handler.MauiContext);
			if (scrollView.PresentedContent.Handler is IPlatformViewHandler thandler)
			{
				handler.UpdateContent(thandler);
			}
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

				var pos = scrollView.Orientation == ScrollOrientation.Vertical ? y : x;

				handler.PlatformView.ScrollTo(pos.ToPixel(), !request.Instant);

				if (request.Instant)
				{
					scrollView.ScrollFinished();
				}
			}
		}
	}
}
