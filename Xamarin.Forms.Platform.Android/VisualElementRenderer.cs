using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Android.Support.V4.View;
using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public abstract class VisualElementRenderer<TElement> : FormsViewGroup, IVisualElementRenderer, AView.IOnTouchListener, AView.IOnClickListener, IEffectControlProvider where TElement : VisualElement
	{
		readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers = new List<EventHandler<VisualElementChangedEventArgs>>();

		readonly Lazy<GestureDetector> _gestureDetector;
		readonly PanGestureHandler _panGestureHandler;
		readonly PinchGestureHandler _pinchGestureHandler;

		readonly TapGestureHandler _tapGestureHandler;

		NotifyCollectionChangedEventHandler _collectionChangeHandler;

		VisualElementRendererFlags _flags = VisualElementRendererFlags.AutoPackage | VisualElementRendererFlags.AutoTrack;

		InnerGestureListener _gestureListener;
		VisualElementPackager _packager;
		PropertyChangedEventHandler _propertyChangeHandler;
		Lazy<ScaleGestureDetector> _scaleDetector;

		protected VisualElementRenderer() : base(Forms.Context)
		{
			_tapGestureHandler = new TapGestureHandler(() => View);
			_panGestureHandler = new PanGestureHandler(() => View, Context.FromPixels);
			_pinchGestureHandler = new PinchGestureHandler(() => View);

			_gestureDetector =
				new Lazy<GestureDetector>(
					() =>
					new GestureDetector(
						_gestureListener =
						new InnerGestureListener(_tapGestureHandler.OnTap, _tapGestureHandler.TapGestureRecognizers, _panGestureHandler.OnPan, _panGestureHandler.OnPanStarted, _panGestureHandler.OnPanComplete)));

			_scaleDetector = new Lazy<ScaleGestureDetector>(
					() => new ScaleGestureDetector(Context, new InnerScaleListener(_pinchGestureHandler.OnPinch, _pinchGestureHandler.OnPinchStarted, _pinchGestureHandler.OnPinchEnded))
					);
		}

		public TElement Element { get; private set; }

		protected bool AutoPackage
		{
			get { return (_flags & VisualElementRendererFlags.AutoPackage) != 0; }
			set
			{
				if (value)
					_flags |= VisualElementRendererFlags.AutoPackage;
				else
					_flags &= ~VisualElementRendererFlags.AutoPackage;
			}
		}

		protected bool AutoTrack
		{
			get { return (_flags & VisualElementRendererFlags.AutoTrack) != 0; }
			set
			{
				if (value)
					_flags |= VisualElementRendererFlags.AutoTrack;
				else
					_flags &= ~VisualElementRendererFlags.AutoTrack;
			}
		}

		View View
		{
			get { return Element as View; }
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				OnRegisterEffect(platformEffect);
		}

		void IOnClickListener.OnClick(AView v)
		{
			_tapGestureHandler.OnSingleClick();
		}

		bool IOnTouchListener.OnTouch(AView v, MotionEvent e)
		{
			var handled = false;
			if (_pinchGestureHandler.IsPinchSupported)
			{
				if (!_scaleDetector.IsValueCreated)
					ScaleGestureDetectorCompat.SetQuickScaleEnabled(_scaleDetector.Value, true);
				handled = _scaleDetector.Value.OnTouchEvent(e);
			}
			return _gestureDetector.Value.OnTouchEvent(e) || handled;
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add { _elementChangedHandlers.Add(value); }
			remove { _elementChangedHandlers.Remove(value); }
		}

		public virtual SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), MinimumSize());
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (!(element is TElement))
				throw new ArgumentException("element is not of type " + typeof(TElement), "element");

			SetElement((TElement)element);
		}

		public VisualElementTracker Tracker { get; private set; }

		public void UpdateLayout()
		{
			Performance.Start();
			if (Tracker != null)
				Tracker.UpdateLayout();

			Performance.Stop();
		}

		public ViewGroup ViewGroup
		{
			get { return this; }
		}

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;

		public void SetElement(TElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			TElement oldElement = Element;
			Element = element;

			Performance.Start();

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= _propertyChangeHandler;
				UnsubscribeGestureRecognizers(oldElement);
			}

			// element may be allowed to be passed as null in the future
			if (element != null)
			{
				Color currentColor = oldElement != null ? oldElement.BackgroundColor : Color.Default;
				if (element.BackgroundColor != currentColor)
					UpdateBackgroundColor();
			}

			if (_propertyChangeHandler == null)
				_propertyChangeHandler = OnElementPropertyChanged;

			element.PropertyChanged += _propertyChangeHandler;
			SubscribeGestureRecognizers(element);

			if (oldElement == null)
			{
				SetOnClickListener(this);
				SetOnTouchListener(this);
				SoundEffectsEnabled = false;
			}

			InputTransparent = Element.InputTransparent;

			// must be updated AFTER SetOnClickListener is called
			// SetOnClickListener implicitly calls Clickable = true
			UpdateGestureRecognizers(true);

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, element));

			if (AutoPackage && _packager == null)
				SetPackager(new VisualElementPackager(this));

			if (AutoTrack && Tracker == null)
				SetTracker(new VisualElementTracker(this));

			if (element != null)
				SendVisualElementInitialized(element, this);

			var controller = (IElementController)oldElement;
			if (controller != null && controller.EffectControlProvider == this)
				controller.EffectControlProvider = null;

			controller = element;
			if (controller != null)
				controller.EffectControlProvider = this;

			if (element != null && !string.IsNullOrEmpty(element.AutomationId))
				SetAutomationId(element.AutomationId);

			Performance.Stop();
		}

		/// <summary>
		/// Determines whether the native control is disposed of when this renderer is disposed
		/// Can be overridden in deriving classes 
		/// </summary>
		protected virtual bool ManageNativeControlLifetime => true;

		protected override void Dispose(bool disposing)
		{
			if ((_flags & VisualElementRendererFlags.Disposed) != 0)
				return;
			_flags |= VisualElementRendererFlags.Disposed;

			if (disposing)
			{
				if (Tracker != null)
				{
					Tracker.Dispose();
					Tracker = null;
				}

				if (_packager != null)
				{
					_packager.Dispose();
					_packager = null;
				}

				if (_scaleDetector != null && _scaleDetector.IsValueCreated)
				{
					_scaleDetector.Value.Dispose();
					_scaleDetector = null;
				}

				if (_gestureListener != null)
				{
					_gestureListener.Dispose();
					_gestureListener = null;
				}

				if (ManageNativeControlLifetime)
				{
					int count = ChildCount;
					for (var i = 0; i < count; i++)
					{
						AView child = GetChildAt(i);
						child.Dispose();
					}
				}

				RemoveAllViews();

				if (Element != null)
				{
					Element.PropertyChanged -= _propertyChangeHandler;
					UnsubscribeGestureRecognizers(Element);

					if (Platform.GetRenderer(Element) == this)
						Platform.SetRenderer(Element, null);

					Element = null;
				}
			}

			base.Dispose(disposing);
		}

		protected virtual Size MinimumSize()
		{
			return new Size();
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);
			for (var i = 0; i < _elementChangedHandlers.Count; i++)
				_elementChangedHandlers[i](this, args);

			EventHandler<ElementChangedEventArgs<TElement>> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				InputTransparent = Element.InputTransparent;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Element == null)
				return;

			ReadOnlyCollection<Element> children = ((IElementController)Element).LogicalChildren;
			for (var i = 0; i < children.Count; i++)
			{
				var visualElement = children[i] as VisualElement;
				if (visualElement == null)
					continue;

				IVisualElementRenderer renderer = Platform.GetRenderer(visualElement);
				renderer?.UpdateLayout();
			}
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.Container = this;
		}

		protected virtual void SetAutomationId(string id)
		{
			ContentDescription = id;
		}

		protected void SetPackager(VisualElementPackager packager)
		{
			_packager = packager;
			packager.Load();
		}

		protected void SetTracker(VisualElementTracker tracker)
		{
			Tracker = tracker;
		}

		protected virtual void UpdateBackgroundColor()
		{
			SetBackgroundColor(Element.BackgroundColor.ToAndroid());
		}

		internal virtual void SendVisualElementInitialized(VisualElement element, AView nativeView)
		{
			element.SendViewInitialized(nativeView);
		}

		void HandleGestureRecognizerCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdateGestureRecognizers();
		}

		void SubscribeGestureRecognizers(VisualElement element)
		{
			var view = element as View;
			if (view == null)
				return;

			if (_collectionChangeHandler == null)
				_collectionChangeHandler = HandleGestureRecognizerCollectionChanged;

			var observableCollection = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
			observableCollection.CollectionChanged += _collectionChangeHandler;
		}

		void UnsubscribeGestureRecognizers(VisualElement element)
		{
			var view = element as View;
			if (view == null || _collectionChangeHandler == null)
				return;

			var observableCollection = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
			observableCollection.CollectionChanged -= _collectionChangeHandler;
		}

		void UpdateClickable(bool force = false)
		{
			var view = Element as View;
			if (view == null)
				return;

			bool newValue = view.ShouldBeMadeClickable();
			if (force || newValue)
				Clickable = newValue;
		}

		void UpdateGestureRecognizers(bool forceClick = false)
		{
			if (View == null)
				return;

			UpdateClickable(forceClick);
		}
	}
}