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
	public partial class WindowOverlay : IWindowOverlay
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
				if (_graphicsView != null)
					_graphicsView.IsHitTestVisible = value;
			}
		}

		/// <inheritdoc/>
		public virtual bool Initialize()
		{
			if (IsNativeViewInitialized)
				return true;

			if (Window?.Content == null)
				return false;

			_nativeWindow = Window.Content.GetNative(true);
			if (_nativeWindow == null)
				return false;
			var handler = Window.Handler as WindowHandler;
			if (handler == null || handler._rootPanel == null)
				return false;

			_rootPanel = handler._rootPanel;
			// Capture when the frame is navigating.
			// When it is, we will clear existing adorners.
			if (_nativeWindow is Frame frame)
			{
				_frame = frame;
				_frame.Navigating += FrameNavigating;
			}

			_graphicsView = new W2DGraphicsView() { Drawable = this };
			if (_graphicsView == null)
				return false;

			_nativeWindow.Tapped += ViewTapped;
			_nativeWindow.PointerMoved += PointerMoved;
			_graphicsView.Tapped += ViewTapped;
			_graphicsView.PointerMoved += PointerMoved;

			_graphicsView.SetValue(Canvas.ZIndexProperty, 99);
			_graphicsView.IsHitTestVisible = false;
			handler._rootPanel.Children.Add(_graphicsView);
			IsNativeViewInitialized = true;
			return IsNativeViewInitialized;
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			_graphicsView?.Invalidate();
		}

		/// <summary>
		/// Deinitializes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		private void DeinitializeNativeDependencies()
		{
			if (_frame != null)
				_frame.Navigating -= FrameNavigating;
			if (_rootPanel != null)
				_rootPanel.Children.Remove(_graphicsView);
			if (_nativeWindow != null)
			{
				_nativeWindow.Tapped -= ViewTapped;
				_nativeWindow.PointerMoved -= PointerMoved;
			}
			if (_graphicsView != null)
			{
				_graphicsView.Tapped -= ViewTapped;
				_graphicsView.PointerMoved -= PointerMoved;
			}
			_graphicsView = null;
			IsNativeViewInitialized = false;
		}

		private void PointerMoved(object sender, UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			if (!EnableDrawableTouchHandling)
				return;

			if (!_windowElements.Any())
				return;

			if (_graphicsView == null)
				return;

			var pointerPoint = e.GetCurrentPoint(_graphicsView);
			if (pointerPoint == null)
				return;

			this._graphicsView.IsHitTestVisible = _windowElements.Any(n => n.IsPointInElement(new Point(pointerPoint.Position.X, pointerPoint.Position.Y)));
		}

		private void FrameNavigating(object sender, UI.Xaml.Navigation.NavigatingCancelEventArgs e)
		{
			HandleUIChange();
			Invalidate();
		}

		private void ViewTapped(object sender, UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			if (e == null)
				return;
			var position = e.GetPosition(_graphicsView);
			var point = new Point(position.X, position.Y);

			if (DisableUITouchEventPassthrough)
				e.Handled = true;
			else if (EnableDrawableTouchHandling)
				e.Handled = _windowElements.Any(n => n.IsPointInElement(point));

			OnTouchInternal(point);
		}
	}
}
