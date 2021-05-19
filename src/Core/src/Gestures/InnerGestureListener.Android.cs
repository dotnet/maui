using System;
using System.Collections.Generic;
using System.Linq;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Graphics;
using Object = Java.Lang.Object;

namespace Microsoft.Maui
{
	public class InnerGestureListener : Object, GestureDetector.IOnGestureListener, GestureDetector.IOnDoubleTapListener
	{
		TapGestureHandler? _tapGestureHandler;
		bool _disposed;

		Func<int, Point, bool>? _tapDelegate;
		Func<int, IEnumerable<ITapGestureRecognizer>>? _tapGestureRecognizers;

		public InnerGestureListener(TapGestureHandler tapGestureHandler)
		{
			_ = tapGestureHandler ?? throw new ArgumentNullException(nameof(tapGestureHandler));

			_tapGestureHandler = tapGestureHandler;

			_tapDelegate = tapGestureHandler.OnTap;
			_tapGestureRecognizers = tapGestureHandler.TapGestureRecognizers;
		}

		bool HasAnyGestures() =>
			_tapGestureHandler?.HasAnyGestures() ?? false;

		// This is needed because GestureRecognizer callbacks can be delayed several hundred milliseconds
		// which can result in the need to resurrect this object if it has already been disposed. We dispose
		// eagerly to allow easier garbage collection of the renderer
		internal InnerGestureListener(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTap(MotionEvent? e)
		{
			if (_disposed)
				return false;

			if (HasDoubleTapHandler())
			{
				if (_tapDelegate != null && e != null)
					return _tapDelegate(2, new Point(e.GetX(), e.GetY()));

				return false;
			}

			if (HasSingleTapHandler())
			{
				// If we're registering double taps and we don't actually have a double-tap handler,
				// but we _do_ have a single-tap handler, then we're really just seeing two singles in a row
				// Fire off the delegate for the second single-tap (OnSingleTapUp already did the first one)
				if (_tapDelegate != null && e != null)
					return _tapDelegate(1, new Point(e.GetX(), e.GetY()));

				return false;
			}

			return false;
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTapEvent(MotionEvent? e)
		{
			return false;
		}

		bool GestureDetector.IOnGestureListener.OnDown(MotionEvent? e)
		{
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

		bool GestureDetector.IOnGestureListener.OnFling(MotionEvent? e1, MotionEvent? e2, float velocityX, float velocityY)
		{
			return false;
		}

		void GestureDetector.IOnGestureListener.OnLongPress(MotionEvent? e)
		{

		}

		bool GestureDetector.IOnGestureListener.OnScroll(MotionEvent? e1, MotionEvent? e2, float distanceX, float distanceY)
		{
			return false;
		}

		void GestureDetector.IOnGestureListener.OnShowPress(MotionEvent? e)
		{
		}

		bool GestureDetector.IOnGestureListener.OnSingleTapUp(MotionEvent? e)
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
			if (_tapDelegate != null && e != null)
				return _tapDelegate(1, new Point(e.GetX(), e.GetY()));

			return false;
		}

		bool GestureDetector.IOnDoubleTapListener.OnSingleTapConfirmed(MotionEvent? e)
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
			if (_tapDelegate != null && e != null)
				return _tapDelegate(1, new Point(e.GetX(), e.GetY()));

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
				_tapGestureHandler = null;

				_tapDelegate = null;
				_tapGestureRecognizers = null;
			}

			base.Dispose(disposing);
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