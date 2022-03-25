using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class PlatformTouchGraphicsView : PlatformGraphicsView
	{

		IGraphicsView? graphicsView;
		UIHoverGestureRecognizer? hoverGesture;
		RectF rect;
		bool pressedContained = false;

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			rect = this.Bounds.AsRectangleF();
		}

		public void Connect(IGraphicsView graphicsView)
		{
			this.graphicsView = graphicsView;
			AddGestureRecognizer(hoverGesture = new UIHoverGestureRecognizer(OnHover));
		}
		public void Disconnect()
		{
			RemoveGestureRecognizer(hoverGesture!);
			hoverGesture = null;
			graphicsView = null;
		}

		void OnHover()
		{
			if (hoverGesture!.State == UIGestureRecognizerState.Began)
			{
				var touch = hoverGesture.LocationInView(this);
				graphicsView?.StartHoverInteraction(new[] { (PointF)touch.ToPoint() });
			}
			else if (hoverGesture.State == UIGestureRecognizerState.Changed)
			{
				var touch = hoverGesture.LocationInView(this);
				graphicsView?.MoveHoverInteraction(new[] { (PointF)touch.ToPoint() });
			}
			else
				graphicsView?.EndHoverInteraction();
		}

		public override void TouchesBegan(NSSet touches, UIEvent? evt)
		{
			if (!IsFirstResponder)
				BecomeFirstResponder();
			var viewPoints = this.GetPointsInView(evt);
			graphicsView?.StartInteraction(viewPoints);
			pressedContained = true;
		}

		public override void TouchesMoved(NSSet touches, UIEvent? evt) {
			var viewPoints = this.GetPointsInView(evt);
			pressedContained = rect.ContainsAny(viewPoints);
			graphicsView?.DragInteraction(viewPoints);
		}
		public override void TouchesEnded(NSSet touches, UIEvent? evt) =>
			graphicsView?.EndInteraction(this.GetPointsInView(evt), pressedContained);

		public override void TouchesCancelled(NSSet touches, UIEvent? evt)
		{
			pressedContained = false;
			graphicsView?.CancelInteraction();
		}
	}
}
