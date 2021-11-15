using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Core;

namespace Microsoft.Maui
{
	public partial class WindowOverlay : IWindowOverlay, IDrawable
	{
		private W2DGraphicsView? _graphicsView;
		private bool disableUITouchEventPassthrough;
		private Frame? _frame;
		private RootPanel? _rootPanel;
		private FrameworkElement? _nativeWindow;

		/// <inheritdoc/>
		public bool DisableUITouchEventPassthrough
		{
			get { return disableUITouchEventPassthrough; }
			set
			{
				disableUITouchEventPassthrough = value;
				if (this._graphicsView != null)
					this._graphicsView.IsHitTestVisible = value;
			}
		}

		/// <inheritdoc/>
		public bool InitializeNativeLayer()
		{
			if (this.IsNativeViewInitialized)
				return true;

			if (this.Window == null)
				return false;

			_nativeWindow = this.Window.Content.GetNative(true);
			if (_nativeWindow == null)
				return false;
			var handler = this.Window.Handler as WindowHandler;
			if (handler == null || handler._rootPanel == null)
				return false;

			this._rootPanel = handler._rootPanel;
			// Capture when the frame is navigating.
			// When it is, we will clear existing adorners.
			if (_nativeWindow is Frame frame)
			{
				_frame = frame;
				_frame.Navigating += Frame_Navigating;
			}

			this._graphicsView = new W2DGraphicsView() { Drawable = this };
			if (this._graphicsView == null)
				return false;

			_nativeWindow.Tapped += GraphicsView_Tapped;
			this._graphicsView.Tapped += GraphicsView_Tapped;

			this._graphicsView.SetValue(Canvas.ZIndexProperty, 99);
			this._graphicsView.IsHitTestVisible = false;
			handler._rootPanel.Children.Add(this._graphicsView);
			this.IsNativeViewInitialized = true;
			return this.IsNativeViewInitialized;
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			this._graphicsView?.Invalidate();
		}

		/// <summary>
		/// Disposes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		private void DisposeNativeDependencies()
		{
			if (_frame != null)
				_frame.Navigating -= Frame_Navigating;
			if (this._rootPanel != null)
				this._rootPanel.Children.Remove(this._graphicsView);
			if (this._nativeWindow != null)
				this._nativeWindow.Tapped -= GraphicsView_Tapped;
			if (this._graphicsView != null)
				this._graphicsView.Tapped -= GraphicsView_Tapped;
			this._graphicsView = null;
			this.IsNativeViewInitialized = false;
		}

		private void Frame_Navigating(object sender, UI.Xaml.Navigation.NavigatingCancelEventArgs e)
		{
			this.HandleUIChange();
			this.Invalidate();
		}

		private void GraphicsView_Tapped(object sender, UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			if (e == null)
				return;
			var position = e.GetPosition(this._graphicsView);
			var point = new Point(position.X, position.Y);

			if (this.DisableUITouchEventPassthrough)
				e.Handled = true;
			else if (this.EnableDrawableTouchHandling)
				e.Handled = this._windowElements.Any(n => n.IsPointInElement(point));

			this.OnTouchInternal(point);
		}
	}
}
