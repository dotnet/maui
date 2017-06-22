using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Android.Support.V4.View;
using Android.Views;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public class GestureManager : Object, global::Android.Views.View.IOnClickListener, global::Android.Views.View.IOnTouchListener
	{
		IVisualElementRenderer _renderer;
		readonly Lazy<GestureDetector> _gestureDetector;
		readonly PanGestureHandler _panGestureHandler;
		readonly PinchGestureHandler _pinchGestureHandler;
		readonly Lazy<ScaleGestureDetector> _scaleDetector;
		readonly TapGestureHandler _tapGestureHandler;
        readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();
        InnerGestureListener _gestureListener;

		bool _clickable;
		bool _disposed;
		bool _inputTransparent;
	    bool _isEnabled;

		NotifyCollectionChangedEventHandler _collectionChangeHandler;

		VisualElement Element => _renderer?.Element;

		View View => _renderer?.Element as View;

		global::Android.Views.View Control => _renderer?.View;

		public GestureManager(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
			_renderer.ElementChanged += OnElementChanged;

			_tapGestureHandler = new TapGestureHandler(() => View);
			_panGestureHandler = new PanGestureHandler(() => View, Control.Context.FromPixels);
			_pinchGestureHandler = new PinchGestureHandler(() => View);
			_gestureDetector =
				new Lazy<GestureDetector>(
					() =>
						new GestureDetector(
							_gestureListener =
								new InnerGestureListener(_tapGestureHandler.OnTap, _tapGestureHandler.TapGestureRecognizers,
									_panGestureHandler.OnPan, _panGestureHandler.OnPanStarted, _panGestureHandler.OnPanComplete)));

			_scaleDetector =
				new Lazy<ScaleGestureDetector>(
					() =>
						new ScaleGestureDetector(Control.Context,
							new InnerScaleListener(_pinchGestureHandler.OnPinch, _pinchGestureHandler.OnPinchStarted,
								_pinchGestureHandler.OnPinchEnded), Control.Handler));

			Control.SetOnClickListener(this);
			Control.SetOnTouchListener(this);
		}

		public bool OnTouchEvent(MotionEvent e, IViewParent parent, out bool handled)
		{
			if (_inputTransparent)
			{
				handled = true;
				return false;
			}

			if (View.GestureRecognizers.Count == 0)
			{
				handled = true;
				return _motionEventHelper.HandleMotionEvent(parent, e);
			}

			handled = false;
			return false;
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				UnsubscribeGestureRecognizers(e.OldElement);
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				UpdateGestureRecognizers(true);
				SubscribeGestureRecognizers(e.NewElement);
                _motionEventHelper.UpdateElement(e.NewElement);
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

		protected override void Dispose(bool disposing)
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
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				Control.SetOnClickListener(null);
				Control.SetOnTouchListener(null);

				if (_gestureListener != null)
				{
					_gestureListener.Dispose();
					_gestureListener = null;
				}

				if (_renderer?.Element != null)
				{
					UnsubscribeGestureRecognizers(Element);
				}

				_renderer = null;
			}

			base.Dispose(disposing);
		}

		void global::Android.Views.View.IOnClickListener.OnClick(global::Android.Views.View v)
		{
			_tapGestureHandler.OnSingleClick();
		}

		bool global::Android.Views.View.IOnTouchListener.OnTouch(global::Android.Views.View v, MotionEvent e)
		{
            if (!_isEnabled)
                return true;

            if (_inputTransparent)
                return false;

            var handled = false;
			if (_pinchGestureHandler.IsPinchSupported)
			{
				if (!_scaleDetector.IsValueCreated)
					ScaleGestureDetectorCompat.SetQuickScaleEnabled(_scaleDetector.Value, true);
				handled = _scaleDetector.Value.OnTouchEvent(e);
			}

			if (_gestureDetector.IsValueCreated && _gestureDetector.Value.Handle == IntPtr.Zero)
			{
				// This gesture detector has already been disposed, probably because it's on a cell which is going away
				return handled;
			}

			// It's very important that the gesture detection happen first here
			// if we check handled first, we might short-circuit and never check for tap/pan
			handled = _gestureDetector.Value.OnTouchEvent(e) || handled;

			v.EnsureLongClickCancellation(e, handled, Element);

			return handled;
		}

		void HandleGestureRecognizerCollectionChanged(object sender,
			NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdateGestureRecognizers();
		}

		void SubscribeGestureRecognizers(VisualElement element)
		{
			var view = element as View;
			if (view == null)
			{
				return;
			}

			if (_collectionChangeHandler == null)
			{
				_collectionChangeHandler = HandleGestureRecognizerCollectionChanged;
			}

			var observableCollection = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
			if (observableCollection != null)
			{
				observableCollection.CollectionChanged += _collectionChangeHandler;
			}
		}

		void UnsubscribeGestureRecognizers(VisualElement element)
		{
			var view = element as View;
			if (view == null || _collectionChangeHandler == null)
			{
				return;
			}

			var observableCollection = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
			if (observableCollection != null)
			{
				observableCollection.CollectionChanged -= _collectionChangeHandler;
			}
		}

		void UpdateClickable(bool force = false)
		{
			var view = Element as View;
			if (view == null)
			{
				return;
			}

			bool newValue = view.ShouldBeMadeClickable();
			if (force || _clickable != newValue)
			{
				Control.Clickable = newValue;
				_clickable = newValue;
			}
		}

		void UpdateGestureRecognizers(bool forceClick = false)
		{
			if (Element == null)
			{
				return;
			}

			UpdateClickable(forceClick);
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