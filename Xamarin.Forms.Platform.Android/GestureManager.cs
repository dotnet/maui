using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.Content;
#if __ANDROID_29__
using AndroidX.Core.View;
#else
using Android.Support.V4.View;
#endif
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class GestureManager : IDisposable
	{
		IVisualElementRenderer _renderer;
		readonly Lazy<ScaleGestureDetector> _scaleDetector;
		readonly Lazy<GestureDetector> _tapAndPanAndSwipeDetector;

		bool _disposed;
		bool _inputTransparent;
		bool _isEnabled;

		VisualElement Element => _renderer?.Element;

		View View => _renderer?.Element as View;

		global::Android.Views.View Control => _renderer?.View;

		public GestureManager(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
			_renderer.ElementChanged += OnElementChanged;

			_tapAndPanAndSwipeDetector = new Lazy<GestureDetector>(InitializeTapAndPanAndSwipeDetector);
			_scaleDetector = new Lazy<ScaleGestureDetector>(InitializeScaleDetector);
		}

		public bool OnTouchEvent(MotionEvent e)
		{
			if (Control == null)
			{
				return false;
			}

			if (!_isEnabled || _inputTransparent)
			{
				return false;
			}

			if (!DetectorsValid())
			{
				return false;
			}

			var eventConsumed = false;
			if (ViewHasPinchGestures())
			{
				eventConsumed = _scaleDetector.Value.OnTouchEvent(e);
			}

			if (!ViewHasPinchGestures() || !_scaleDetector.Value.IsInProgress)
				eventConsumed = _tapAndPanAndSwipeDetector.Value.OnTouchEvent(e) || eventConsumed;

			return eventConsumed;
		}

		public class TapAndPanGestureDetector : GestureDetector
		{
			InnerGestureListener _listener;
			public TapAndPanGestureDetector(Context context, InnerGestureListener listener) : base(context, listener)
			{
				_listener = listener;
				InitializeLongPressSettings();
			}

			void InitializeLongPressSettings()
			{
				// Right now this just disables long press, since we don't support a long press gesture
				// in Forms. If we ever do, we'll need to selectively enable it, probably by hooking into the 
				// InnerGestureListener and listening for the addition of any long press gesture recognizers.
				// (since a long press will prevent a pan gesture from starting, we can't just leave support for it 
				// on by default).
				// Also, since the property is virtual we shouldn't just set it from the constructor.

				IsLongpressEnabled = false;
			}

			public override bool OnTouchEvent(MotionEvent ev)
			{
				if (base.OnTouchEvent(ev))
					return true;

				if (ev.Action == MotionEventActions.Up)
					_listener.EndScrolling();

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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool DetectorsValid()
		{
			// Make sure we're not testing for gestures on old motion events after our 
			// detectors have already been disposed

			if (_scaleDetector.IsValueCreated && _scaleDetector.Value.Handle == IntPtr.Zero)
			{
				return false;
			}

			if (_tapAndPanAndSwipeDetector.IsValueCreated && _tapAndPanAndSwipeDetector.Value.Handle == IntPtr.Zero)
			{
				return false;
			}

			return true;
		}

		GestureDetector InitializeTapAndPanAndSwipeDetector()
		{
			var context = Control.Context;
			var listener = new InnerGestureListener(new TapGestureHandler(() => View, () =>
			{
				if (Element is View view)
					return view.GetChildElements(Point.Zero) ?? new List<GestureElement>();

				return new List<GestureElement>();
			}),
				new PanGestureHandler(() => View, context.FromPixels),
			    	new SwipeGestureHandler(() => View, context.FromPixels));

			return new TapAndPanGestureDetector(context, listener);
		}

		ScaleGestureDetector InitializeScaleDetector()
		{
			var context = Control.Context;
			var listener = new InnerScaleListener(new PinchGestureHandler(() => View), context.FromPixels);
			var detector = new ScaleGestureDetector(context, listener, Control.Handler);
			ScaleGestureDetectorCompat.SetQuickScaleEnabled(detector, true);

			return detector;
		}

		bool ViewHasPinchGestures()
		{
			return View != null && View.GestureRecognizers.OfType<PinchGestureRecognizer>().Any();
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				e.NewElement.PropertyChanged += OnElementPropertyChanged;
			}

			UpdateInputTransparent();
			UpdateIsEnabled();
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
		}

		protected void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				_renderer.ElementChanged -= OnElementChanged;

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				if (_tapAndPanAndSwipeDetector.IsValueCreated)
				{
					_tapAndPanAndSwipeDetector.Value.Dispose();
				}

				if (_scaleDetector.IsValueCreated)
				{
					_scaleDetector.Value.Dispose();
				}

				_renderer = null;
			}
		}

		void UpdateInputTransparent()
		{
			if (Element == null)
			{
				return;
			}

			_inputTransparent = Element.InputTransparent;
		}

		void UpdateIsEnabled()
		{
			if (Element == null)
			{
				return;
			}

			_isEnabled = Element.IsEnabled;
		}
	}
}
