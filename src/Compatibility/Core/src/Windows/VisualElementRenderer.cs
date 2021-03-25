using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Input;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.VisualElement;
using WRect = Windows.Foundation.Rect;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class VisualElementRenderer<TElement, TNativeElement> : Panel, IVisualNativeElementRenderer, IDisposable, IEffectControlProvider where TElement : VisualElement
																																	  where TNativeElement : FrameworkElement
	{
		string _defaultAutomationPropertiesName;
		AccessibilityView? _defaultAutomationPropertiesAccessibilityView;
		string _defaultAutomationPropertiesHelpText;
		UIElement _defaultAutomationPropertiesLabeledBy;
		bool _disposed;
		FocusNavigationDirection focusDirection;
		EventHandler<VisualElementChangedEventArgs> _elementChangedHandlers;
		event EventHandler<PropertyChangedEventArgs> _elementPropertyChanged;
		event EventHandler _controlChanging;
		event EventHandler _controlChanged;
		VisualElementTracker<TElement, TNativeElement> _tracker;
		Microsoft.UI.Xaml.Controls.Page _containingPage; // Cache of containing page used for unfocusing
		Control _control => Control as Control;

		Canvas _backgroundLayer;

		public TNativeElement Control { get; private set; }

		public TElement Element { get; private set; }

		protected bool AutoPackage { get; set; } = true;

		protected bool AutoTrack { get; set; } = true;

		protected bool ArrangeNativeChildren { get; set; }

		protected virtual bool PreventGestureBubbling { get; set; } = false;

		IElementController ElementController => Element as IElementController;

		protected VisualElementTracker<TElement, TNativeElement> Tracker
		{
			get { return _tracker; }
			set
			{
				if (_tracker == value)
					return;

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker.Updated -= OnTrackerUpdated;
				}

				_tracker = value;

				if (_tracker != null)
				{
					_tracker.Updated += OnTrackerUpdated;
					UpdateTracker();
				}
			}
		}

		VisualElementPackager Packager { get; set; }

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				OnRegisterEffect(platformEffect);
		}

		public FrameworkElement ContainerElement
		{
			get { return this; }
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add
			{
				if (_elementChangedHandlers == null)
					_elementChangedHandlers = value;
				else
					_elementChangedHandlers = (EventHandler<VisualElementChangedEventArgs>)Delegate.Combine(_elementChangedHandlers, value);
			}

			remove { _elementChangedHandlers = (EventHandler<VisualElementChangedEventArgs>)Delegate.Remove(_elementChangedHandlers, value); }
		}

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Children.Count == 0 || Control == null)
				return new SizeRequest();

			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);
			TNativeElement child = Control;

			child.Measure(constraint);
			var result = new Size(Math.Ceiling(child.DesiredSize.Width), Math.Ceiling(child.DesiredSize.Height));

			return new SizeRequest(result);
		}

		public UIElement GetNativeElement()
		{
			return Control;
		}

		public void SetElement(VisualElement element)
		{
			TElement oldElement = Element;
			Element = (TElement)element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				oldElement.FocusChangeRequested -= OnElementFocusChangeRequested;
			}

			if (element != null)
			{
				Element.PropertyChanged += OnElementPropertyChanged;
				Element.FocusChangeRequested += OnElementFocusChangeRequested;

				if (AutoPackage && Packager == null)
					Packager = new VisualElementPackager(this);

				if (AutoTrack && Tracker == null)
				{
					Tracker = new VisualElementTracker<TElement, TNativeElement>();
				}

				// Disabled until reason for crashes with unhandled exceptions is discovered
				// Without this some layouts may end up with improper sizes, however their children
				// will position correctly
				// Consider using Loading vs Loaded if this is added back, calling in Loaded appears to be to late in the layout cycle
				// and may cause issues
				//Loaded += (sender, args) =>
				//{
				if (Packager != null)
					Packager.Load();
				//};
			}

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));

			if (_control != null && this is IDontGetFocus)
			{
				_control.GotFocus += OnGotFocus;
				_control.GettingFocus += OnGettingFocus;
			}

			var controller = (IElementController)oldElement;
			if (controller != null && (Panel)controller.EffectControlProvider == this)
			{
				controller.EffectControlProvider = null;
			}

			controller = element;
			if (controller != null)
				controller.EffectControlProvider = this;
		}

		void OnGettingFocus(UIElement sender, GettingFocusEventArgs args) => focusDirection = args.Direction;

		void OnGotFocus(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource == Control)
				FocusManager.TryMoveFocus(focusDirection != FocusNavigationDirection.None ? focusDirection : FocusNavigationDirection.Next);
		}

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;
		event EventHandler<PropertyChangedEventArgs> IVisualNativeElementRenderer.ElementPropertyChanged
		{
			add => _elementPropertyChanged += value;
			remove => _elementPropertyChanged -= value;
		}

		event EventHandler IVisualNativeElementRenderer.ControlChanging
		{
			add { _controlChanging += value; }
			remove { _controlChanging -= value; }
		}
		event EventHandler IVisualNativeElementRenderer.ControlChanged
		{
			add { _controlChanged += value; }
			remove { _controlChanged -= value; }
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (Element == null || finalSize.Width * finalSize.Height == 0)
				return finalSize;

			Element.IsInNativeLayout = true;

			var myRect = new WRect(0, 0, finalSize.Width, finalSize.Height);

			if (Control != null)
			{
				Control.Arrange(myRect);
			}

			List<UIElement> arrangedChildren = null;
			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				var child = ElementController.LogicalChildren[i] as VisualElement;
				if (child == null)
					continue;
				IVisualElementRenderer renderer = Platform.GetRenderer(child);
				if (renderer == null)
					continue;
				Rectangle bounds = child.Bounds;

				renderer.ContainerElement.Arrange(new WRect(bounds.X, bounds.Y, Math.Max(0, bounds.Width), Math.Max(0, bounds.Height)));

				if (ArrangeNativeChildren)
				{
					if (arrangedChildren == null)
						arrangedChildren = new List<UIElement>();
					arrangedChildren.Add(renderer.ContainerElement);
				}
			}

			if (ArrangeNativeChildren)
			{
				// in the event that a custom renderer has added native controls,
				// we need to be sure to arrange them so that they are laid out.
				var nativeChildren = Children;
				for (int i = 0; i < nativeChildren.Count; i++)
				{
					var nativeChild = nativeChildren[i];
					if (arrangedChildren?.Contains(nativeChild) == true)
						// don't try to rearrange renderers that were just arranged, 
						// lest you suffer a layout cycle
						continue;
					else
						nativeChild.Arrange(myRect);
				}
			}

			_backgroundLayer?.Arrange(myRect);

			Element.IsInNativeLayout = false;

			return finalSize;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
				return;

			_disposed = true;

			Tracker?.Dispose();
			Tracker = null;

			Packager?.Dispose();
			Packager = null;

			if (_control != null)
			{
				_control.GotFocus -= OnGotFocus;
				_control.GettingFocus -= OnGettingFocus;
			}
			SetNativeControl(null);
			SetElement(null);
		}

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (Element == null || availableSize.Width * availableSize.Height == 0)
				return new Windows.Foundation.Size(0, 0);

			if (Element is Layout layout)
			{
				layout.ResolveLayoutChanges();
			}

			Element.IsInNativeLayout = true;

			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				var child = ElementController.LogicalChildren[i] as VisualElement;
				if (child == null)
					continue;
				IVisualElementRenderer renderer = Platform.GetRenderer(child);
				if (renderer == null)
					continue;

				renderer.ContainerElement.Measure(availableSize);
			}

			double width = Math.Max(0, Element.Width);
			double height = Math.Max(0, Element.Height);
			var result = new Windows.Foundation.Size(width, height);
			if (Control != null)
			{
				double w = Element.Width;
				double h = Element.Height;
				if (w == -1)
					w = availableSize.Width;
				if (h == -1)
					h = availableSize.Height;
				w = Math.Max(0, w);
				h = Math.Max(0, h);
				Control.Measure(new Windows.Foundation.Size(w, h));
			}

			Element.IsInNativeLayout = false;

			return result;
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);
			if (_elementChangedHandlers != null)
				_elementChangedHandlers(this, args);

			EventHandler<ElementChangedEventArgs<TElement>> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		protected void UpdateTabStop()
		{
			if (_control == null)
				return;
			_control.IsTabStop = Element.IsTabStop;

			if (this is ITabStopOnDescendants)
				_control?.GetChildren<Control>().ForEach(c => c.IsTabStop = Element.IsTabStop);
		}

		protected void UpdateTabIndex()
		{
			if (_control != null)
				_control.TabIndex = Element.TabIndex;
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
				SetAutomationPropertiesHelpText();
			else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
				SetAutomationPropertiesName();
			else if (e.PropertyName == AutomationProperties.IsInAccessibleTreeProperty.PropertyName)
				SetAutomationPropertiesAccessibilityView();
			else if (e.PropertyName == AutomationProperties.LabeledByProperty.PropertyName)
				SetAutomationPropertiesLabeledBy();
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName ||
					e.PropertyName == Layout.CascadeInputTransparentProperty.PropertyName)
				UpdateInputTransparent();
			if (e.PropertyName == Specifics.AccessKeyProperty.PropertyName ||
					e.PropertyName == Specifics.AccessKeyPlacementProperty.PropertyName ||
					e.PropertyName == Specifics.AccessKeyHorizontalOffsetProperty.PropertyName ||
					e.PropertyName == Specifics.AccessKeyVerticalOffsetProperty.PropertyName)
				UpdateAccessKey();
			else if (e.PropertyName == VisualElement.IsTabStopProperty.PropertyName)
				UpdateTabStop();
			else if (e.PropertyName == VisualElement.TabIndexProperty.PropertyName)
				UpdateTabIndex();

			_elementPropertyChanged?.Invoke(this, e);
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.SetContainer(this);
			effect.SetControl(Control);
		}

		protected virtual void SetAutomationId(string id)
		{
			this.SetAutomationPropertiesAutomationId(id);
		}

		protected virtual void SetAutomationPropertiesName()
		{
			if (Control == null)
				return;

			_defaultAutomationPropertiesName = Control.SetAutomationPropertiesName(Element, _defaultAutomationPropertiesName);
		}

		protected virtual void SetAutomationPropertiesAccessibilityView()
		{
			if (Control == null)
				return;

			_defaultAutomationPropertiesAccessibilityView = Control.SetAutomationPropertiesAccessibilityView(Element, _defaultAutomationPropertiesAccessibilityView);
		}

		protected virtual void SetAutomationPropertiesHelpText()
		{
			if (Control == null)
				return;

			_defaultAutomationPropertiesHelpText = Control.SetAutomationPropertiesHelpText(Element, _defaultAutomationPropertiesHelpText);
		}

		protected virtual void SetAutomationPropertiesLabeledBy()
		{
			if (Control == null)
				return;

			_defaultAutomationPropertiesLabeledBy = Control.SetAutomationPropertiesLabeledBy(Element, _defaultAutomationPropertiesLabeledBy);
		}

		protected void SetNativeControl(TNativeElement control)
		{
			_controlChanging?.Invoke(this, EventArgs.Empty);
			TNativeElement oldControl = Control;
			Control = control;

			if (oldControl != null)
			{
				Children.Remove(oldControl);

				oldControl.Loaded -= OnControlLoaded;
				oldControl.GotFocus -= OnControlGotFocus;
				oldControl.LostFocus -= OnControlLostFocus;
			}

			UpdateTracker();

			if (control == null)
			{
				_controlChanged?.Invoke(this, EventArgs.Empty);
				return;
			}

			Control.HorizontalAlignment = HorizontalAlignment.Stretch;
			Control.VerticalAlignment = VerticalAlignment.Stretch;

			if (Element == null)
				throw new InvalidOperationException(
					"Cannot assign a native control without an Element; Renderer unbound and/or disposed. " +
					"Please consult Microsoft.Maui.Controls.Compatibility renderers for reference implementation of OnElementChanged.");

			Element.IsNativeStateConsistent = false;
			control.Loaded += OnControlLoaded;

			control.GotFocus += OnControlGotFocus;
			control.LostFocus += OnControlLostFocus;
			Children.Add(control);
			
			UpdateBackgroundColor();
			UpdateBackground();

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				SetAutomationId(Element.AutomationId);

			_controlChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void UpdateBackgroundColor()
		{
			Color backgroundColor = Element.BackgroundColor;

			var backgroundLayer = (Panel)this;
			if (_backgroundLayer != null)
			{
				backgroundLayer = _backgroundLayer;
				Background = null; // Make the container effectively hit test invisible
			}

			if (_control != null)
			{
				if (!backgroundColor.IsDefault)
				{
					_control.Background = backgroundColor.ToBrush();
				}
				else
				{
					_control.ClearValue(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty);
					backgroundLayer.ClearValue(BackgroundProperty);
				}
			}
			else
			{
				if (!backgroundColor.IsDefault)
				{
					backgroundLayer.Background = backgroundColor.ToBrush();
				}
				else
				{
					backgroundLayer.ClearValue(BackgroundProperty);
				}
			}
		}

		protected virtual void UpdateBackground()
		{
			Color backgroundColor = Element.BackgroundColor;
			Brush background = Element.Background;

			var backgroundLayer = (Panel)this;
			if (_backgroundLayer != null)
			{
				backgroundLayer = _backgroundLayer;
				Background = null;
			}

			if (_control != null)
			{
				if (!Brush.IsNullOrEmpty(background))
					_control.Background = background.ToBrush();
				else
				{
					if (!backgroundColor.IsDefault)
						_control.Background = backgroundColor.ToBrush();
					else
					{
						_control.ClearValue(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty);
						backgroundLayer.ClearValue(BackgroundProperty);
					}
				}
			}
			else
			{
				if (!Brush.IsNullOrEmpty(background))
					backgroundLayer.Background = background.ToBrush();
				else
				{
					if (!backgroundColor.IsDefault)
						backgroundLayer.Background = backgroundColor.ToBrush();
					else
						backgroundLayer.ClearValue(BackgroundProperty);
				}
			}
		}

		protected void UpdateAccessKey() {
			var control = Control;
			var element = Element as IElementConfiguration<TElement>;

			if (element != null && control != null)
				AccessKeyHelper.UpdateAccessKey(Control, Element);
		}

		protected virtual void UpdateNativeControl()
		{
			UpdateEnabled();
			UpdateInputTransparent();
			UpdateAccessKey();
			UpdateTabStop();
			UpdateTabIndex();
			SetAutomationPropertiesHelpText();
			SetAutomationPropertiesName();
			SetAutomationPropertiesAccessibilityView();
			SetAutomationPropertiesLabeledBy();
		}

		internal virtual void OnElementFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			if (_control == null)
				return;

			if (args.Focus)
				args.Result = _control.Focus(FocusState.Programmatic);
			else
			{
				UnfocusControl(_control);
				args.Result = true;
			}
		}

		internal void UnfocusControl(Control control)
		{
			if (control == null || !control.IsEnabled || !control.IsTabStop)
				return;

			// "Unfocusing" doesn't really make sense on Windows; for accessibility reasons,
			// something always has focus. So forcing the unfocusing of a control would normally 
			// just move focus to the next control, or leave it on the current control if no other
			// focus targets are available. This is what happens if you use the "disable/enable"
			// hack. What we *can* do is set the focus to the Page which contains Control;
			// this will cause Control to lose focus without shifting focus to, say, the next Entry 

			if (_containingPage == null)
			{
				// Work our way up the tree to find the containing Page
				DependencyObject parent = _control;
				while (parent != null && !(parent is Microsoft.UI.Xaml.Controls.Page))
				{
					parent = VisualTreeHelper.GetParent(parent);
				}
				_containingPage = parent as Microsoft.UI.Xaml.Controls.Page;
			}

			if (_containingPage != null)
			{
				// Cache the tabstop setting
				var wasTabStop = _containingPage.IsTabStop;

				// Controls can only get focus if they're a tabstop
				_containingPage.IsTabStop = true;
				_containingPage.Focus(FocusState.Programmatic);

				// Restore the tabstop setting; that may cause the Page to lose focus,
				// but it won't restore the focus to Control
				_containingPage.IsTabStop = wasTabStop;
			}
		}

		void OnControlGotFocus(object sender, RoutedEventArgs args)
		{
			((IVisualElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnControlLoaded(object sender, RoutedEventArgs args)
		{
			Element.IsNativeStateConsistent = true;
		}

		void OnControlLostFocus(object sender, RoutedEventArgs args)
		{
			((IVisualElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnTrackerUpdated(object sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		void UpdateEnabled()
		{
			if (_control != null)
				_control.IsEnabled = Element.IsEnabled;
			else
				IsHitTestVisible = Element.IsEnabled && !Element.InputTransparent;
		}

		void UpdateInputTransparent()
		{
			if (NeedsBackgroundLayer(Element))
			{
				IsHitTestVisible = true;
				AddBackgroundLayer();
			}
			else
			{
				RemoveBackgroundLayer();
				IsHitTestVisible = Element.IsEnabled && !Element.InputTransparent;

				if (!IsHitTestVisible)
				{
					return;
				}

				// If this Panel's background brush is null, the UWP considers it transparent to hit testing (even 
				// when IsHitTestVisible is true). So we have to explicitly set a background brush to make it show up
				// in hit testing. 
				if (Element is Layout && Background == null)
				{
					Background = new WSolidColorBrush(Microsoft.UI.Colors.Transparent);
				}
			}
		}

		void AddBackgroundLayer()
		{
			if (_backgroundLayer != null)
			{
				return;
			}

			// In UWP, once a control has hit testing disabled, all of its child controls
			// also have hit testing disabled. The exception is a Panel with its 
			// Background Brush set to `null`; the Panel will be invisible to hit testing, but its
			// children will work just fine. 

			// In order to handle the situation where we need the layout to be invisible to hit testing,
			// the child controls to be visible to hit testing, *and* we need to support non-null
			// background brushes, we insert another empty Panel which is invisible to hit testing; that
			// Panel will be our Background color

			_backgroundLayer = new Canvas { IsHitTestVisible = false };
			Children.Insert(0, _backgroundLayer);

			UpdateBackgroundColor();
			UpdateBackground();
		}

		void RemoveBackgroundLayer()
		{
			if (_backgroundLayer == null)
			{
				return;
			}

			Children.Remove(_backgroundLayer);
			_backgroundLayer = null;

			UpdateBackgroundColor();
			UpdateBackground();
		}

		internal static bool NeedsBackgroundLayer(VisualElement element)
		{
			if (!(element is Layout layout))
			{
				return false;
			}

			if (layout.IsEnabled && layout.InputTransparent && !layout.CascadeInputTransparent)
			{
				return true;
			}

			return false;
		}


		void UpdateTracker()
		{
			if (_tracker == null)
				return;

			_tracker.PreventGestureBubbling = PreventGestureBubbling;
			_tracker.Control = Control;
			_tracker.Element = Element;
			_tracker.Container = ContainerElement;
		}
	}
}