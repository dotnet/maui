using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, W2DGraphicsView>
	{
		protected override W2DGraphicsView CreatePlatformView()
		{
			return new W2DGraphicsView();
		}

		protected override void ConnectHandler(W2DGraphicsView platformView)
		{
			base.ConnectHandler(nativeView);

			platformView.PointerPressed += OnPointerPressed;
			platformView.PointerMoved += OnPointerMoved;
			platformView.PointerReleased += OnPointerReleased;
			platformView.PointerCanceled += OnPointerCanceled;
		}

		protected override void DisconnectHandler(W2DGraphicsView platformView)
		{
			base.DisconnectHandler(nativeView);

			platformView.PointerPressed -= OnPointerPressed;
			platformView.PointerMoved -= OnPointerMoved;
			platformView.PointerReleased -= OnPointerReleased;
			platformView.PointerCanceled -= OnPointerCanceled;
		}

		public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateFlowDirection(graphicsView);
			handler.PlatformView?.Invalidate();
		}

		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.PlatformView?.Invalidate();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var currentPoint = e.GetCurrentPoint(PlatformView);
			var currentPosition = currentPoint.Position;
			var point = new Point(currentPosition.X, currentPosition.Y);

			VirtualView?.OnTouch(new TouchEventArgs(TouchAction.Pressed, point));
		}

		void OnPointerMoved(object sender, PointerRoutedEventArgs e)
		{
			var currentPoint = e.GetCurrentPoint(PlatformView);
			var currentPosition = currentPoint.Position;
			var point = new Point(currentPosition.X, currentPosition.Y);

			VirtualView?.OnTouch(new TouchEventArgs(TouchAction.Moved, point));
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			var currentPoint = e.GetCurrentPoint(PlatformView);
			var currentPosition = currentPoint.Position;
			var point = new Point(currentPosition.X, currentPosition.Y);

			VirtualView?.OnTouch(new TouchEventArgs(TouchAction.Released, point));
		}

		void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
		{
			var currentPoint = e.GetCurrentPoint(PlatformView);
			var currentPosition = currentPoint.Position;
			var point = new Point(currentPosition.X, currentPosition.Y);

			VirtualView?.OnTouch(new TouchEventArgs(TouchAction.Cancelled, point));
		}
	}
}