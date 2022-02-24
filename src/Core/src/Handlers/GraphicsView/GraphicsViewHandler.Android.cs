using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using APointF = Android.Graphics.PointF;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformGraphicsView>
	{
		protected override PlatformGraphicsView CreatePlatformView()
		{
			return new PlatformGraphicsView(Context);
		}

		protected override void ConnectHandler(PlatformGraphicsView platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Touch += OnTouch;
		}

		protected override void DisconnectHandler(PlatformGraphicsView platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.Touch -= OnTouch;
		}

		public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateFlowDirection(graphicsView);
			handler.PlatformView?.Invalidate();
		}

		public static void MapInvalidate(GraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.PlatformView?.Invalidate();
		}

		void OnTouch(object? sender, View.TouchEventArgs e)
		{
			if (e.Event == null)
				return;

			float density = Context?.Resources?.DisplayMetrics?.Density ?? 1.0f;
			APointF aPoint = new APointF(e.Event.GetX() / density, e.Event.GetY() / density);
			Point point = new Point(aPoint.X, aPoint.Y);

			switch (e.Event.Action)
			{
				case MotionEventActions.Down:
					VirtualView?.OnTouch(new TouchEventArgs(TouchAction.Pressed, point));
					break;
				case MotionEventActions.Move:
					VirtualView?.OnTouch(new TouchEventArgs(TouchAction.Moved, point));
					break;
				case MotionEventActions.Up:
					VirtualView?.OnTouch(new TouchEventArgs(TouchAction.Released, point));
					break;
				case MotionEventActions.Cancel:
					VirtualView?.OnTouch(new TouchEventArgs(TouchAction.Cancelled, point));
					break;
				default:
					break;
			}
		}
	}
}