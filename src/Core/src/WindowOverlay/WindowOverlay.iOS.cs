using System;
using System.Linq;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui
{
	public partial class WindowOverlay
	{
		PassthroughView? _passthroughView;
		IDisposable? _frameObserver;
		PlatformGraphicsView? _graphicsView;

		public virtual bool Initialize()
		{
			if (IsPlatformViewInitialized)
				return true;

			var nativeLayer = Window?.ToPlatform();
			if (nativeLayer is not UIWindow platformWindow)
				return false;

			if (platformWindow?.RootViewController?.View == null)
				return false;

			// Create a passthrough view for holding the canvas and other diagnostics tools.
			_passthroughView = new PassthroughView(this, platformWindow.RootViewController.View.Frame);
			_passthroughView.AutoresizingMask = UIViewAutoresizing.All;

			_graphicsView = new PlatformGraphicsView(_passthroughView.Frame, this, new DirectRenderer());
			_graphicsView.AutoresizingMask = UIViewAutoresizing.All;

			_passthroughView.AddSubview(_graphicsView);

			if (_graphicsView == null)
			{
				return false;
			}

			// Any time the frame gets a new value, we need to update and invalidate the canvas.
			_frameObserver = nativeLayer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.New, FrameAction);
			// Disable the graphics view from user input.
			// This will be handled by the passthrough view.
			_graphicsView.UserInteractionEnabled = false;

			// Make the canvas view transparent.
			_graphicsView.BackgroundColor = UIColor.FromWhiteAlpha(1, 0.0f);

			// Add the passthrough view to the front of the stack.
			platformWindow.RootViewController.View.AddSubview(_passthroughView);
			platformWindow.RootViewController.View.BringSubviewToFront(_passthroughView);

			IsPlatformViewInitialized = true;
			return IsPlatformViewInitialized;
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			_graphicsView?.InvalidateIntrinsicContentSize();
			_graphicsView?.InvalidateDrawable();
		}

		/// <summary>
		/// Deinitializes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		void DeinitializePlatformDependencies()
		{
			_frameObserver?.Dispose();
			_passthroughView?.RemoveFromSuperview();
			_passthroughView?.Dispose();
			IsPlatformViewInitialized = false;
		}

		void FrameAction(Foundation.NSObservedChange obj)
		{
			HandleUIChange();
			Invalidate();
		}

		class PassthroughView : UIView
		{
			readonly WeakReference<WindowOverlay> _overlay;

			/// <summary>
			/// Initializes a new instance of the <see cref="PassthroughView"/> class.
			/// </summary>
			/// <param name="windowOverlay">The Window Overlay.</param>
			/// <param name="frame">Base Frame.</param>
			public PassthroughView(WindowOverlay windowOverlay, CGRect frame)
				: base(frame)
			{
				_overlay = new(windowOverlay);
			}

			public override bool PointInside(CGPoint point, UIEvent? uievent)
			{
				// If we don't have a UI event, return.
				if (uievent == null)
					return false;

				if (uievent.Type == UIEventType.Hover)
					return false;

				// If we are not pressing down, return.
				if (uievent.Type != UIEventType.Touches)
					return false;

				var disableTouchEvent = false;

				if (_overlay.TryGetTarget(out var overlay))
				{
					if (overlay.DisableUITouchEventPassthrough)
						disableTouchEvent = true;
					else if (overlay.EnableDrawableTouchHandling)
						disableTouchEvent = overlay.WindowElements.Any(n => n.Contains(new Point(point.X, point.Y)));

					overlay.OnTappedInternal(new Point(point.X, point.Y));
				}

				return disableTouchEvent;
			}
		}
	}
}