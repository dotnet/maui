using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, CustomPlatformGraphicsView>
	{
		protected override CustomPlatformGraphicsView CreatePlatformView()
		{
			return new CustomPlatformGraphicsView();
		}

		protected override void ConnectHandler(CustomPlatformGraphicsView platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Touch += OnTouch;
		}

		protected override void DisconnectHandler(CustomPlatformGraphicsView platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.Touch -= OnTouch;
		}

		public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateFlowDirection(graphicsView);
			handler.PlatformView?.InvalidateDrawable();
		}

		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.PlatformView?.InvalidateDrawable();
		}

		void OnTouch(object? sender, TouchEventArgs e)
		{
			VirtualView?.OnTouch(e);
		}
	}
}