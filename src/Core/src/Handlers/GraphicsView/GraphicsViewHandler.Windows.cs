using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
	{
		protected override PlatformTouchGraphicsView CreatePlatformView()
		{
			return new PlatformTouchGraphicsView();
		}

		// A container is needed when the GraphicsView has a background to ensure proper rendering,Without it the background may not be drawn correctly.
		public override bool NeedsContainer => VirtualView?.Background is not null || base.NeedsContainer;

		private protected override void OnConnectHandler(FrameworkElement platformView)
		{
			base.OnConnectHandler(platformView);

			platformView.Loaded += OnLoaded;
		}

		private protected override void OnDisconnectHandler(FrameworkElement platformView)
		{
			base.OnDisconnectHandler(platformView);

			platformView.Loaded -= OnLoaded;
		}

		public static void MapBackground(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			if (graphicsView.Background is not null)
			{
				handler.UpdateValue(nameof(IViewHandler.ContainerView));
				handler.ToPlatform().UpdateBackground(graphicsView);
				handler.PlatformView?.Invalidate();
			}
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

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			VirtualView?.InvalidateMeasure();
		}
	}
}