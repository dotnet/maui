#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class GestureManager : IDisposable
	{
		IViewHandler? _handler;
		Lazy<ScaleGestureDetector> _scaleDetector;
		Lazy<TapAndPanGestureDetector> _tapAndPanAndSwipeDetector;
		Lazy<DragAndDropGestureHandler> _dragAndDropGestureHandler;
		bool _disposed;
		bool _inputTransparent;
		bool _isEnabled;

		protected virtual VisualElement? Element => _handler?.VirtualView as VisualElement;

		View? View => Element as View;

		protected virtual AView? Control => _handler?.NativeView as AView;

		public GestureManager(IViewHandler handler)
		{
			_handler = handler;
			_tapAndPanAndSwipeDetector = new Lazy<TapAndPanGestureDetector>(InitializeTapAndPanAndSwipeDetector);
			_scaleDetector = new Lazy<ScaleGestureDetector>(InitializeScaleDetector);
			_dragAndDropGestureHandler = new Lazy<DragAndDropGestureHandler>(InitializeDragAndDropHandler);
			UpdateDragAndDrop();

			if (Element is View ov &&
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
			InnerGestureListener? _listener;
			public TapAndPanGestureDetector(Context context, InnerGestureListener listener) : base(context, listener)
			{
				_listener = listener;
				UpdateLongPressSettings();
			}

			public void UpdateLongPressSettings()
			{
				if (_listener == null)
					return;

				// Right now this just disables long press, since we don't support a long press gesture
				// in Forms. If we ever do, we'll need to selectively enable it, probably by hooking into the 
				// InnerGestureListener and listening for the addition of any long press gesture recognizers.
				// (since a long press will prevent a pan gesture from starting, we can't just leave support for it 
				// on by default).
				// Also, since the property is virtual we shouldn't just set it from the constructor.

				IsLongpressEnabled = _listener.EnableLongPressGestures;
			}

			public override bool OnTouchEvent(MotionEvent? ev)
			{
				if (base.OnTouchEvent(ev))
					return true;

				if (_listener != null && ev?.Action == MotionEventActions.Up)
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
			if (Control?.Context == null)
				throw new InvalidOperationException("Context cannot be null here");

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
			if (Control?.Context == null)
				throw new InvalidOperationException("Context cannot be null here");

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

		internal void OnElementChanged(VisualElementChangedEventArgs e)
		{
			_handler = null;

			if (e.OldElement != null)
			{
				if (e.OldElement is View ov &&
					ov.GestureRecognizers is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= GestureCollectionChanged;
				}
			}

			if (e.NewElement != null)
			{
				_handler = e.NewElement.Handler;
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

		void GestureCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

		internal void OnElementPropertyChanged(PropertyChangedEventArgs e)
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
				if (Element != null)
				{
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

				_handler = null;
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
