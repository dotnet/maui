using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public partial class WindowOverlay : IWindowOverlay, IDrawable
	{
		private W2DGraphicsView? _graphicsView;
		private bool disableUITouchEventPassthrough;
		private Frame? _frame;

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

			var nativeWindow = this.Window.Content.GetNative(true);
			var handler = this.Window.Handler as WindowHandler;
			if (handler == null || handler._rootPanel == null)
				return false;

			// Capture when the frame is navigating.
			// When it is, we will clear existing adorners.
			if (nativeWindow is Frame frame)
			{
				_frame = frame;
				_frame.Navigating += Frame_Navigating;
			}

			this._graphicsView = new W2DGraphicsView() { Drawable = this };
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

			e.Handled = this.DisableUITouchEventPassthrough;
			var position = e.GetPosition(this._graphicsView);

			var point = new Point(position.X, position.Y);
			this.OnTouchInternal(point);
		}
	}
}
