using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class GestureManager : IGestureManager
	{
		readonly Lazy<TapGestureDetector> _tapDetector;

		IViewHandler? _handler;
		bool _isDisposed;
		IView? _virtualView;
		AView? _nativeView;

		public GestureManager()
		{
			_tapDetector = new Lazy<TapGestureDetector>(InitializeTapDetector);
		}

		public void SetViewHandler(IViewHandler handler)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(null);

			_handler = handler ?? throw new ArgumentNullException(nameof(handler));

			_virtualView = _handler.VirtualView as IView;
			_nativeView = _handler.NativeView as AView;
		}

		public bool OnTouchEvent(MotionEvent? e)
		{
			if (_nativeView == null)
			{
				return false;
			}

			if (_virtualView != null && !_virtualView.IsEnabled)
			{
				return false;
			}

			if (!DetectorsValid())
			{
				return false;
			}

			var eventConsumed = _tapDetector.Value.OnTouchEvent(e);

			return eventConsumed;
		}
				
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;

			if (disposing)
			{
				if (_tapDetector.IsValueCreated)
				{
					_tapDetector.Value.Dispose();
				}

				_handler = null;
			}
		}

		bool DetectorsValid()
		{
			// Make sure we're not testing for gestures on old motion events after our 
			// detectors have already been disposed

			if (_tapDetector.IsValueCreated && _tapDetector.Value.Handle == IntPtr.Zero)
			{
				return false;
			}

			return true;
		}

		TapGestureDetector InitializeTapDetector()
		{
			var context = _nativeView?.Context;

			var listener = new InnerGestureListener(new TapGestureHandler(() => _virtualView!, () =>
			{
				if (_virtualView is IView view)
					return view.GetChildElements(Point.Zero) ?? new List<IGestureView>();

				return new List<IGestureView>();
			}));

			return new TapGestureDetector(context, listener);
		}

		public class TapGestureDetector : GestureDetector
		{
			InnerGestureListener? _listener;

			public TapGestureDetector(Context? context, InnerGestureListener listener) : base(context, listener)
			{
				_listener = listener;
			}

			public override bool OnTouchEvent(MotionEvent? ev)
			{
				if (base.OnTouchEvent(ev))
					return true;

				return false;
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);

				if (disposing)
				{
					if (_listener != null)
					{
						_listener.Dispose();
						_listener = null;
					}
				}
			}
		}
	}
}