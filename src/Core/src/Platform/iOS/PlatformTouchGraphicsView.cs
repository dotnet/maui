using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
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
			if (OperatingSystem.IsIOSVersionAtLeast(13))
				AddGestureRecognizer(hoverGesture = new CustomHover(OnHover));
		}

		public void Disconnect()
		{
			RemoveGestureRecognizer(hoverGesture!);
			hoverGesture = null;
			graphicsView = null;
		}

		static void OnHover(UIHoverGestureRecognizer hoverGesture)
		{
			var platformView = hoverGesture.View as PlatformTouchGraphicsView;

			if (platformView == null)
				return;

			var graphicsView = platformView.graphicsView;

			if (hoverGesture!.State == UIGestureRecognizerState.Began)
			{
				var touch = hoverGesture.LocationInView(platformView);
				graphicsView?.StartHoverInteraction(new[] { (PointF)touch.ToPoint() });
			}
			else if (hoverGesture.State == UIGestureRecognizerState.Changed)
			{
				var touch = hoverGesture.LocationInView(platformView);
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

		public override void TouchesMoved(NSSet touches, UIEvent? evt)
		{
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

		// this is used so that I can provide a static action
		// which lets us avoid circular references and memory leaks
		class CustomHover : UIHoverGestureRecognizer
		{
#pragma warning disable CA1416
			public CustomHover(Action<UIHoverGestureRecognizer> action)
				: base(new Callback(action), Selector.FromHandle(Selector.GetHandle("target:"))!) { }
#pragma warning restore CA1416

			[Register("__UIHoverGestureRecognizer")]
			class Callback : Token
			{
				Action<UIHoverGestureRecognizer> action;
				internal Callback(Action<UIHoverGestureRecognizer> action)
				{
					this.action = action;
				}
				[Export("target:")]
				[Preserve(Conditional = true)]
				public void Activated(UIHoverGestureRecognizer sender)
				{
					if (OperatingSystem.IsIOSVersionAtLeast(13))
						action(sender);
				}
			}
		}
	}
}
