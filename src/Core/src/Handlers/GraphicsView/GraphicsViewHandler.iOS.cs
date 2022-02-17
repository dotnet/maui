using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, CustomPlatformGraphicsView>
	{
		protected override CustomPlatformGraphicsView CreatePlatformView()
		{
			return new CustomPlatformGraphicsView();
		}

		protected override void ConnectHandler(CustomPlatformGraphicsView nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.Touch += OnTouch;
		}

		protected override void DisconnectHandler(CustomPlatformGraphicsView nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.Touch -= OnTouch;
		}

		public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateFlowDirection(graphicsView);
			handler.PlatformView?.InvalidateDrawable();
		}

		public static void MapInvalidate(GraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.PlatformView?.InvalidateDrawable();
		}

		void OnTouch(object? sender, TouchEventArgs e)
		{
			VirtualView?.OnTouch(e);
		}
	}
}