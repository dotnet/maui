using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class PlatformTouchGraphicsView : PlatformGraphicsView
	{

		IGraphicsView? graphicsView;
		RectF _bounds;
		bool _dragStarted;
		PointF[] _lastMovedViewPoints = new PointF [0];
		float scale = 1;
		bool pressedContained = false;

		public PlatformTouchGraphicsView(Context context) : base(context)
		{
			scale = (context ?? global::Android.App.Application.Context)?.Resources?.DisplayMetrics?.Density ?? 1;
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
			if (changed)
			{
				var width = right - left;
				var height = bottom - top;
				_bounds = new RectF(0, 0, width / scale, height / scale);
			}
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (e == null)
				throw new ArgumentNullException(nameof(e));

			int touchCount = e.PointerCount;
			var touchPoints = new PointF[touchCount];
			for (int i = 0; i < touchCount; i++)
				touchPoints[i] = new PointF(e.GetX(i) / scale, e.GetY(i) / scale);

			var actionMasked = e.Action & MotionEventActions.Mask;

			switch (actionMasked)
			{
				case MotionEventActions.Move:
					TouchesMoved(touchPoints);
					break;
				case MotionEventActions.Down:
				case MotionEventActions.PointerDown:
					TouchesBegan(touchPoints);
					break;
				case MotionEventActions.Up:
				case MotionEventActions.PointerUp:
					TouchesEnded(touchPoints);
					break;
				case MotionEventActions.Cancel:
					TouchesCanceled();
					break;
			}


			return true;
		}
		public void TouchesBegan(PointF[] points)
		{
			_dragStarted = false;
			_lastMovedViewPoints = points;
			graphicsView?.StartInteraction(points);
			pressedContained = true;
		}

		public void TouchesMoved(PointF[] points)
		{
			if (!_dragStarted)
			{
				if (points.Length == 1)
				{
					float deltaX = _lastMovedViewPoints[0].X - points[0].X;
					float deltaY = _lastMovedViewPoints[0].Y - points[0].Y;

					if (Math.Abs(deltaX) <= 3 && Math.Abs(deltaY) <= 3)
						return;
				}
			}

			_lastMovedViewPoints = points;
			_dragStarted = true;
			pressedContained = _bounds.ContainsAny(points);
			graphicsView?.DragInteraction(points);
		}

		public void TouchesEnded(PointF[] points)
		{
			graphicsView?.EndInteraction(points, pressedContained);
		}

		public void TouchesCanceled()
		{
			pressedContained = false;
			graphicsView?.CancelInteraction();
		}

		public void Connect(IGraphicsView graphicsView) => this.graphicsView = graphicsView;

		public void Disconnect() => graphicsView = null;
	}
}
