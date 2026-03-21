using System;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class PlatformTouchGraphicsView : PlatformGraphicsView
	{
		readonly UIHoverGestureRecognizerProxy _proxy;
		WeakReference<IGraphicsView>? _graphicsView;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		UIHoverGestureRecognizer? _hoverGesture;
		RectF _rect;
		bool _pressedContained;

		public PlatformTouchGraphicsView()
		{
			_proxy = new(this);
			Opaque = false;
			BackgroundColor = null;
			MultipleTouchEnabled = true;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			_rect = Bounds.AsRectangleF();
		}

		public void Connect(IGraphicsView graphicsView)
		{
			_graphicsView = new(graphicsView);

			if (OperatingSystem.IsIOSVersionAtLeast(13))
				AddGestureRecognizer(_hoverGesture = new UIHoverGestureRecognizer(_proxy.OnHover));
		}

		public void Disconnect()
		{
			RemoveGestureRecognizer(_hoverGesture!);
			_hoverGesture = null;
			_graphicsView = null;
		}

		public override void TouchesBegan(NSSet touches, UIEvent? evt)
		{
			if (_graphicsView is null || !_graphicsView.TryGetTarget(out var graphicsView) || !graphicsView.IsEnabled)
			{
				return;
			}

			if (!IsFirstResponder)
				BecomeFirstResponder();
			var viewPoints = this.GetPointsInView(evt);
			graphicsView.StartInteraction(viewPoints);
			_pressedContained = true;
		}

		public override void TouchesMoved(NSSet touches, UIEvent? evt)
		{
			if (_graphicsView is null || !_graphicsView.TryGetTarget(out var graphicsView) || !graphicsView.IsEnabled)
			{
				return;
			}
			var viewPoints = this.GetPointsInView(evt);
			_pressedContained = _rect.ContainsAny(viewPoints);
			graphicsView.DragInteraction(viewPoints);
		}

		public override void TouchesEnded(NSSet touches, UIEvent? evt)
		{
			if (_graphicsView is null || !_graphicsView.TryGetTarget(out var graphicsView) || !graphicsView.IsEnabled)
			{
				return;
			}
			graphicsView.EndInteraction(this.GetPointsInView(evt), _pressedContained);
		}

		public override void TouchesCancelled(NSSet touches, UIEvent? evt)
		{
			if (_graphicsView is null || !_graphicsView.TryGetTarget(out var graphicsView) || !graphicsView.IsEnabled)
			{
				return;
			}
			_pressedContained = false;
			graphicsView.CancelInteraction();
		}

		class UIHoverGestureRecognizerProxy
		{
			readonly WeakReference<PlatformTouchGraphicsView> _platformView;

			public UIHoverGestureRecognizerProxy(PlatformTouchGraphicsView platformView) => _platformView = new(platformView);

			public void OnHover()
			{
				if (!_platformView.TryGetTarget(out var platformView))
				{
					return;
				}

				if (platformView._graphicsView is null || !platformView._graphicsView.TryGetTarget(out var graphicsView) || !graphicsView.IsEnabled)
				{
					return;
				}

				var hoverGesture = platformView._hoverGesture;
				if (hoverGesture!.State == UIGestureRecognizerState.Began)
				{
					var touch = hoverGesture.LocationInView(platformView);
					graphicsView.StartHoverInteraction(new[] { (PointF)touch.ToPoint() });
				}
				else if (hoverGesture.State == UIGestureRecognizerState.Changed)
				{
					var touch = hoverGesture.LocationInView(platformView);
					graphicsView.MoveHoverInteraction(new[] { (PointF)touch.ToPoint() });
				}
				else
					graphicsView.EndHoverInteraction();
			}
		}
	}
}
