#nullable disable
using System;
using System.Linq;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public class CustomNativeGraphicsView : NativeGraphicsView
	{
		public event EventHandler<PointF[]> OnTouchesBegan;
		public event EventHandler<PointF[]> OnTouchesMoved;
		public event EventHandler<PointF[]> OnTouchesEnded;
		public event EventHandler<PointF[]> OnTouchesCancelled;

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			var viewPoints = this.GetPointsInView(evt);
			if (viewPoints?.Any() ?? false)
				OnTouchesBegan?.Invoke(this, viewPoints);
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);

			var viewPoints = this.GetPointsInView(evt);
			if (viewPoints?.Any() ?? false)
				OnTouchesMoved?.Invoke(this, new TouchEventArgs(TouchAction.Moved, point));
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			var viewPoints = this.GetPointsInView(evt);
			if (viewPoints?.Any() ?? false)
				OnTouchesEnded?.Invoke(this, new TouchEventArgs(TouchAction.Released, point));
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);

			var viewPoints = this.GetPointsInView(evt);
			if (viewPoints?.Any() ?? false)
				OnTouchesCancelled?.Invoke(this, new TouchEventArgs(TouchAction.Cancelled, point));
		}
	}
}