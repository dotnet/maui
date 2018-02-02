using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;
using Xamarin.Forms.Platform.Android.FastRenderers;

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
		bool _inputTransparentInherited = true;

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
			if (InputTransparent && _inputTransparentInherited)
			{
				// If the Element is InputTransparent, this ViewGroup will be marked InputTransparent
				// If we're InputTransparent and our transparency should be applied to our child controls,
				// we return false on all touch events without even bothering to send them to the child Views

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
			var reference = Guid.NewGuid().ToString();
			Performance.Start(reference);
			Tracker?.UpdateLayout();
			Performance.Stop(reference);
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

			var reference = Guid.NewGuid().ToString();
			Performance.Start(reference);

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
			UpdateInputTransparentInherited();

			Performance.Stop(reference);
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
			else if (e.PropertyName == Xamarin.Forms.Layout.CascadeInputTransparentProperty.PropertyName)
				UpdateInputTransparentInherited();

			ElementPropertyChanged?.Invoke(this, e);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Element == null)
				return;

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
			=> AutomationPropertiesProvider.SetAutomationId(this, Element, id);

		protected virtual void SetContentDescription()
			=> AutomationPropertiesProvider.SetContentDescription(this, Element, ref _defaultContentDescription, ref _defaultHint);

		protected virtual void SetFocusable()
			=> AutomationPropertiesProvider.SetFocusable(this, Element, ref _defaultFocusable);

		void UpdateInputTransparent()
		{
			InputTransparent = Element.InputTransparent;
		}

		void UpdateInputTransparentInherited()
		{
			var layout = Element as Layout;

			if (layout == null)
			{
				return;
			}

			_inputTransparentInherited = layout.CascadeInputTransparent;
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
			=> LabelFor = id ?? LabelFor;
	}
}