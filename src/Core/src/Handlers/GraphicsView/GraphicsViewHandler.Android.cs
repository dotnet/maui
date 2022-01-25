using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;
using APointF = Android.Graphics.PointF;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, NativeGraphicsView>
	{
		protected override NativeGraphicsView CreateNativeView()
		{
			return new NativeGraphicsView(Context);
		}

		protected override void ConnectHandler(NativeGraphicsView nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.Touch += OnTouch;
		}

		protected override void DisconnectHandler(NativeGraphicsView nativeView)
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
			handler.NativeView?.Invalidate();
		}

		public static void MapInvalidate(GraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.NativeView?.Invalidate();
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