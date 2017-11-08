using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public abstract class VisualElementRenderer<TElement> : FormsViewGroup, IVisualElementRenderer, 
		IEffectControlProvider where TElement : VisualElement
	{
		readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers = new List<EventHandler<VisualElementChangedEventArgs>>();

		VisualElementRendererFlags _flags = VisualElementRendererFlags.AutoPackage | VisualElementRendererFlags.AutoTrack;

		string _defaultContentDescription;
		bool? _defaultFocusable;
		string _defaultHint;
		int? _defaultLabelFor;
		
		VisualElementPackager _packager;
		PropertyChangedEventHandler _propertyChangeHandler;

		readonly GestureManager _gestureManager;

		protected VisualElementRenderer(Context context) : base(context)
		{
			_gestureManager = new GestureManager(this);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			return _gestureManager.OnTouchEvent(e) || base.OnTouchEvent(e);
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (!Enabled)
			{
				// If Enabled is false, prevent all the events from being dispatched to child Views
				// and prevent them from being processed by this View as well
				return true; // IOW, intercepted
			}

			return base.OnInterceptTouchEvent(ev);
		}

		public override bool DispatchTouchEvent(MotionEvent e)
		{
			if (InputTransparent)
			{
				// If the Element is InputTransparent, this ViewGroup will be marked InputTransparent
				// If we're InputTransparent we should return false on all touch events without
				// even bothering to send them to the child Views

				return false; // IOW, not handled
			}

			return base.DispatchTouchEvent(e);
		}		  		

		[Obsolete("This constructor is obsolete as of version 2.5. Please use VisualElementRenderer(Context) instead.")]
		protected VisualElementRenderer() : this(Forms.Context)
		{
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

		View View => Element as View;

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				OnRegisterEffect(platformEffect);
		}

		VisualElement IVisualElementRenderer.Element => Element;

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
				throw new ArgumentException("element is not of type " + typeof(TElement), nameof(element));

			SetElement((TElement)element);
		}

		public VisualElementTracker Tracker { get; private set; }

		public void UpdateLayout()
		{
			Performance.Start();
			Tracker?.UpdateLayout();
			Performance.Stop();
		}

		public ViewGroup ViewGroup => this;
		AView IVisualElementRenderer.View => this;

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public void SetElement(TElement element)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			TElement oldElement = Element;
			Element = element;

			Performance.Start();

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= _propertyChangeHandler;
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

			if (oldElement == null)
			{
				SoundEffectsEnabled = false;
			}

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, element));

			if (AutoPackage && _packager == null)
				SetPackager(new VisualElementPackager(this));

			if (AutoTrack && Tracker == null)
				SetTracker(new VisualElementTracker(this));

			if (element != null)
				SendVisualElementInitialized(element, this);

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (element != null && !string.IsNullOrEmpty(element.AutomationId))
				SetAutomationId(element.AutomationId);

			SetContentDescription();
			SetFocusable();
			UpdateInputTransparent();

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
				SetOnClickListener(null);
				SetOnTouchListener(null);

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
			foreach (EventHandler<VisualElementChangedEventArgs> handler in _elementChangedHandlers)
				handler(this, args);

			ElementChanged?.Invoke(this, e);

			ElevationHelper.SetElevation(this, e.NewElement);
		}
		
		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
				SetContentDescription();
			else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
				SetContentDescription();
			else if (e.PropertyName == AutomationProperties.IsInAccessibleTreeProperty.PropertyName)
				SetFocusable();
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent();
			

			ElementPropertyChanged?.Invoke(this, e);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Element == null)
				return;

			ReadOnlyCollection<Element> children = ((IElementController)Element).LogicalChildren;
			UpdateLayout(((IElementController)Element).LogicalChildren);
		}

		static void UpdateLayout(IEnumerable<Element> children)
		{
			foreach (Element element in children)  	{
				var visualElement = element as VisualElement;
				if (visualElement == null)
					continue;

				IVisualElementRenderer renderer = Platform.GetRenderer(visualElement);
				if (renderer == null && CompressedLayout.GetIsHeadless(visualElement))
					UpdateLayout(visualElement.LogicalChildren);

				renderer?.UpdateLayout();
			}
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.SetContainer(this);
		}

		protected virtual void SetAutomationId(string id)
		{
			ContentDescription = id;
		}

		protected virtual void SetContentDescription()
		{
			if (Element == null)
				return;

			if (SetHint())
				return;

			if (_defaultContentDescription == null)
				_defaultContentDescription = ContentDescription;

			var elemValue = FastRenderers.AutomationPropertiesProvider.ConcatenateNameAndHelpText(Element);

			if (!string.IsNullOrWhiteSpace(elemValue))
				ContentDescription = elemValue;
			else
				ContentDescription = _defaultContentDescription;
		}

		protected virtual void SetFocusable()
		{
			if (Element == null)
				return;

			if (!_defaultFocusable.HasValue)
				_defaultFocusable = Focusable;

			Focusable = (bool)((bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultFocusable);
		}

		protected virtual bool SetHint()
		{
			if (Element == null)
				return false;

			var textView = this as global::Android.Widget.TextView;
			if (textView == null)
				return false;

			// Let the specified Title/Placeholder take precedence, but don't set the ContentDescription (won't work anyway)
			if (((Element as Picker)?.Title ?? (Element as Entry)?.Placeholder ?? (Element as EntryCell)?.Placeholder) != null)
				return true;

			if (_defaultHint == null)
				_defaultHint = textView.Hint;

			var elemValue = FastRenderers.AutomationPropertiesProvider.ConcatenateNameAndHelpText(Element);

			if (!string.IsNullOrWhiteSpace(elemValue))
				textView.Hint = elemValue;
			else
				textView.Hint = _defaultHint;

			return true;
		}

		void UpdateInputTransparent()
		{
			InputTransparent = Element.InputTransparent;
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

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = LabelFor;

			LabelFor = (int)(id ?? _defaultLabelFor);
		}
	}
}