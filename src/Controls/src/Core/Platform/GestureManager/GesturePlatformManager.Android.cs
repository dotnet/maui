using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using static Android.Views.View;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	class GesturePlatformManager : IDisposable
	{
		IViewHandler? _handler;
		Lazy<ScaleGestureDetector> _scaleDetector;
		Lazy<TapAndPanGestureDetector> _tapAndPanAndSwipeDetector;
		Lazy<DragAndDropGestureHandler> _dragAndDropGestureHandler;
		Lazy<PointerGestureHandler> _pointerGestureHandler;
		bool _disposed;
		bool _inputTransparent;
		bool _isEnabled;
		bool? _focusableDefaultValue;
		protected virtual VisualElement? Element => _handler?.VirtualView as VisualElement;

		View? View => Element as View;
		WeakReference<AView>? _control;

		public GesturePlatformManager(IViewHandler handler)
		{
			_handler = handler;
			_control = new WeakReference<AView>(_handler.ToPlatform());
			_tapAndPanAndSwipeDetector = new Lazy<TapAndPanGestureDetector>(InitializeTapAndPanAndSwipeDetector);
			_scaleDetector = new Lazy<ScaleGestureDetector>(InitializeScaleDetector);
			_dragAndDropGestureHandler = new Lazy<DragAndDropGestureHandler>(InitializeDragAndDropHandler);
			_pointerGestureHandler = new Lazy<PointerGestureHandler>(InitializePointerHandler);
			SetupElement(null, Element);
		}

		protected virtual AView? Control
		{
			get
			{
				if (_control?.TryGetTarget(out var target) == true && target.IsAlive())
					return target;

				return null;
			}
			set
			{
				if (value != null)
					_control = new WeakReference<AView>(value);
				else
					_control = null;
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

		PointerGestureHandler InitializePointerHandler()
		{
			return new PointerGestureHandler(() => View, () => Control);
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
				new PanGestureHandler(() => View),
				new SwipeGestureHandler(() => View),
				InitializeDragAndDropHandler(),
				InitializePointerHandler()
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
#pragma warning disable CS0618 // Type or member is obsolete
			ScaleGestureDetectorCompat.SetQuickScaleEnabled(detector, true);
#pragma warning restore CS0618 // Type or member is obsolete

			return detector;
		}

		bool ViewHasPinchGestures()
		{
			if (View is null)
				return false;

			int count = View.GestureRecognizers.Count;
			for (var i = 0; i < count; i++)
			{
				if (View.GestureRecognizers[i] is PinchGestureRecognizer)
					return true;
			}

			return false;
		}

		void SetupGestures()
		{
			if (View == null)
				return;

			var platformView = Control;

			if (platformView == null)
				return;

			bool shouldAddTouchEvent = false;

			// This change is probably not 100 percent correct.
			// The main purpose right now is to maintain the behavior of this code
			// prior to this change
			// https://github.com/dotnet/maui/commit/2c301d7988a06c3b41c2992bbee557aca04c9388#diff-2d78f02242798d0f2863f679e4dfdee230944be37db5e1a1446bfa4c6c43a5c6R183
			// If the only CompositeGestureRecognizers is a PointerGestureRecognizer
			//
			// Most likely we should just not subscribe to Touch at all if the only gesture is a PGR
			// But that will be re-evaluated for preview6
			if (View.GestureRecognizers.Count == 0)
			{
				var recognizers = View.GestureController.CompositeGestureRecognizers;
				foreach (var recognizer in recognizers)
				{
					if (recognizer is not PointerGestureRecognizer)
					{
						shouldAddTouchEvent = true;
						break;
					}
				}
			}
			else
			{
				shouldAddTouchEvent = true;
			}

			// Always unsubscribe first to avoid duplicates

			platformView.Touch -= OnPlatformViewTouched;
			platformView.KeyPress -= OnKeyPress;


			if (shouldAddTouchEvent)
			{
				platformView.Touch += OnPlatformViewTouched;

				// If we have a TapGestureRecognizer, we need to handle key presses
				if (View.HasAccessibleTapGesture())
				{
					platformView.KeyPress += OnKeyPress;
					_focusableDefaultValue ??= platformView.Focusable;
					platformView.Focusable = true;
				}
			}
			else
			{
				_focusableDefaultValue = null;
			}
		}

		void OnKeyPress(object? sender, KeyEventArgs e)
		{
			if (e.Event?.Action != KeyEventActions.Up)
			{
				e.Handled = false;
				return;
			}

			if (View is null || sender is not AView platformView)
			{
				e.Handled = false;
				return;
			}

			if (e.KeyCode.IsConfirmKey() &&
				View.HasAccessibleTapGesture(out var tapGestureRecognizer) &&
				e.Event.HasNoModifiers)
			{
				if (!platformView.Enabled)
				{
					e.Handled = true;
					return;
				}

				if (!e.Event.IsCanceled)
					tapGestureRecognizer.SendTapped(View, (v) => Point.Zero);
			}

			e.Handled = false;
		}

		void OnPlatformViewTouched(object? sender, AView.TouchEventArgs e)
		{
			if (_disposed)
			{
				var platformView = Control;
				if (platformView != null)
					platformView.Touch -= OnPlatformViewTouched;

				return;
			}

			if (e.Event != null)
				OnTouchEvent(e.Event);
		}

		void SetupElement(VisualElement? oldElement, VisualElement? newElement)
		{
			var platformView = Control;
			if (platformView is not null)
			{
				platformView.Focusable = _focusableDefaultValue ?? platformView.Focusable;
				_focusableDefaultValue = null;
				platformView.Touch -= OnPlatformViewTouched;
				platformView.KeyPress -= OnKeyPress;
			}

			_handler = null;
			if (oldElement != null)
			{
				if (oldElement is View ov &&
					ov.GetCompositeGestureRecognizers() is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= GestureCollectionChanged;
				}

				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (newElement != null)
			{
				_handler = newElement.Handler;
				if (newElement is View ov &&
					ov.GetCompositeGestureRecognizers() is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged += GestureCollectionChanged;
				}

				newElement.PropertyChanged += OnElementPropertyChanged;
			}

			UpdateInputTransparent();
			UpdateIsEnabled();
			UpdateDragAndDrop();
			UpdatePointer();
			SetupGestures();
		}

		void GestureCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateDragAndDrop();
			UpdatePointer();
			SetupGestures();

			if (_tapAndPanAndSwipeDetector.IsValueCreated)
				_tapAndPanAndSwipeDetector.Value.UpdateLongPressSettings();

			View?.AddOrRemoveControlsAccessibilityDelegate();
		}

		void UpdateDragAndDrop()
		{
			if (View?.GetCompositeGestureRecognizers()?.Count > 0)
				_dragAndDropGestureHandler.Value.SetupHandlerForDrop();
		}

		void UpdatePointer()
		{
			if (View?.GetCompositeGestureRecognizers()?.Count > 0)
				_pointerGestureHandler.Value.SetupHandlerForPointer();
		}

		void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
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
				SetupElement(Element, null);

				if (_tapAndPanAndSwipeDetector.IsValueCreated)
				{
					_tapAndPanAndSwipeDetector.Value.Dispose();
				}

				if (_scaleDetector.IsValueCreated)
				{
					_scaleDetector.Value.Dispose();
				}

				_control = null;
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
