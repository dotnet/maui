using System;
using System.Collections.Generic;
using System.Linq;
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
		readonly Dictionary<LongPressGestureRecognizer, DispatcherTimer> _timers = new();
		WinPoint _startPosition;
		readonly HashSet<LongPressGestureRecognizer> _firedRecognizers = new();

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
			CancelTimers();
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

			// Cancel any existing timers from a previous press before starting new ones
			CancelTimers();

			var pointerPoint = e.GetCurrentPoint(container);
			_startPosition = pointerPoint.Position;
			_firedRecognizers.Clear();

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

			foreach (var recognizer in recognizers.ToList())
			{
				if (recognizer is LongPressGestureRecognizer longPress)
				{
					if (deltaX > longPress.AllowableMovement || deltaY > longPress.AllowableMovement)
					{
						CancelTimer(longPress);
					}
				}
			}

			// Don't set e.Handled - allow scrolling/panning to work
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			CancelTimers();
		}

		void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
		{
			CancelTimers();
		}

		void StartTimers(View view, MauiPoint position)
		{
			// Snapshot recognizers to avoid collection-modified issues in timer callbacks
			var longPressRecognizers = view.GestureRecognizers.OfType<LongPressGestureRecognizer>().ToList();
			if (longPressRecognizers.Count == 0)
				return;

			// Create a separate timer for each recognizer so each fires at its own MinimumPressDuration
			foreach (var longPress in longPressRecognizers)
			{
				var duration = TimeSpan.FromMilliseconds(longPress.MinimumPressDuration);
				var recognizer = longPress;

				var timer = new DispatcherTimer();
				timer.Interval = duration;
				timer.Tick += (s, e) =>
				{
					// Stop this individual timer immediately
					((DispatcherTimer)s!).Stop();

					if (!_firedRecognizers.Add(recognizer))
						return;

					Func<IElement?, MauiPoint?> getPosition = (relativeTo) => CalculatePosition(relativeTo, position, _handler);

					recognizer.SendLongPressed(view, getPosition);
					recognizer.SendLongPressing(view, GestureStatus.Completed, getPosition);
				};
				timer.Start();
				_timers[longPress] = timer;
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

		void CancelTimer(LongPressGestureRecognizer recognizer)
		{
			if (_timers.TryGetValue(recognizer, out var timer))
			{
				timer.Stop();
				_timers.Remove(recognizer);
			}
			_firedRecognizers.Remove(recognizer);
		}

		void CancelTimers()
		{
			foreach (var timer in _timers.Values)
			{
				timer.Stop();
			}
			_timers.Clear();
			_firedRecognizers.Clear();
		}

		public void Dispose()
		{
			UnsubscribeEvents();
			CancelTimers();
		}
	}
}
