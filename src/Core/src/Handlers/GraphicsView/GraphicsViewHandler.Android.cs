using System.Collections.Generic;
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

		bool pointsContained = false;

		void OnTouch(object? sender, View.TouchEventArgs e)
		{
			if (e.Event == null)
				return;

			// Get all the points
			float density = Context?.Resources?.DisplayMetrics?.Density ?? 1.0f;
			var points = new List<PointF>();
			var pointCount = e.Event.PointerCount;
			if (pointCount > 0)
			{
				var coords = new MotionEvent.PointerCoords();

				for (int i = 0; i < pointCount; i++)
				{
					
					var pointCoords = e.Event.GetPointerCoords(i, coords);
					var aPoint = new APointF(pointCoords.X / density, pointCoords.Y / density);
					var point = new Point(aPoint.X, aPoint.Y);

					points.Add(point);
				}
			}

			var pointsArr = points.ToArray();

			switch (e.Event.Action)
			{
				case MotionEventActions.Down:
					VirtualView?.StartInteraction(pointsArr);
					pointsContained = true;
					break;
				case MotionEventActions.Move:
					VirtualView?.StartInteraction(pointsArr);
					pointsContained = true;
					// TODO: See if points contained;
					break;
				case MotionEventActions.Up:
					VirtualView?.EndInteraction(pointsArr, pointsContained);
					break;
				case MotionEventActions.Cancel:
					VirtualView?.CancelInteraction();
					break;
				default:
					break;
			}
		}
	}
}