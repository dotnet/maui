using System.Linq;

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

			nativeView.OnTouchesBegan += NativeView_OnTouchesBegan;
			nativeView.OnTouchesMoved += NativeView_OnTouchesMoved;
			nativeView.OnTouchesEnded += NativeView_OnTouchesEnded;
			nativeView.OnTouchesCancelled += NativeView_OnTouchesCancelled;
		}

		bool pressContained = false;

		void NativeView_OnTouchesCancelled(object? sender, Graphics.PointF[] e)
		{
			pressContained = false;
			VirtualView?.CancelInteraction();
		}

		void NativeView_OnTouchesEnded(object? sender, Graphics.PointF[] e)
		{
			VirtualView?.EndInteraction(e, pressContained);
		}

		void NativeView_OnTouchesMoved(object? sender, Graphics.PointF[] e)
		{
			VirtualView?.DragInteraction(e);

			// Track if a point is inside the view frame
			// So that if the end is fired we know if it was clicked 
			pressContained = e.Any(p => VirtualView?.Frame?.Contains(p) ?? false);
		}

		void NativeView_OnTouchesBegan(object? sender, Graphics.PointF[] e)
		{
			VirtualView?.StartInteraction(e);
			pressContained = true;
		}

		protected override void DisconnectHandler(CustomNativeGraphicsView nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.OnTouchesBegan -= NativeView_OnTouchesBegan;
			nativeView.OnTouchesMoved -= NativeView_OnTouchesMoved;
			nativeView.OnTouchesEnded -= NativeView_OnTouchesEnded;
			nativeView.OnTouchesCancelled -= NativeView_OnTouchesCancelled;
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

	}
}