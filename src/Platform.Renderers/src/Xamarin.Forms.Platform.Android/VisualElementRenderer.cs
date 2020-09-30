using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.View;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.FastRenderers;
using AView = Android.Views.View;


namespace Xamarin.Forms.Platform.Android
{
	public abstract class VisualElementRenderer<TElement> : FormsViewGroup, IVisualElementRenderer, IDisposedState,
		IEffectControlProvider where TElement : VisualElement
	{
		readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers = new List<EventHandler<VisualElementChangedEventArgs>>();

		VisualElementRendererFlags _flags = VisualElementRendererFlags.AutoPackage | VisualElementRendererFlags.AutoTrack;

		string _defaultContentDescription;
		bool? _defaultFocusable;
		ImportantForAccessibility? _defaultImportantForAccessibility;
		string _defaultHint;
		bool _cascadeInputTransparent = true;

		VisualElementPackager _packager;
		PropertyChangedEventHandler _propertyChangeHandler;

		GestureManager _gestureManager;

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
			if (Element == null || (InputTransparent && _cascadeInputTransparent))
			{
				// If the Element is InputTransparent, this ViewGroup will be marked InputTransparent
				// If we're InputTransparent and our transparency should be applied to our child controls,
				// we return false on all touch events without even bothering to send them to the child Views

				return false; // IOW, not handled
			}

			return base.DispatchTouchEvent(e);
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use VisualElementRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
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
			Performance.Start(out string reference);
			Tracker?.UpdateLayout();
			Performance.Stop(reference);
		}

		protected int TabIndex { get; set; } = 0;

		protected bool TabStop { get; set; } = true;

		protected void UpdateTabStop()
		{
			TabStop = Element.IsTabStop;
			UpdateParentPageTraversalOrder();
		}

		protected void UpdateTabIndex()
		{
			TabIndex = Element.TabIndex;
			UpdateParentPageTraversalOrder();
		}

		bool CheckCustomNextFocus(AView focused, FocusSearchDirection direction)
		{
			return direction == FocusSearchDirection.Forward && focused.NextFocusForwardId != NoId ||
				direction == FocusSearchDirection.Down && focused.NextFocusDownId != NoId ||
				direction == FocusSearchDirection.Left && focused.NextFocusLeftId != NoId ||
				direction == FocusSearchDirection.Right && focused.NextFocusRightId != NoId ||
				direction == FocusSearchDirection.Up && focused.NextFocusUpId != NoId;
		}

		public override AView FocusSearch(AView focused, [GeneratedEnum] FocusSearchDirection direction)
		{
			if (CheckCustomNextFocus(focused, direction))
				return base.FocusSearch(focused, direction);

			var element = Element as ITabStopElement;
			int maxAttempts = 0;
			var tabIndexes = element?.GetTabIndexesOnParentPage(out maxAttempts);
			if (tabIndexes == null)
				return base.FocusSearch(focused, direction);

			// use OS default--there's no need for us to keep going if there's one or fewer tab indexes!
			if (tabIndexes.Count <= 1)
				return base.FocusSearch(focused, direction);

			int tabIndex = element.TabIndex;
			AView control = null;
			int attempt = 0;
			bool forwardDirection = !(
				(direction & FocusSearchDirection.Backward) != 0 ||
				(direction & FocusSearchDirection.Left) != 0 ||
				(direction & FocusSearchDirection.Up) != 0);

			do
			{
				element = element.FindNextElement(forwardDirection, tabIndexes, ref tabIndex);
				var renderer = (element as VisualElement)?.GetRenderer();
				control = (renderer as ITabStop)?.TabStop;
			} while (!(control?.Focusable == true || ++attempt >= maxAttempts));

			// when the user focuses on picker show a popup dialog
			if (control is IPopupTrigger popupElement)
				popupElement.ShowPopupOnFocus = true;

			return control?.Focusable == true ? control : null;
		}

		public ViewGroup ViewGroup => this;
		AView IVisualElementRenderer.View => this;

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public void SetElement(TElement element)
		{
			TElement oldElement = Element;
			Element = element ?? throw new ArgumentNullException(nameof(element));

			Performance.Start(out string reference);

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= _propertyChangeHandler;
			}

			Color currentColor = oldElement?.BackgroundColor ?? Color.Default;

			if (element.BackgroundColor != currentColor)
				UpdateBackgroundColor();

