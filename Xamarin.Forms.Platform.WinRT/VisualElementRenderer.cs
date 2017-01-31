using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class VisualElementRenderer<TElement, TNativeElement> : Panel, IVisualElementRenderer, IDisposable, IEffectControlProvider where TElement : VisualElement
																																	  where TNativeElement : FrameworkElement
	{
		string _defaultAutomationPropertiesName;
		AccessibilityView? _defaultAutomationPropertiesAccessibilityView;
		string _defaultAutomationPropertiesHelpText;
		UIElement _defaultAutomationPropertiesLabeledBy;
		bool _disposed;
		EventHandler<VisualElementChangedEventArgs> _elementChangedHandlers;
		VisualElementTracker<TElement, TNativeElement> _tracker;

		public TNativeElement Control { get; private set; }

		public TElement Element { get; private set; }

		protected bool AutoPackage { get; set; } = true;

		protected bool AutoTrack { get; set; } = true;

		protected bool ArrangeNativeChildren { get; set; }

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

		public void Dispose()
		{
			Dispose(true);
		}

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
			if (Children.Count == 0)
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
				//Loaded += (sender, args) => {
				if (Packager != null)
					Packager.Load();
				//};
			}

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));

			var controller = (IElementController)oldElement;
			if (controller != null && controller.EffectControlProvider == this)
			{
				controller.EffectControlProvider = null;
			}

			controller = element;
			if (controller != null)
				controller.EffectControlProvider = this;
		}

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (Element == null || finalSize.Width * finalSize.Height == 0)
				return finalSize;

			Element.IsInNativeLayout = true;

			var myRect = new Rect(0, 0, finalSize.Width, finalSize.Height);

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

				renderer.ContainerElement.Arrange(new Rect(bounds.X, bounds.Y, Math.Max(0, bounds.Width), Math.Max(0, bounds.Height)));

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

			Element.IsInNativeLayout = false;

			return finalSize;
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

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (Element == null || availableSize.Width * availableSize.Height == 0)
				return new Windows.Foundation.Size(0, 0);

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

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == Accessibility.HintProperty.PropertyName)
				SetAutomationPropertiesHelpText();
			else if (e.PropertyName == Accessibility.NameProperty.PropertyName)
				SetAutomationPropertiesName();
			else if (e.PropertyName == Accessibility.IsInAccessibleTreeProperty.PropertyName)
				SetAutomationPropertiesAccessibilityView();
			else if (e.PropertyName == Accessibility.LabeledByProperty.PropertyName)
				SetAutomationPropertiesLabeledBy();
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.Container = this;
			effect.Control = Control;
		}

		protected virtual void SetAutomationId(string id)
		{
			SetValue(AutomationProperties.AutomationIdProperty, id);
		}

		protected virtual void SetAutomationPropertiesName()
		{
			if (Element == null || Control == null)
				return;

			if (_defaultAutomationPropertiesName == null)
				_defaultAutomationPropertiesName = (string)Control.GetValue(AutomationProperties.NameProperty);

			var elemValue = (string)Element.GetValue(Accessibility.NameProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(AutomationProperties.NameProperty, elemValue);
			else
				Control.SetValue(AutomationProperties.NameProperty, _defaultAutomationPropertiesName);
		}

		protected virtual void SetAutomationPropertiesAccessibilityView()
		{
			if (Element == null || Control == null)
				return;

			if (!_defaultAutomationPropertiesAccessibilityView.HasValue)
				_defaultAutomationPropertiesAccessibilityView = (AccessibilityView)Control.GetValue(AutomationProperties.AccessibilityViewProperty);

			var newValue = _defaultAutomationPropertiesAccessibilityView;
			var elemValue = (bool?)Element.GetValue(Accessibility.IsInAccessibleTreeProperty);

			if (elemValue == true)
				newValue = AccessibilityView.Content;
			else if (elemValue == false)
				newValue = AccessibilityView.Raw;

			Control.SetValue(AutomationProperties.AccessibilityViewProperty, newValue);
		}

		protected virtual void SetAutomationPropertiesHelpText()
		{
			if (Element == null || Control == null)
				return;

			if (_defaultAutomationPropertiesHelpText == null)
				_defaultAutomationPropertiesHelpText = (string)Control.GetValue(AutomationProperties.HelpTextProperty);

			var elemValue = (string)Element.GetValue(Accessibility.HintProperty);

			if (!string.IsNullOrWhiteSpace(elemValue))
				Control.SetValue(AutomationProperties.HelpTextProperty, elemValue);
			else
				Control.SetValue(AutomationProperties.HelpTextProperty, _defaultAutomationPropertiesHelpText);
		}

		protected virtual void SetAutomationPropertiesLabeledBy()
		{
			if (Element == null || Control == null)
				return;

			if (_defaultAutomationPropertiesLabeledBy == null)
				_defaultAutomationPropertiesLabeledBy = (UIElement)Control.GetValue(AutomationProperties.LabeledByProperty);

			var elemValue = (VisualElement)Element.GetValue(Accessibility.LabeledByProperty);
			var renderer = elemValue?.GetOrCreateRenderer();
			var nativeElement = renderer?.GetNativeElement();

			if (nativeElement != null)
				Control.SetValue(AutomationProperties.LabeledByProperty, nativeElement);
			else
				Control.SetValue(AutomationProperties.LabeledByProperty, _defaultAutomationPropertiesLabeledBy);
		}

		protected void SetNativeControl(TNativeElement control)
		{
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
				return;

			Control.HorizontalAlignment = HorizontalAlignment.Stretch;
			Control.VerticalAlignment = VerticalAlignment.Stretch;

			Children.Add(control);

			Element.IsNativeStateConsistent = false;
			control.Loaded += OnControlLoaded;

			control.GotFocus += OnControlGotFocus;
			control.LostFocus += OnControlLostFocus;

			UpdateBackgroundColor();

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				SetAutomationId(Element.AutomationId);
		}

		protected virtual void UpdateBackgroundColor()
		{
			Color backgroundColor = Element.BackgroundColor;
			var control = Control as Control;
			if (control != null)
			{
				if (!backgroundColor.IsDefault)
				{
					control.Background = backgroundColor.ToBrush();
				}
				else
				{
					control.ClearValue(Windows.UI.Xaml.Controls.Control.BackgroundProperty);
				}
			}
			else
			{
				if (!backgroundColor.IsDefault)
				{
					Background = backgroundColor.ToBrush();
				}
				else
				{
					ClearValue(BackgroundProperty);
				}
			}
		}

		protected virtual void UpdateNativeControl()
		{
			UpdateEnabled();
			SetAutomationPropertiesHelpText();
			SetAutomationPropertiesName();
			SetAutomationPropertiesAccessibilityView();
			SetAutomationPropertiesLabeledBy();
		}

		internal virtual void OnElementFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			var control = Control as Control;
			if (control == null)
				return;

			if (args.Focus)
				args.Result = control.Focus(FocusState.Programmatic);
			else
			{
				UnfocusControl(control);
				args.Result = true;
			}
		}

		internal void UnfocusControl(Control control)
		{
			if (control == null || !control.IsEnabled)
				return;

			control.IsEnabled = false;
			control.IsEnabled = true;
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
			var control = Control as Control;
			if (control != null)
				control.IsEnabled = Element.IsEnabled;
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