using System;
using System.Collections.Generic;
using System.Linq;
using Android.Runtime;
using Android.Views;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	internal class InnerGestureListener : Object, GestureDetector.IOnGestureListener, GestureDetector.IOnDoubleTapListener
	{
		readonly TapGestureHandler _tapGestureHandler;
		readonly PanGestureHandler _panGestureHandler;
		bool _isScrolling;		
		float _lastX;
		float _lastY;
		bool _disposed;

		Func<bool> _scrollCompleteDelegate;
		Func<float, float, int, bool> _scrollDelegate;
		Func<int, bool> _scrollStartedDelegate;
		Func<int, bool> _tapDelegate;
		Func<int, IEnumerable<TapGestureRecognizer>> _tapGestureRecognizers;

		public InnerGestureListener(TapGestureHandler tapGestureHandler, PanGestureHandler panGestureHandler)
		{
			if (tapGestureHandler == null)
			{
				throw new ArgumentNullException(nameof(tapGestureHandler));
			}

			if (panGestureHandler == null)
			{
				throw new ArgumentNullException(nameof(panGestureHandler));
			}

			_tapGestureHandler = tapGestureHandler;
			_panGestureHandler = panGestureHandler;

			_tapDelegate = tapGestureHandler.OnTap;
			_tapGestureRecognizers = tapGestureHandler.TapGestureRecognizers;
			_scrollDelegate = panGestureHandler.OnPan;
			_scrollStartedDelegate = panGestureHandler.OnPanStarted;
			_scrollCompleteDelegate = panGestureHandler.OnPanComplete;
		}

		bool HasAnyGestures()
		{
			return _panGestureHandler.HasAnyGestures() || _tapGestureHandler.HasAnyGestures();
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
				return _tapDelegate(2);
			}

			if (HasSingleTapHandler())
			{
				// If we're registering double taps and we don't actually have a double-tap handler,
				// but we _do_ have a single-tap handler, then we're really just seeing two singles in a row
				// Fire off the delegate for the second single-tap (OnSingleTapUp already did the first one)
				return _tapDelegate(1);
			}

			return false;
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTapEvent(MotionEvent e)
		{
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
				// Because we have a handler for double-tap, we need to wait for
				// OnSingleTapConfirmed (to verify it's really just a single tap) before running the delegate
				return false;
			}

			// A single tap has occurred and there's no handler for double tap to worry about,
			// so we can go ahead and run the delegate
			return _tapDelegate(1);
		}

		bool GestureDetector.IOnDoubleTapListener.OnSingleTapConfirmed(MotionEvent e)
		{
			if (_disposed)
				return false;

			if (!HasDoubleTapHandler())
			{
				// We're not worried about double-tap, so OnSingleTapUp has already run the delegate
				// there's nothing for us to do here
				return false;
			}

			// Since there was a double-tap handler, we had to wait for OnSingleTapConfirmed;
			// Now that we're sure it's a single tap, we can run the delegate
			return _tapDelegate(1);
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
				_tapDelegate = null;
				_tapGestureRecognizers = null;
				_scrollDelegate = null;
				_scrollStartedDelegate = null;
				_scrollCompleteDelegate = null;
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

			return _scrollDelegate(totalX, totalY, e2.PointerCount);
		}

		internal void EndScrolling()
		{
			if (_isScrolling && _scrollCompleteDelegate != null)
				_scrollCompleteDelegate();

			_isScrolling = false;
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