			if (element.Background != null)
				UpdateBackground();

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

			if (oldElement != null)
				Tracker?.UpdateLayout();

			if (element != null)
				SendVisualElementInitialized(element, this);

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (!string.IsNullOrEmpty(element.AutomationId))
				SetAutomationId(element.AutomationId);

			SetContentDescription();
			SetFocusable();
			UpdateInputTransparent();
			UpdateInputTransparentInherited();
			UpdateTabStop();
			UpdateTabIndex();

			Performance.Stop(reference);
		}

		/// <summary>
		/// Determines whether the native control is disposed of when this renderer is disposed
		/// Can be overridden in deriving classes 
		/// </summary>
		protected virtual bool ManageNativeControlLifetime => true;

		bool CheckFlagsForDisposed() => (_flags & VisualElementRendererFlags.Disposed) != 0;
		bool IDisposedState.IsDisposed => CheckFlagsForDisposed();

		protected override void Dispose(bool disposing)
		{
			if (CheckFlagsForDisposed())
				return;

			_flags |= VisualElementRendererFlags.Disposed;

			if (disposing)
			{
				SetOnClickListener(null);
				SetOnTouchListener(null);

				EffectUtilities.UnregisterEffectControlProvider(this, Element);

				if (Element != null)
				{
					Element.PropertyChanged -= _propertyChangeHandler;
				}

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

				if (_gestureManager != null)
				{
					_gestureManager.Dispose();
					_gestureManager = null;
				}

				if (ManageNativeControlLifetime)
				{
					while (ChildCount > 0)
					{
						AView child = GetChildAt(0);
						child.RemoveFromParent();
						child.Dispose();
					}
				}

				if (Element != null)
				{
					if (Platform.GetRenderer(Element) == this)
						Platform.SetRenderer(Element, null);

					Element = null;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			Invalidate();
		}

		protected virtual Size MinimumSize()
		{
			return new Size();
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);

			// The list of event handlers can be changed inside the handlers. (ex.: are used CompressedLayout)
			// To avoid an exception, a copy of the handlers is called.
			var handlers = _elementChangedHandlers.ToArray();
			foreach (var handler in handlers)
				handler(this, args);

			ElementChanged?.Invoke(this, e);

			ElevationHelper.SetElevation(this, e.NewElement);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
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
			else if (e.PropertyName == VisualElement.IsTabStopProperty.PropertyName)
				UpdateTabStop();
			else if (e.PropertyName == VisualElement.TabIndexProperty.PropertyName)
				UpdateTabIndex();
			else if (e.PropertyName == nameof(Element.Parent))
				UpdateParentPageTraversalOrder();

			ElementPropertyChanged?.Invoke(this, e);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Element == null)
				return;

			UpdateLayout(((IElementController)Element).LogicalChildren);
		}

		public override void Draw(Canvas canvas)
		{
			canvas.ClipShape(Context, Element);

			base.Draw(canvas);
		}

		static void UpdateLayout(IEnumerable<Element> children)
		{
			foreach (Element element in children)
			{
				var visualElement = element as VisualElement;
				if (visualElement == null)
					continue;

				IVisualElementRenderer renderer = Platform.GetRenderer(visualElement);
				if (renderer == null && CompressedLayout.GetIsHeadless(visualElement))
					UpdateLayout(visualElement.LogicalChildren);

				renderer?.UpdateLayout();
			}
		}

		void UpdateParentPageTraversalOrder()
		{
			IViewParent parentRenderer = Parent;
			while (parentRenderer != null && !(parentRenderer is IOrderedTraversalController))
				parentRenderer = parentRenderer.Parent;

			if (parentRenderer is IOrderedTraversalController controller)
				controller.UpdateTraversalOrder();
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
			=> AutomationPropertiesProvider.SetFocusable(this, Element, ref _defaultFocusable, ref _defaultImportantForAccessibility);

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

			_cascadeInputTransparent = layout.CascadeInputTransparent;
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

		protected virtual void UpdateBackground()
		{
			Brush background = Element.Background;

			this.UpdateBackground(background);
		}

		internal virtual void SendVisualElementInitialized(VisualElement element, AView nativeView)
		{
			element.SendViewInitialized(nativeView);
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
			=> ViewCompat.SetLabelFor(this, id ?? ViewCompat.GetLabelFor(this));
	}
}