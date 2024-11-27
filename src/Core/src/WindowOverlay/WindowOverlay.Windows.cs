﻿using System;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public partial class WindowOverlay
	{
		W2DGraphicsView? _graphicsView;
		Frame? _frame;
		WindowRootViewContainer? _panel;
		WeakReference<FrameworkElement>? _platformElement;

		/// <inheritdoc/>
		public virtual bool Initialize()
		{
			if (IsPlatformViewInitialized)
				return true;

			if (Window?.Content == null)
				return false;

			var platformElement = Window.Content.ToPlatform();
			if (platformElement is null)
				return false;
			_platformElement = new(platformElement);

			var handler = Window.Handler as WindowHandler;
			if (handler?.PlatformView is not Window _window)
				return false;

			_panel = _window.Content as WindowRootViewContainer;
			if (_panel is null)
				return false;

			// Capture when the frame is navigating.
			// When it is, we will clear existing adorners.
			if (platformElement is Frame frame)
			{
				_frame = frame;
				_frame.Navigating += FrameNavigating;
			}

			_graphicsView = new W2DGraphicsView() { Drawable = this };
			if (_graphicsView == null)
				return false;

			platformElement.Tapped += ViewTapped;
			platformElement.PointerMoved += PointerMoved;
			_graphicsView.Tapped += ViewTapped;
			_graphicsView.PointerMoved += PointerMoved;

			_graphicsView.SetValue(Canvas.ZIndexProperty, 99);
			_graphicsView.IsHitTestVisible = false;
			_graphicsView.Visibility = UI.Xaml.Visibility.Collapsed;

			_panel.AddOverlay(_graphicsView);

			IsPlatformViewInitialized = true;
			return IsPlatformViewInitialized;
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			if (_graphicsView is null)
				return;

			// Hide the visibility of the graphics view if there are no drawn elements.
			// This way, the In-App Toolbar will work as expected.
			_graphicsView.Visibility = WindowElements.Count == 0 ? UI.Xaml.Visibility.Collapsed : UI.Xaml.Visibility.Visible;
			_graphicsView.Invalidate();
		}

		/// <summary>
		/// Deinitializes the platform event hooks and handlers used to drive the overlay.
		/// </summary>
		void DeinitializePlatformDependencies()
		{
			if (_frame != null)
				_frame.Navigating -= FrameNavigating;
			if (_platformElement is not null && _platformElement.TryGetTarget(out var platformElement))
			{
				platformElement.Tapped -= ViewTapped;
				platformElement.PointerMoved -= PointerMoved;
			}
			if (_graphicsView != null)
			{
				_graphicsView.Tapped -= ViewTapped;
				_graphicsView.PointerMoved -= PointerMoved;
				if (_panel != null)
					_panel.RemoveOverlay(_graphicsView);
				_graphicsView = null;
			}
			IsPlatformViewInitialized = false;
		}

		void PointerMoved(object sender, UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			if (!EnableDrawableTouchHandling)
				return;

			if (_windowElements.Count == 0)
				return;

			if (_graphicsView == null)
				return;

			var pointerPoint = e.GetCurrentPoint(_graphicsView);
			if (pointerPoint == null)
				return;

			_graphicsView.IsHitTestVisible = _windowElements.Any(n => n.Contains(new Point(pointerPoint.Position.X, pointerPoint.Position.Y)));
		}

		void FrameNavigating(object sender, UI.Xaml.Navigation.NavigatingCancelEventArgs e)
		{
			HandleUIChange();
			Invalidate();
		}

		void ViewTapped(object sender, UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			if (e == null)
				return;
			var position = e.GetPosition(_graphicsView);
			var point = new Point(position.X, position.Y);

			if (DisableUITouchEventPassthrough)
				e.Handled = true;
			else if (EnableDrawableTouchHandling)
				e.Handled = _windowElements.Any(n => n.Contains(point));

			OnTappedInternal(point);
		}

		partial void OnDisableUITouchEventPassthroughSet()
		{
			if (_graphicsView != null)
				_graphicsView.IsHitTestVisible = DisableUITouchEventPassthrough;
		}
	}
}