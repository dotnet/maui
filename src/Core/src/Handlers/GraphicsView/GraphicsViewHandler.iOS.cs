namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, CustomNativeGraphicsView>
	{
		protected override CustomNativeGraphicsView CreateNativeView()
		{
			return new CustomNativeGraphicsView { UserInteractionEnabled = true };
		}

		protected override void ConnectHandler(CustomNativeGraphicsView nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.Touch += OnTouch;
		}

		protected override void DisconnectHandler(CustomNativeGraphicsView nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.Touch -= OnTouch;
		}

		public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.NativeView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.NativeView?.UpdateFlowDirection(graphicsView);
			handler.NativeView?.InvalidateDrawable();
		}

		public static void MapInvalidate(GraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.NativeView?.InvalidateDrawable();
		}

		void OnTouch(object? sender, TouchEventArgs e)
		{
			VirtualView?.OnTouch(e);
		}
	}
}