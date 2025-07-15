using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.VisualElement;
using WRect = Windows.Foundation.Rect;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.VisualElementRenderer instead")]
	public partial class VisualElementRenderer<TElement, TNativeElement> : Panel, IVisualNativeElementRenderer, IDisposable, IEffectControlProvider where TElement : VisualElement
																																	  where TNativeElement : FrameworkElement
	{
		string _defaultAutomationPropertiesName;
		AccessibilityView? _defaultAutomationPropertiesAccessibilityView;
		string _defaultAutomationPropertiesHelpText;
		UIElement _defaultAutomationPropertiesLabeledBy;
		bool _disposed;
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
#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons.
			if (Children.Count == 0 || Control == null)
				return new SizeRequest();
#pragma warning restore RS0030 // Do not use banned APIs

			var constraint = new global::Windows.Foundation.Size(widthConstraint, heightConstraint);
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
				Packager?.Load();
				//};
			}

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));

			var controller = (IElementController)oldElement;
			if (controller != null && (Panel)controller.EffectControlProvider == this)
			{
				controller.EffectControlProvider = null;
			}

			controller = element;
			if (controller != null)
				controller.EffectControlProvider = this;
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

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (Element == null || finalSize.Width * finalSize.Height == 0)
				return finalSize;

			Element.IsInPlatformLayout = true;

			var myRect = new WRect(0, 0, finalSize.Width, finalSize.Height);

			Control?.Arrange(myRect);

			List<UIElement> arrangedChildren = null;
			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				var child = ElementController.LogicalChildren[i] as VisualElement;
				if (child == null)
					continue;
				IVisualElementRenderer renderer = Platform.GetRenderer(child);
				if (renderer == null)
					continue;
				var bounds = child.Bounds;

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
				// we need to be sure to arrange them so that they are arranged.
#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons.
				var nativeChildren = Children;
#pragma warning restore RS0030 // Do not use banned APIs

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

			Element.IsInPlatformLayout = false;

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

			SetNativeControl(null);
			SetElement(null);
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (Element == null || availableSize.Width * availableSize.Height == 0)
				return new global::Windows.Foundation.Size(0, 0);

			Element.IsInPlatformLayout = true;

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
			var result = new global::Windows.Foundation.Size(width, height);
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
				Control.Measure(new global::Windows.Foundation.Size(w, h));
			}

			Element.IsInPlatformLayout = false;

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

			_elementPropertyChanged?.Invoke(this, e);
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.Container = this;
			effect.Control = Control;
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

			// TODO MAUI
			_defaultAutomationPropertiesLabeledBy = Control.SetAutomationPropertiesLabeledBy(Element, null, _defaultAutomationPropertiesLabeledBy);
		}

		protected void SetNativeControl(TNativeElement control)
		{
			_controlChanging?.Invoke(this, EventArgs.Empty);
			TNativeElement oldControl = Control;
			Control = control;

			if (oldControl != null)
			{
#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons.
				Children.Remove(oldControl);
#pragma warning restore RS0030 // Do not use banned APIs

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

			Control.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
			Control.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

			if (Element == null)
				throw new InvalidOperationException(
					"Cannot assign a native control without an Element; Renderer unbound and/or disposed. " +
					"Please consult Microsoft.Maui.Controls.Compatibility renderers for reference implementation of OnElementChanged.");

			Element.IsPlatformStateConsistent = false;
			control.Loaded += OnControlLoaded;

			control.GotFocus += OnControlGotFocus;
			control.LostFocus += OnControlLostFocus;

#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons.
			Children.Add(control);
#pragma warning restore RS0030 // Do not use banned APIs

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
				if (!backgroundColor.IsDefault())
				{
					_control.Background = backgroundColor.ToPlatform();
				}
				else
				{
					_control.ClearValue(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty);
					backgroundLayer.ClearValue(BackgroundProperty);
				}
			}
			else
			{
				if (!backgroundColor.IsDefault())
				{
					backgroundLayer.Background = backgroundColor.ToPlatform();
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
					if (!backgroundColor.IsDefault())
						_control.Background = backgroundColor.ToPlatform();
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
					if (!backgroundColor.IsDefault())
						backgroundLayer.Background = backgroundColor.ToPlatform();
					else
						backgroundLayer.ClearValue(BackgroundProperty);
				}
			}
		}

		protected void UpdateAccessKey()
		{
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
			SetAutomationPropertiesHelpText();
			SetAutomationPropertiesName();
			SetAutomationPropertiesAccessibilityView();
			SetAutomationPropertiesLabeledBy();
		}

		[PortHandler]
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

		[PortHandler]
		internal void UnfocusControl(Control control)
		{
			if (control == null || !control.IsEnabled)
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

		[PortHandler]
		void OnControlGotFocus(object sender, RoutedEventArgs args)
		{
			((IVisualElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnControlLoaded(object sender, RoutedEventArgs args)
		{
			Element.IsPlatformStateConsistent = true;
		}

		[PortHandler]
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

#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons.
			Children.Insert(0, _backgroundLayer);
#pragma warning restore RS0030 // Do not use banned APIs

			UpdateBackgroundColor();
			UpdateBackground();
		}

		void RemoveBackgroundLayer()
		{
			if (_backgroundLayer == null)
			{
				return;
			}

#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons.
			Children.Remove(_backgroundLayer);
#pragma warning restore RS0030 // Do not use banned APIs

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

			_tracker.Control = Control;
			_tracker.Element = Element;
			_tracker.Container = ContainerElement;
		}
	}
}