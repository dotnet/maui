using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.CoreGraphics;
using Microsoft.Maui.Graphics.Native;
using UIKit;


namespace Microsoft.Maui
{
	public partial class WindowOverlay : IWindowOverlay, IDrawable
	{
		private bool disableUITouchEventPassthrough;
		private PassthroughView? _passthroughView;
		private IDisposable? _frameObserver;
		private NativeGraphicsView? _graphicsView;

		public bool DisableUITouchEventPassthrough
		{
			get { return disableUITouchEventPassthrough; }
			set
			{
				disableUITouchEventPassthrough = value;
				if (this._passthroughView != null)
					this._passthroughView.DisableUITouchEventPassthrough = value;
			}
		}

		public bool InitializeNativeLayer()
		{
			if (this.IsNativeViewInitialized)
				return true;

			if (this.Window == null)
				return false;

			var nativeLayer = this.Window.GetNative(true);
			if (nativeLayer == null || nativeLayer.Window == null)
				return false;

			var nativeWindow = nativeLayer.Window;

			if (nativeWindow.RootViewController == null || nativeWindow.RootViewController.View == null)
				return false;

			// Create a passthrough view for holding the canvas and other diagnostics tools.
			_passthroughView = new PassthroughView(nativeWindow.RootViewController.View.Frame);

			this._graphicsView = new NativeGraphicsView(_passthroughView.Frame, this, new DirectRenderer());
			_passthroughView.AddSubview(this._graphicsView);

			if (this._graphicsView == null)
			{
				return false;
			}

			// Any time the frame gets a new value, we need to update and invalidate the canvas.
			this._frameObserver = nativeLayer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.New, FrameAction);
			// Disable the graphics view from user input.
			// This will be handled by the passthrough view.
			this._graphicsView.UserInteractionEnabled = false;

			// Make the canvas view transparent.
			this._graphicsView.BackgroundColor = UIColor.FromWhiteAlpha(1, 0.0f);

			// Add the passthrough view to the front of the stack.
			nativeWindow.RootViewController.View.AddSubview(_passthroughView);
			nativeWindow.RootViewController.View.BringSubviewToFront(_passthroughView);

			// Any time the passthrough view is touched, handle it.
			this._passthroughView.OnTouch += _uiView_OnTouch;
			this.IsNativeViewInitialized = true;
			return this.IsNativeViewInitialized;
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			this._graphicsView?.InvalidateIntrinsicContentSize();
			this._graphicsView?.InvalidateDrawable();
		}

		/// <summary>
		/// Disposes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		private void DisposeNativeDependencies()
		{
			this._frameObserver?.Dispose();
			this._passthroughView?.Dispose();
			this.IsNativeViewInitialized = false;
		}

		private void _uiView_OnTouch(object? sender, CGPoint e) => OnTouchInternal(new Point(e.X, e.Y));

		private void FrameAction(Foundation.NSObservedChange obj)
		{
			this.HandleUIChange();
			this.Invalidate();
		}
	}

	internal class PassthroughView : UIView
	{
		/// <summary>
		/// Gets or sets a value whether to enable or disable the UI layer from handling touch events.
		/// </summary>
		public bool DisableUITouchEventPassthrough { get; set; }

		/// <summary>
		/// Event Handler for handling on touch events on the Passthrough View.
		/// </summary>
		public event EventHandler<CGPoint>? OnTouch;

		/// <summary>
		/// Initializes a new instance of the <see cref="PassthroughView"/> class.
		/// </summary>
		/// <param name="frame">Base Frame.</param>
		public PassthroughView(CGRect frame) : base(frame)
		{
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

			this.OnTouch?.Invoke(this, point);
			return DisableUITouchEventPassthrough;
		}
	}
}
