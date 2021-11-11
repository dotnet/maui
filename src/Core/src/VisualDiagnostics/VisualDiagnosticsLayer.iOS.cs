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
	/// <summary>
	/// Visual Diagnostics Layer.
	/// </summary>
	public partial class VisualDiagnosticsLayer : IVisualDiagnosticsLayer, IDrawable
	{
		private bool disableUITouchEventPassthrough;
		private PassthroughView? _passthroughView;
		private IDisposable? _frameObserver;

		/// <inheritdoc/>
		public HashSet<Tuple<IScrollView, IDisposable>> ScrollViews { get; } = new HashSet<Tuple<IScrollView, IDisposable>>();

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public NativeGraphicsView? VisualDiagnosticsGraphicsView { get; internal set; }

		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null)
			{
				var dispose = nativeScroll.AddObserver("contentOffset", Foundation.NSKeyValueObservingOptions.New, FrameAction);
				this.ScrollViews.Add(new Tuple<IScrollView, IDisposable>(scrollBar, dispose));
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scroll in this.ScrollViews)
			{
				scroll.Item2.Dispose();
			}

			this.ScrollViews.Clear();
		}

		/// <inheritdoc/>
		public void InitializeNativeLayer(IMauiContext context, UIKit.UIWindow nativeLayer)
		{

			if (nativeLayer.RootViewController == null || nativeLayer.RootViewController.View == null)
				return;

			// Create a passthrough view for holding the canvas and other diagnostics tools.
			_passthroughView = new PassthroughView(nativeLayer.RootViewController.View.Frame);

			this.VisualDiagnosticsGraphicsView = new NativeGraphicsView(_passthroughView.Frame, this, new DirectRenderer());
			_passthroughView.AddSubview(this.VisualDiagnosticsGraphicsView);

			if (this.VisualDiagnosticsGraphicsView == null)
			{
				System.Diagnostics.Debug.WriteLine("VisualDiagnosticsLayer: Could not set up touch layer canvas.");
				return;
			}

			// Any time the frame gets a new value, we need to update and invalidate the canvas.
			this._frameObserver = nativeLayer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.New, FrameAction);

			// Disable the graphics view from user input.
			// This will be handled by the passthrough view.
			this.VisualDiagnosticsGraphicsView.UserInteractionEnabled = false;
			
			// Make the canvas view transparent.
			this.VisualDiagnosticsGraphicsView.BackgroundColor = UIColor.FromWhiteAlpha(1, 0.0f);

			// Add the passthrough view to the front of the stack.
			nativeLayer.RootViewController.View.AddSubview(_passthroughView);
			nativeLayer.RootViewController.View.BringSubviewToFront(_passthroughView);

			// Any time the passthrough view is touched, handle it.
			this._passthroughView.OnTouch += _uiView_OnTouch;
			this.IsNativeViewInitialized = true;
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			this.VisualDiagnosticsGraphicsView?.InvalidateIntrinsicContentSize();
			this.VisualDiagnosticsGraphicsView?.InvalidateDrawable();
		}

		private void Scroll_Scrolled(object? sender, EventArgs e)
		{
			this.Invalidate();
		}

		private void _uiView_OnTouch(object? sender, CGPoint e) 
			=> OnTouchInternal(new Point(e.X, e.Y), true);

		private void FrameAction(Foundation.NSObservedChange obj)
		{
			if (this.AdornerBorders.Any())
				this.RemoveAdorners();

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
