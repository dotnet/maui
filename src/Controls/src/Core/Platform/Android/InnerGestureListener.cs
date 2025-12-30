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
		bool _singleTapFiredInSequence;

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

			if (HasDoubleTapHandler())
			{
				// Reset the flag since this is completing a double tap sequence
				_singleTapFiredInSequence = false;
				
				// Fire only the double tap handler here Single tap was already
				// fired earlier in OnSingleTapUp for better timing
				return _tapDelegate(2, e);
			}

			// If we're getting here and don't have a double-tap handler, we might be looking at multiple
			// single taps; that'll be handled in OnDoubleTapEvent

			return false;
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTapEvent(MotionEvent e)
		{
			if (!HasDoubleTapHandler() && HasSingleTapHandler() && e.Action == MotionEventActions.Up)
			{
				// If we're registering double taps and we don't actually have a double-tap handler,
				// but we _do_ have a single-tap handler, then we're really just seeing two singles in a row

				// Fire off the delegate for the second single-tap (OnSingleTapUp already did the first one)
				return _tapDelegate(1, e);
			}

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

			if (HasDoubleTapHandler())
			{
				// Fire single tap immediately for the first tap, even when we
				// have double tap handlers (mimics Windows timing)
				if (HasSingleTapHandler())
				{
					_tapDelegate(1, e);
					_singleTapFiredInSequence = true; // Track that we fired single tap
				}
				
				// Still return false to continue waiting for potential double tap
				return false;
			}

			// A single tap has occurred and there's no handler for double tap to worry about,
			// so we can go ahead and run the delegate
			return _tapDelegate(1, e);
		}

		bool GestureDetector.IOnDoubleTapListener.OnSingleTapConfirmed(MotionEvent e)
		{
			if (_disposed)
				return false;

			// The secondary button state only surfaces inside `OnSingleTapConfirmed`
			// Inside 'OnSingleTap' the e.ButtonState doesn't indicate a secondary click
			// So, if the gesture recognizer here has primary/secondary we want to ignore
			// the _tapDelegate call that accounts for secondary clicks
			if (!HasDoubleTapHandler() &&
				(!e.IsSecondary() || HasSingleTapHandlerWithPrimaryAndSecondary()))
			{
				// We're not worried about double-tap, so OnSingleTapUp has already run the delegate
				// there's nothing for us to do here
				return false;
			}

			// Check if we already fired the single tap in OnSingleTapUp during a potential double tap sequence
			if (_singleTapFiredInSequence)
			{
				_singleTapFiredInSequence = false; // Reset the flag
				return false; // Don't fire again
			}

			// Since there was a double-tap handler, we had to wait for OnSingleTapConfirmed;
			// This is called only for confirmed single taps (not part of double tap sequences)
			return _tapDelegate(1, e);
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
	}
}