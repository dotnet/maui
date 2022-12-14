using System;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollView>
	{
		IPlatformViewHandler? _contentHandler;
		double _cachedWidth;
		double _cachedHeight;
		Graphics.Size _measureCache;

		protected override ScrollView CreatePlatformView() => new MauiScrollView(VirtualView);

		protected override void ConnectHandler(ScrollView platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Scrolling += OnScrolled;
			platformView.ScrollAnimationEnded += ScrollAnimationEnded;
			platformView.Relayout += OnRelayout;
		}

		protected override void DisconnectHandler(ScrollView platformView)
		{
			if (!platformView.HasBody())
				return;

			base.DisconnectHandler(platformView);
			platformView.Scrolling -= OnScrolled;
			platformView.ScrollAnimationEnded -= ScrollAnimationEnded;
			platformView.Relayout -= OnRelayout;
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

		void OnRelayout(object? sender, EventArgs e)
		{
			OnContentLayoutUpdated();
		}

		void UpdateContentSize()
		{
			if (VirtualView != null && VirtualView.PresentedContent != null)
			{
				var width = Math.Max((VirtualView.PresentedContent.Margin.HorizontalThickness + VirtualView.PresentedContent.Frame.Width + VirtualView.Padding.HorizontalThickness).ToScaledPixel(), 100);
				var height = Math.Max((VirtualView.PresentedContent.Margin.VerticalThickness + VirtualView.PresentedContent.Frame.Height + VirtualView.Padding.VerticalThickness).ToScaledPixel(), 100);

				if (_cachedWidth != width)
				{
					PlatformView.ContentContainer.SizeWidth = width;
					_cachedWidth = width;
				}

				if (_cachedHeight != height)
				{
					PlatformView.ContentContainer.SizeHeight = height;
					_cachedHeight = height;
				}
			}
		}

		void UpdateContent(IPlatformViewHandler? content)
		{
			if (_contentHandler != null)
			{
				if (_contentHandler.PlatformView is LayoutViewGroup viewgroup)
				{
					viewgroup.LayoutUpdated -= OnContentLayoutUpdated;
				}

				PlatformView.ContentContainer.Remove(_contentHandler.PlatformView);
				_contentHandler.Dispose();
				_contentHandler = null;
			}
			_contentHandler = content;

			if (_contentHandler != null)
			{
				PlatformView.ContentContainer.Add(_contentHandler.PlatformView);

				if (_contentHandler.PlatformView is LayoutViewGroup viewgroup)
				{
					viewgroup.LayoutUpdated += OnContentLayoutUpdated;
				}
			}
			UpdateContentSize();
		}

		void OnContentLayoutUpdated(object? sender, Tizen.UIExtensions.Common.LayoutEventArgs e)
		{
			OnContentLayoutUpdated();
		}

		void OnContentLayoutUpdated()
		{
			var viewGroup = _contentHandler?.PlatformView as LayoutViewGroup;
			if (viewGroup != null)
			{
				viewGroup.IsLayoutUpdating++;
			}

			var platformGeometry = PlatformView.GetBounds().ToDP();
			var measuredSize = VirtualView.CrossPlatformMeasure(platformGeometry.Width, platformGeometry.Height);

			if (_measureCache != measuredSize)
			{
				platformGeometry.X = 0;
				platformGeometry.Y = 0;
				VirtualView.CrossPlatformArrange(platformGeometry);
				UpdateContentSize();
			}
			_measureCache = measuredSize;

			if (viewGroup != null)
			{
				viewGroup.IsLayoutUpdating--;
			}
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.MauiContext == null || scrollView.PresentedContent == null || handler is not ScrollViewHandler sHandler)
			{
				return;
			}
			scrollView.PresentedContent.ToPlatform(handler.MauiContext);
			if (scrollView.PresentedContent.Handler is IPlatformViewHandler thandler)
			{
				sHandler.UpdateContent(thandler);
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
				var x = request.HorizontalOffset;
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
