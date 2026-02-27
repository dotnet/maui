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
		IGraphicsView? _graphicsView;
		RectF _bounds;
		bool _dragStarted;
		PointF[] _lastMovedViewPoints = Array.Empty<PointF>();
		float _scale = 1;
		bool _pressedContained;

		public PlatformTouchGraphicsView(Context context) : base(context)
		{
			_scale = (context ?? global::Android.App.Application.Context)?.Resources?.DisplayMetrics?.Density ?? 1;
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
			if (changed)
			{
				var width = right - left;
				var height = bottom - top;
				_bounds = new RectF(0, 0, width / _scale, height / _scale);
			}
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}

			// If the GraphicsView is disabled, we don't want to handle touch events.
			// This is to prevent any interaction when the view is not interactive.
			if (_graphicsView is null || !_graphicsView.IsEnabled)
			{
				return false;
			}

			int touchCount = e.PointerCount;
			var touchPoints = new PointF[touchCount];
			for (int i = 0; i < touchCount; i++)
			{
				touchPoints[i] = new PointF(e.GetX(i) / _scale, e.GetY(i) / _scale);
			}

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
			_graphicsView?.StartInteraction(points);
			_pressedContained = true;
		}

		public void TouchesMoved(PointF[] points)
		{
			if (!_dragStarted)
			{
				if (points.Length == 1)
				{
					float deltaX = _lastMovedViewPoints[0].X - points[0].X;
					float deltaY = _lastMovedViewPoints[0].Y - points[0].Y;

					if (MathF.Abs(deltaX) <= 3 && MathF.Abs(deltaY) <= 3)
						return;
				}
			}

			_lastMovedViewPoints = points;
			_dragStarted = true;
			_pressedContained = _bounds.ContainsAny(points);
			_graphicsView?.DragInteraction(points);
		}

		public void TouchesEnded(PointF[] points)
		{
			_graphicsView?.EndInteraction(points, _pressedContained);
		}

		public void TouchesCanceled()
		{
			_pressedContained = false;
			_graphicsView?.CancelInteraction();
		}

		public override bool OnHoverEvent(MotionEvent? e)
		{
			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}

			if (_graphicsView is null || !_graphicsView.IsEnabled)
			{
				return false;
			}

			int touchCount = e.PointerCount;
			var touchPoints = new PointF[touchCount];
			for (int i = 0; i < touchCount; i++)
			{
				touchPoints[i] = new PointF(e.GetX(i) / _scale, e.GetY(i) / _scale);
			}

			var actionMasked = e.Action & MotionEventActions.Mask;

			switch (actionMasked)
			{
				case MotionEventActions.HoverMove:
					_graphicsView?.MoveHoverInteraction(touchPoints);
					break;
				case MotionEventActions.HoverEnter:
					_graphicsView?.StartHoverInteraction(touchPoints);
					break;
				case MotionEventActions.HoverExit:
					_graphicsView?.EndHoverInteraction();
					break;
			}

			return true;
		}

		public void Connect(IGraphicsView graphicsView) => _graphicsView = graphicsView;

		public void Disconnect() => _graphicsView = null;
	}
}
