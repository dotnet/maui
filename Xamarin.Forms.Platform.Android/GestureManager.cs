using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class GestureManager : IDisposable
	{
		IVisualElementRenderer _renderer;
		readonly Lazy<ScaleGestureDetector> _scaleDetector;
		readonly Lazy<TapAndPanGestureDetector> _tapAndPanAndSwipeDetector;
		readonly Lazy<DragAndDropGestureHandler> _dragAndDropGestureHandler;
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

			_tapAndPanAndSwipeDetector = new Lazy<TapAndPanGestureDetector>(InitializeTapAndPanAndSwipeDetector);
			_scaleDetector = new Lazy<ScaleGestureDetector>(InitializeScaleDetector);
			_dragAndDropGestureHandler = new Lazy<DragAndDropGestureHandler>(InitializeDragAndDropHandler);
			UpdateDragAndDrop();

			if (_renderer.Element is View ov &&
				ov.GestureRecognizers is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += GestureCollectionChanged;
			}
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
				UpdateLongPressSettings();
			}

			public void UpdateLongPressSettings()
			{
				// Right now this just disables long press, since we don't support a long press gesture
				// in Forms. If we ever do, we'll need to selectively enable it, probably by hooking into the 
				// InnerGestureListener and listening for the addition of any long press gesture recognizers.
				// (since a long press will prevent a pan gesture from starting, we can't just leave support for it 
				// on by default).
				// Also, since the property is virtual we shouldn't just set it from the constructor.

				IsLongpressEnabled = _listener.EnableLongPressGestures;
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

		DragAndDropGestureHandler InitializeDragAndDropHandler()
		{
			return new DragAndDropGestureHandler(() => View, () => Control);
		}

		TapAndPanGestureDetector InitializeTapAndPanAndSwipeDetector()
		{
			var context = Control.Context;
			var listener = new InnerGestureListener(
				new TapGestureHandler(() => View, () =>
				{
					if (Element is View view)
						return view.GetChildElements(Point.Zero) ?? new List<GestureElement>();

					return new List<GestureElement>();
				}),
				new PanGestureHandler(() => View, context.FromPixels),
				new SwipeGestureHandler(() => View, context.FromPixels),
				InitializeDragAndDropHandler()
			);

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

				if (e.OldElement is View ov &&
					ov.GestureRecognizers is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= GestureCollectionChanged;
				}
			}

			if (e.NewElement != null)
			{
				e.NewElement.PropertyChanged += OnElementPropertyChanged;

				if (e.NewElement is View ov &&
					ov.GestureRecognizers is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged += GestureCollectionChanged;
				}
			}

			UpdateInputTransparent();
			UpdateIsEnabled();
			UpdateDragAndDrop();
		}

		void GestureCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateDragAndDrop();

			if (_tapAndPanAndSwipeDetector.IsValueCreated)
				_tapAndPanAndSwipeDetector.Value.UpdateLongPressSettings();
		}

		void UpdateDragAndDrop()
		{
			if (View?.GestureRecognizers?.Count > 0)
				_dragAndDropGestureHandler.Value.SetupHandlerForDrop();
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

					if (Element is View ov &&
						ov.GestureRecognizers is INotifyCollectionChanged incc)
					{
						incc.CollectionChanged -= GestureCollectionChanged;
					}
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