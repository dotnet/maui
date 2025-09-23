#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
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

		// Multi-tap gesture support for taps > 2
		int _currentTapCount = 0;
		long _lastTapTime = 0;
		float _lastTapX = -1;
		float _lastTapY = -1;
		const long _maxTapInterval = 500; // milliseconds
		const float _maxTapDistance = 30; // pixels

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

			// Use the unified tap handling
			return HandleTapEvent(e);
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

			// Handle multi-tap gesture counting
			return HandleTapEvent(e);
		}

		bool GestureDetector.IOnDoubleTapListener.OnSingleTapConfirmed(MotionEvent e)
		{
			if (_disposed)
				return false;

			// For compatibility with existing single tap handling when we only have single tap handlers
			if (!HasMultiTapHandler() && !HasDoubleTapHandler())
			{
				return _tapDelegate(1, e);
			}

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

		bool HasMultiTapHandler()
		{
			if (_tapGestureRecognizers == null)
				return false;

			// Check if there are any tap recognizers with more than 2 taps required
			// We check a reasonable range but don't artificially limit functionality
			for (int i = 3; i <= 100; i++)
			{
				if (_tapGestureRecognizers(i).Any())
					return true;
			}
			return false;
		}

		bool HandleTapEvent(MotionEvent e)
		{
			if (e == null)
				return false;

			long currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
			float currentX = e.GetX();
			float currentY = e.GetY();

			// Check if this tap is part of a multi-tap sequence
			bool isSequentialTap = false;
			if (_lastTapTime > 0)
			{
				long timeDiff = currentTime - _lastTapTime;
				float distance = (float)Math.Sqrt(Math.Pow(currentX - _lastTapX, 2) + Math.Pow(currentY - _lastTapY, 2));

				if (timeDiff <= _maxTapInterval && distance <= _maxTapDistance)
				{
					isSequentialTap = true;
				}
			}

			if (isSequentialTap)
			{
				_currentTapCount++;
			}
			else
			{
				_currentTapCount = 1;
			}

			_lastTapTime = currentTime;
			_lastTapX = currentX;
			_lastTapY = currentY;

			// Check if we have a gesture recognizer for this tap count
			bool hasHandler = false;
			if (_tapGestureRecognizers != null)
			{
				hasHandler = _tapGestureRecognizers(_currentTapCount).Any();
			}

			if (hasHandler)
			{
				// Fire the gesture immediately if we have a handler for this tap count
				bool handled = _tapDelegate(_currentTapCount, e);
				if (handled)
				{
					// Reset the tap count after successful gesture
					_currentTapCount = 0;
					_lastTapTime = 0;
				}
				return handled;
			}

			// If we don't have a handler for the current tap count, 
			// continue waiting for more taps
			// We set a reasonable upper bound to prevent infinite counting
			if (_currentTapCount < 100)
			{
				return false; // Continue waiting for more taps
			}
			else
			{
				// Reset if we've exceeded a very high tap count
				_currentTapCount = 0;
				_lastTapTime = 0;
				return false;
			}
		}
	}
}