using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class VisualElementRenderer<TElement, TNativeElement> : Panel, IVisualElementRenderer, IEffectControlProvider where TElement : VisualElement where TNativeElement : FrameworkElement
	{
		readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers = new List<EventHandler<VisualElementChangedEventArgs>>();

		Brush _initialBrush;

		VisualElementTracker _tracker;

		IElementController ElementController => Element as IElementController;

		public TNativeElement Control { get; private set; }

		public TElement Element { get; private set; }

		protected bool AutoPackage { get; set; } = true;

		protected bool AutoTrack { get; set; } = true;

		protected VisualElementTracker Tracker
		{
			get { return _tracker; }
			set
			{
				if (_tracker == value)
					return;

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker.Updated -= HandleTrackerUpdated;
				}

				_tracker = value;

				if (_tracker != null)
					_tracker.Updated += HandleTrackerUpdated;
			}
		}

		VisualElementPackager Packager { get; set; }

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				OnRegisterEffect(platformEffect);
		}

		public UIElement ContainerElement
		{
			get { return this; }
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

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Children.Count == 0)
				return new SizeRequest();

			var constraint = new System.Windows.Size(widthConstraint, heightConstraint);
			var child = (FrameworkElement)Children[0];

			child.Measure(constraint);
			var result = new Size(Math.Ceiling(child.DesiredSize.Width), Math.Ceiling(child.DesiredSize.Height));

			return new SizeRequest(result);
		}

		public void SetElement(VisualElement element)
		{
			TElement oldElement = Element;
			Element = (TElement)element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				oldElement.FocusChangeRequested -= OnModelFocusChangeRequested;
			}

			Element.PropertyChanged += OnElementPropertyChanged;
			Element.FocusChangeRequested += OnModelFocusChangeRequested;

			if (AutoPackage && Packager == null)
				Packager = new VisualElementPackager(this);

			if (AutoTrack && Tracker == null)
			{
				Tracker = new VisualElementTracker<TElement, FrameworkElement> { Model = Element, Element = this };
			}

			// Disabled until reason for crashes with unhandled exceptions is discovered
			// Without this some layouts may end up with improper sizes, however their children
			// will position correctly
			//Loaded += (sender, args) => {
			if (Packager != null)
				Packager.Load();
			//};

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));

			var controller = (IElementController)oldElement;
			if (controller != null && controller.EffectControlProvider == this)
				controller.EffectControlProvider = null;

			controller = element;
			if (controller != null)
				controller.EffectControlProvider = this;
		}

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;

		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			if (Element == null || finalSize.Width * finalSize.Height == 0)
				return finalSize;

			Element.IsInNativeLayout = true;

			if (Control != null)
			{
				Control.Measure(finalSize);
				Control.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
			}

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
			}

			Element.IsInNativeLayout = false;

			return finalSize;
		}

		protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
		{
			if (Element == null || availableSize.Width * availableSize.Height == 0)
				return new System.Windows.Size(0, 0);

			Element.IsInNativeLayout = true;

			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				var child = ElementController.LogicalChildren[i] as VisualElement;
				if (child == null)
					continue;
				IVisualElementRenderer renderer = Platform.GetRenderer(child);
				if (renderer == null)
					continue;

				try
				{
					renderer.ContainerElement.Measure(availableSize);
				}
				catch (NullReferenceException)
				{
					if (!IsExpectedTabbedPageMeasurementException(renderer))
						throw;
				}
			}

			double width = Math.Max(0, Element.Width);
			double height = Math.Max(0, Element.Height);
			var result = new System.Windows.Size(width, height);

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
				Control.Measure(new System.Windows.Size(w, h));
			}

			Element.IsInNativeLayout = false;

			return result;
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
			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		protected virtual void OnGotFocus(object sender, RoutedEventArgs args)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		protected virtual void OnLostFocus(object sender, RoutedEventArgs args)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.SetContainer(this);
			effect.SetControl(Control);
		}

		protected void SetNativeControl(TNativeElement element)
		{
			Control = element;

			Children.Add(element);
			Element.IsNativeStateConsistent = false;
			element.Loaded += (sender, args) => Element.IsNativeStateConsistent = true;

			element.GotFocus += OnGotFocus;
			element.LostFocus += OnLostFocus;

			_tracker.Child = element;

			UpdateBackgroundColor();
		}

		protected virtual void UpdateBackgroundColor()
		{
			var control = Control as Control;
			if (_initialBrush == null)
				_initialBrush = control == null ? Background : control.Background;
			if (control != null)
				control.Background = Element.BackgroundColor != Color.Default ? Element.BackgroundColor.ToBrush() : _initialBrush;
			else
				Background = Element.BackgroundColor != Color.Default ? Element.BackgroundColor.ToBrush() : _initialBrush;
		}

		protected virtual void UpdateNativeWidget()
		{
			UpdateEnabled();
		}

		internal virtual void OnModelFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			var control = Control as Control;
			if (control == null)
				return;

			if (args.Focus)
				args.Result = control.Focus();
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

		void HandleTrackerUpdated(object sender, EventArgs e)
		{
			UpdateNativeWidget();
		}

		static bool IsExpectedTabbedPageMeasurementException(IVisualElementRenderer renderer)
		{
			// The TabbedPageRenderer's underlying Pivot control throws a NRE if the tabbed page
			// does not have any toolbar items and it's measured during an animated transition 
			// from a page which does have toolbar items 

			// The NRE happens before TabbedPageRenderer's MeasureOverride is even called,
			// so unfortunately we have to handle it here

			var tpr = renderer.ContainerElement as TabbedPageRenderer;

			var tp = tpr?.Element as TabbedPage;

			return tp?.ToolbarItems.Count == 0;
		}

		void UpdateEnabled()
		{
			if (Control is Control)
				(Control as Control).IsEnabled = Element.IsEnabled;
		}
	}
}