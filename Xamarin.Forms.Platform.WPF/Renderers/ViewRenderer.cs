using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WControl = System.Windows.Controls.Control;

namespace Xamarin.Forms.Platform.WPF
{
	public class DefaultViewRenderer : ViewRenderer<View, UserControl>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged(e);
			SetNativeControl(new UserControl());
		}
	}

	public class ViewRenderer<TElement, TNativeElement> : IVisualElementRenderer, IEffectControlProvider
		where TElement : VisualElement where TNativeElement : FrameworkElement
	{
		readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers =
			new List<EventHandler<VisualElementChangedEventArgs>>();

		VisualElementTracker _tracker;

		IElementController ElementController => Element as IElementController;

		public TNativeElement Control { get; private set; }

		public TElement Element { get; private set; }

		protected virtual bool AutoTrack { get; set; } = true;

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
		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			PlatformEffect platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				OnRegisterEffect(platformEffect);
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		public FrameworkElement GetNativeElement()
		{
			return Control;
		}

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add { _elementChangedHandlers.Add(value); }
			remove { _elementChangedHandlers.Remove(value); }
		}

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Control == null)
				return new SizeRequest();

			var constraint = new System.Windows.Size(widthConstraint, heightConstraint);

			Control.Measure(constraint);

			return new SizeRequest(new Size(Math.Ceiling(Control.DesiredSize.Width), Math.Ceiling(Control.DesiredSize.Height)));
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

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, Element));

			var controller = (IElementController)oldElement;
			if (controller != null && controller.EffectControlProvider == this)
				controller.EffectControlProvider = null;

			controller = element;
			if (controller != null)
				controller.EffectControlProvider = this;
		}

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;

		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);
			for (var i = 0; i < _elementChangedHandlers.Count; i++)
				_elementChangedHandlers[i](this, args);
			
			ElementChanged?.Invoke(this, e);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == Frame.HeightProperty.PropertyName)
				UpdateHeight();
			else if (e.PropertyName == Frame.WidthProperty.PropertyName)
				UpdateWidth();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == View.HorizontalOptionsProperty.PropertyName || e.PropertyName == View.VerticalOptionsProperty.PropertyName)
				UpdateAlignment();
		}

		protected virtual void OnGotFocus(object sender, RoutedEventArgs args)
		{
			((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		protected virtual void OnLostFocus(object sender, RoutedEventArgs args)
		{
			((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.SetControl(Control);
		}

		protected void SetNativeControl(TNativeElement native)
		{
			Control = native;

			if (AutoTrack && Tracker == null)
				Tracker = new VisualElementTracker<TElement, FrameworkElement> { Element = Element, Control = Control };

			Element.IsNativeStateConsistent = false;

			Control.Loaded += (sender, e) =>
			{
				Element.IsNativeStateConsistent = true;
				Appearing();
			};
			Control.Unloaded += (sender, e) => { Disappearing(); };

			Control.GotFocus += OnGotFocus;
			Control.LostFocus += OnLostFocus;

			UpdateBackground();
			UpdateAlignment();
		}

		protected virtual void Appearing()
		{

		}

		protected virtual void Disappearing()
		{

		}

		protected virtual void UpdateBackground()
		{
			var control = Control as WControl;
			if (control == null)
				return;

			control.UpdateDependencyColor(WControl.BackgroundProperty, Element.BackgroundColor);
		}

		protected virtual void UpdateHeight()
		{
			if (Control == null || Element == null)
				return;
			
			Control.Height = Element.Height > 0 ? Element.Height : Double.NaN;
		}

		protected virtual void UpdateWidth()
		{
			if (Control == null || Element == null)
				return;

			Control.Width = Element.Width > 0 ? Element.Width : Double.NaN;
		}
		
		protected virtual void UpdateNativeWidget()
		{
			UpdateEnabled();
		}

		internal virtual void OnModelFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			var control = Control as WControl;
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

		internal void UnfocusControl(WControl control)
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

		protected virtual void UpdateEnabled()
		{
			WControl wcontrol = Control as WControl;
			if (wcontrol != null)
				wcontrol.IsEnabled = Element.IsEnabled;
		}

		void UpdateAlignment()
		{
			View view = Element as View;
			if (view != null)
			{
				Control.HorizontalAlignment = view.HorizontalOptions.ToNativeHorizontalAlignment();
				Control.VerticalAlignment = view.VerticalOptions.ToNativeVerticalAlignment();
			}
		}

		bool _disposed;

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
				return;

			_disposed = true;

			if (Control != null)
			{
				//Console.WriteLine("Dispose : " + this.Control.GetType());
				Control.GotFocus -= OnGotFocus;
				Control.LostFocus -= OnLostFocus;
			}

			if (Element != null)
			{
				Element.PropertyChanged -= OnElementPropertyChanged;
				Element.FocusChangeRequested -= OnModelFocusChangeRequested;
			}

			Tracker = null;
		}
	}
}
