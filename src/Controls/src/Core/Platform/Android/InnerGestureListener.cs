#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Graphics;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Platform
{
	internal class InnerGestureListener : Object, GestureDetector.IOnGestureListener, GestureDetector.IOnDoubleTapListener
	{
		TapGestureHandler _tapGestureHandler;
		PanGestureHandler _panGestureHandler;
		SwipeGestureHandler _swipeGestureHandler;
		DragAndDropGestureHandler _dragAndDropGestureHandler;
		PointerGestureHandler _pointerGestureHandler;
		bool _isScrolling;
		float _lastX;
		float _lastY;
		bool _disposed;

		// Simple multi-tap counting - much simpler approach
		int _tapCount = 0;
		long _lastTapTime = 0;
		float _lastTapX = -1;
		float _lastTapY = -1;
		const long TapTimeoutMs = 500; // 500ms between taps
		const float TapDistanceThreshold = 30; // 30px max distance between taps

		Func<float, float, bool> _swipeDelegate;
		Func<bool> _swipeCompletedDelegate;
		Func<bool> _scrollCompleteDelegate;
		Func<float, float, int, bool> _scrollDelegate;
		Func<int, bool> _scrollStartedDelegate;
		Func<int, MotionEvent, bool> _tapDelegate;
		Func<int, IEnumerable<TapGestureRecognizer>> _tapGestureRecognizers;

		public InnerGestureListener(
			TapGestureHandler tapGestureHandler,
			PanGestureHandler panGestureHandler,
			SwipeGestureHandler swipeGestureHandler,
			DragAndDropGestureHandler dragAndDropGestureHandler,
			PointerGestureHandler pointerGestureHandler)
		{
			_ = tapGestureHandler ?? throw new ArgumentNullException(nameof(tapGestureHandler));
			_ = panGestureHandler ?? throw new ArgumentNullException(nameof(panGestureHandler));
			_ = swipeGestureHandler ?? throw new ArgumentNullException(nameof(swipeGestureHandler));
			_ = dragAndDropGestureHandler ?? throw new ArgumentNullException(nameof(dragAndDropGestureHandler));
			_ = pointerGestureHandler ?? throw new ArgumentNullException(nameof(pointerGestureHandler));

			_tapGestureHandler = tapGestureHandler;
			_panGestureHandler = panGestureHandler;
			_swipeGestureHandler = swipeGestureHandler;
			_dragAndDropGestureHandler = dragAndDropGestureHandler;
			_pointerGestureHandler = pointerGestureHandler;

			_tapDelegate = tapGestureHandler.OnTap;
			_tapGestureRecognizers = tapGestureHandler.TapGestureRecognizers;
			_scrollDelegate = panGestureHandler.OnPan;
			_scrollStartedDelegate = panGestureHandler.OnPanStarted;
			_scrollCompleteDelegate = panGestureHandler.OnPanComplete;
			_swipeDelegate = swipeGestureHandler.OnSwipe;
			_swipeCompletedDelegate = swipeGestureHandler.OnSwipeComplete;
		}

		public bool EnableLongPressGestures =>
			_dragAndDropGestureHandler.HasAnyDragGestures();

		bool HasAnyGestures()
		{
			return (_panGestureHandler?.HasAnyGestures() ?? false) || (_tapGestureHandler?.HasAnyGestures() ?? false) || (_swipeGestureHandler?.HasAnyGestures() ?? false);
		}

		// This is needed because GestureRecognizer callbacks can be delayed several hundred milliseconds
		// which can result in the need to resurrect this object if it has already been disposed. We dispose
		// eagerly to allow easier garbage collection of the renderer
		internal InnerGestureListener(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTap(MotionEvent e)
		{
			if (_disposed)
				return false;

			// Use our simplified tap counting for multi-tap gestures
			return ProcessTap(e);
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTapEvent(MotionEvent e)
		{
			// Let OnDoubleTap handle the logic
			return false;
		}

		bool GestureDetector.IOnGestureListener.OnDown(MotionEvent e)
		{
			SetStartingPosition(e);

			if (HasAnyGestures())
			{
				// If we have any gestures to listen for, we need to return true to show we're interested in the rest
				// of the events.		
				return true;
			}

			// Since we don't have any gestures we're listening for, we return false to show we're not interested
			// and let parent controls have a whack at the events
			return false;
		}

		bool GestureDetector.IOnGestureListener.OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			EndScrolling();
			return false;
		}

		void GestureDetector.IOnGestureListener.OnLongPress(MotionEvent e)
		{
			SetStartingPosition(e);
			_dragAndDropGestureHandler.OnLongPress(e);
		}

		bool GestureDetector.IOnGestureListener.OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
		{
			if (e1 == null || e2 == null)
				return false;

			SetStartingPosition(e1);

			return StartScrolling(e2);
		}

		void GestureDetector.IOnGestureListener.OnShowPress(MotionEvent e)
		{
		}

		bool GestureDetector.IOnGestureListener.OnSingleTapUp(MotionEvent e)
		{
			if (_disposed)
				return false;

			// Use our simplified tap counting for all tap gestures
			return ProcessTap(e);
		}

		bool GestureDetector.IOnDoubleTapListener.OnSingleTapConfirmed(MotionEvent e)
		{
			if (_disposed)
				return false;

			// For legacy compatibility, but our main logic is in ProcessTap
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				_panGestureHandler = null;
				_swipeGestureHandler = null;
				_tapGestureHandler = null;

				_tapDelegate = null;
				_tapGestureRecognizers = null;
				_scrollDelegate = null;
				_scrollStartedDelegate = null;
				_scrollCompleteDelegate = null;
				_swipeDelegate = null;
				_swipeCompletedDelegate = null;
				_dragAndDropGestureHandler?.Dispose();
				_dragAndDropGestureHandler = null;
				_pointerGestureHandler = null;
			}

			base.Dispose(disposing);
		}

		void SetStartingPosition(MotionEvent e1)
		{
			_lastX = e1.GetX();
			_lastY = e1.GetY();
		}

		bool StartScrolling(MotionEvent e2)
		{
			if (_scrollDelegate == null)
				return false;

			if (!_isScrolling && _scrollStartedDelegate != null)
				_scrollStartedDelegate(e2.PointerCount);

			_isScrolling = true;

			float totalX = e2.GetX() - _lastX;
			float totalY = e2.GetY() - _lastY;

			return _scrollDelegate(totalX, totalY, e2.PointerCount) || _swipeDelegate(totalX, totalY);
		}

		internal void EndScrolling()
		{
			if (_isScrolling && _scrollCompleteDelegate != null)
				_scrollCompleteDelegate();

			if (_isScrolling && _swipeCompletedDelegate != null)
				_swipeCompletedDelegate();

			_isScrolling = false;
		}

		bool ProcessTap(MotionEvent e)
		{
			if (e == null)
				return false;

			long currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
			float currentX = e.GetX();
			float currentY = e.GetY();

			// Check if this is a sequential tap
			bool isSequential = false;
			if (_lastTapTime > 0)
			{
				long timeDiff = currentTime - _lastTapTime;
				float distance = (float)Math.Sqrt(Math.Pow(currentX - _lastTapX, 2) + Math.Pow(currentY - _lastTapY, 2));
				
				if (timeDiff <= TapTimeoutMs && distance <= TapDistanceThreshold)
				{
					isSequential = true;
				}
			}

			if (isSequential)
			{
				_tapCount++;
			}
			else
			{
				_tapCount = 1;
			}

			_lastTapTime = currentTime;
			_lastTapX = currentX;
			_lastTapY = currentY;

			// Check if we have a handler for this tap count
			var handlers = _tapGestureRecognizers?.Invoke(_tapCount);
			if (handlers?.Any() == true)
			{
				// Fire immediately - we have an exact match
				bool handled = _tapDelegate(_tapCount, e);
				_tapCount = 0; // Reset for next sequence
				_lastTapTime = 0;
				return handled;
			}

			// No exact handler found yet - wait for potential additional taps
			// Use a simple delayed check instead of complex timers
			Task.Delay((int)TapTimeoutMs).ContinueWith(_ =>
			{
				// Check if we're still on the same tap sequence
				if (_tapCount > 0 && (Java.Lang.JavaSystem.CurrentTimeMillis() - _lastTapTime) >= TapTimeoutMs)
				{
					// Time expired, check if we have any handler for the current count
					var timeoutHandlers = _tapGestureRecognizers?.Invoke(_tapCount);
					if (timeoutHandlers?.Any() == true)
					{
						_tapDelegate(_tapCount, null);
					}
					_tapCount = 0;
					_lastTapTime = 0;
				}
			}, TaskScheduler.FromCurrentSynchronizationContext());

			return false; // Don't consume the event yet
		}

		bool HasSingleTapHandlerWithPrimaryAndSecondary()
		{
			if (_tapGestureRecognizers == null)
				return false;

			var check = ButtonsMask.Primary | ButtonsMask.Secondary;
			foreach (var gesture in _tapGestureRecognizers(1))
			{
				if ((gesture.Buttons & check) == check)
					return true;
			}

			return false;
		}

		bool HasDoubleTapHandler()
		{
			if (_tapGestureRecognizers == null)
				return false;
			return _tapGestureRecognizers(2).Any();
		}

		bool HasSingleTapHandler()
		{
			if (_tapGestureRecognizers == null)
				return false;

			return _tapGestureRecognizers(1).Any();
		}
	}
}