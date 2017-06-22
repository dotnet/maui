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
		bool _isScrolling;		
		float _lastX;
		float _lastY;
		bool _disposed;

		Func<bool> _scrollCompleteDelegate;
		Func<float, float, int, bool> _scrollDelegate;
		Func<int, bool> _scrollStartedDelegate;
		Func<int, bool> _tapDelegate;
		Func<int, IEnumerable<TapGestureRecognizer>> _tapGestureRecognizers;

		public InnerGestureListener(Func<int, bool> tapDelegate, Func<int, IEnumerable<TapGestureRecognizer>> tapGestureRecognizers, Func<float, float, int, bool> scrollDelegate,
									Func<int, bool> scrollStartedDelegate, Func<bool> scrollCompleteDelegate)
		{
			if (tapDelegate == null)
				throw new ArgumentNullException(nameof(tapDelegate));
			if (tapGestureRecognizers == null)
				throw new ArgumentNullException(nameof(tapGestureRecognizers));
			if (scrollDelegate == null)
				throw new ArgumentNullException(nameof(scrollDelegate));
			if (scrollStartedDelegate == null)
				throw new ArgumentNullException(nameof(scrollStartedDelegate));
			if (scrollCompleteDelegate == null)
				throw new ArgumentNullException(nameof(scrollCompleteDelegate));

			_tapDelegate = tapDelegate;
			_tapGestureRecognizers = tapGestureRecognizers;
			_scrollDelegate = scrollDelegate;
			_scrollStartedDelegate = scrollStartedDelegate;
			_scrollCompleteDelegate = scrollCompleteDelegate;
		}

		// This is needed because GestureRecognizer callbacks can be delayed several hundred milliseconds
		// which can result in the need to resurrect this object if it has already been disposed. We dispose
		// eagerly to allow easier garbage collection of the renderer
		internal InnerGestureListener(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
		}

		internal void OnTouchEvent(MotionEvent e)
		{
			if (e.Action == MotionEventActions.Up)
				EndScrolling();
			else if (e.Action == MotionEventActions.Down)
				SetStartingPosition(e);
			else if (e.Action == MotionEventActions.Move)
				StartScrolling(e);
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTap(MotionEvent e)
		{
			if (_disposed)
				return false;

			return _tapDelegate(2);
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTapEvent(MotionEvent e)
		{
			return false;
		}

		bool GestureDetector.IOnDoubleTapListener.OnSingleTapConfirmed(MotionEvent e)
		{
			if (_disposed)
				return false;

			// optimization: only wait for a second tap if there is a double tap handler
			if (!HasDoubleTapHandler())
				return false;

			return _tapDelegate(1);
		}

		bool GestureDetector.IOnGestureListener.OnDown(MotionEvent e)
		{
			SetStartingPosition(e);
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

			// optimization: do not wait for a second tap if there is no double tap handler
			if (HasDoubleTapHandler())
				return false;

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

		void EndScrolling()
		{
			if (_isScrolling && _scrollCompleteDelegate != null)
				_scrollCompleteDelegate();

			_isScrolling = false;
		}

		bool HasDoubleTapHandler()
		{
			return _tapGestureRecognizers(2).Any();
		}
	}
}