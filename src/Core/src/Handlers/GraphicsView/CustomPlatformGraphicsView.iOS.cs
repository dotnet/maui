#nullable disable
using System;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public class CustomPlatformGraphicsView : PlatformGraphicsView
	{
		public event EventHandler<TouchEventArgs> Touch;

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			var viewPoints = this.GetPointsInView(evt);
			PointF viewPoint = viewPoints.Length > 0 ? viewPoints[0] : PointF.Zero;
			var point = new Point(viewPoint.X, viewPoint.Y);

			Touch?.Invoke(this, new TouchEventArgs(TouchAction.Pressed, point));
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);

			var viewPoints = this.GetPointsInView(evt);
			PointF viewPoint = viewPoints.Length > 0 ? viewPoints[0] : PointF.Zero;
			var point = new Point(viewPoint.X, viewPoint.Y);

			Touch?.Invoke(this, new TouchEventArgs(TouchAction.Moved, point));
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			var viewPoints = this.GetPointsInView(evt);
			PointF viewPoint = viewPoints.Length > 0 ? viewPoints[0] : PointF.Zero;
			var point = new Point(viewPoint.X, viewPoint.Y);

			Touch?.Invoke(this, new TouchEventArgs(TouchAction.Released, point));
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);

			var viewPoints = this.GetPointsInView(evt);
			PointF viewPoint = viewPoints.Length > 0 ? viewPoints[0] : PointF.Zero;
			var point = new Point(viewPoint.X, viewPoint.Y);

			Touch?.Invoke(this, new TouchEventArgs(TouchAction.Cancelled, point));
		}
	}
}