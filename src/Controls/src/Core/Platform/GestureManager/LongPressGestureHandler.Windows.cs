using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WinPoint = Windows.Foundation.Point;
using MauiPoint = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui.Controls.Platform
{
	class LongPressGestureHandler : IDisposable
	{
		readonly IPlatformViewHandler _handler;
		DispatcherTimer? _timer;
		WinPoint _startPosition;
		bool _isLongPressing;

		public LongPressGestureHandler(IPlatformViewHandler handler)
		{
			_handler = handler;
		}

		public void SubscribeEvents()
		{
			var container = GetContainer();
			if (container == null)
				return;

			container.PointerPressed += OnPointerPressed;
			container.PointerReleased += OnPointerReleased;
			container.PointerCanceled += OnPointerCanceled;
			container.PointerMoved += OnPointerMoved;
		}

		public void UnsubscribeEvents()
		{
			var container = GetContainer();
			if (container != null)
			{
				container.PointerPressed -= OnPointerPressed;
				container.PointerReleased -= OnPointerReleased;
				container.PointerCanceled -= OnPointerCanceled;
				container.PointerMoved -= OnPointerMoved;
			}
			CancelTimer();
		}

		FrameworkElement? GetContainer() => 
			(_handler.ContainerView ?? _handler.PlatformView) as FrameworkElement;

		View? GetView() => _handler.VirtualView as View;

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var view = GetView();
			if (view == null)
				return;

			var container = GetContainer();
			if (container == null)
				return;

			var pointerPoint = e.GetCurrentPoint(container);
			_startPosition = pointerPoint.Position;
			_isLongPressing = false;

			StartTimers(view, new MauiPoint(_startPosition.X, _startPosition.Y));

			// Don't set e.Handled - allow other gesture handlers to process
		}

		void OnPointerMoved(object sender, PointerRoutedEventArgs e)
		{
			var view = GetView();
			if (view == null)
				return;

			var recognizers = view.GestureRecognizers;
			if (recognizers == null)
				return;

			var container = GetContainer();
			if (container == null)
				return;

			var pointerPoint = e.GetCurrentPoint(container);
			var currentPos = pointerPoint.Position;
			var deltaX = Math.Abs(currentPos.X - _startPosition.X);
			var deltaY = Math.Abs(currentPos.Y - _startPosition.Y);

			foreach (var recognizer in recognizers)
			{
				if (recognizer is LongPressGestureRecognizer longPress)
				{
					if (deltaX > longPress.AllowableMovement || deltaY > longPress.AllowableMovement)
					{
						CancelTimer();
						break;
					}
				}
			}

			// Don't set e.Handled - allow scrolling/panning to work
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			CancelTimer();
		}

		void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
		{
			CancelTimer();
		}

		void StartTimers(View view, MauiPoint position)
		{
			var recognizers = view.GestureRecognizers;
			if (recognizers == null)
				return;

			// Note: If multiple LongPressGestureRecognizers are present on the same view,
			// only the last one's MinimumPressDuration is used. This is an edge case scenario
			// that is not commonly used in practice (typically one LongPress per element).
			foreach (var recognizer in recognizers)
			{
				if (recognizer is LongPressGestureRecognizer longPress)
				{
					var duration = TimeSpan.FromMilliseconds(longPress.MinimumPressDuration);
					
					// Use DispatcherTimer which automatically runs on UI thread
					_timer = new DispatcherTimer();
					_timer.Interval = duration;
					_timer.Tick += (s, e) =>
					{
						if (_isLongPressing)
							return; // Already fired

						_isLongPressing = true;
						
						// Create position function that calculates relative position
						Func<IElement?, MauiPoint?> getPosition = (relativeTo) => CalculatePosition(relativeTo, position, _handler);
						
						// Fire for ALL LongPress recognizers on this view
						foreach (var r in view.GestureRecognizers)
						{
							if (r is LongPressGestureRecognizer lp)
							{
								lp.SendLongPressed(view, getPosition);
								lp.SendLongPressing(view, GestureStatus.Completed, getPosition);
							}
						}
						
						CancelTimer();
					};
					_timer.Start();
				}
			}
		}

		static MauiPoint? CalculatePosition(IElement? relativeTo, MauiPoint originPoint, IPlatformViewHandler handler)
		{
			var virtualView = handler?.VirtualView as View;
			if (virtualView == null)
				return null;

			// If relativeTo is null or same as the view, return position relative to the view
			if (relativeTo == null || relativeTo == virtualView)
				return originPoint;

			// Calculate position relative to another element
			var targetViewScreenLocation = virtualView.GetLocationOnScreen();
			if (!targetViewScreenLocation.HasValue)
				return null;

			var windowX = targetViewScreenLocation.Value.X + originPoint.X;
			var windowY = targetViewScreenLocation.Value.Y + originPoint.Y;

			var relativeViewLocation = ((View)relativeTo).GetLocationOnScreen();
			if (!relativeViewLocation.HasValue)
				return new MauiPoint(windowX, windowY);

			return new MauiPoint(windowX - relativeViewLocation.Value.X, windowY - relativeViewLocation.Value.Y);
		}

		void CancelTimer()
		{
			if (_timer != null)
			{
				_timer.Stop();
				_timer = null;
			}
			_isLongPressing = false;
		}

		public void Dispose()
		{
			UnsubscribeEvents();
		}
	}
}
