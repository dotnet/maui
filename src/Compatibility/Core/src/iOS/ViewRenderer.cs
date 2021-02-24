using System;
using System.ComponentModel;

using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

#if __MOBILE__
using UIKit;
using NativeColor = UIKit.UIColor;
using NativeControl = UIKit.UIControl;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using NativeView = AppKit.NSView;
using NativeColor = CoreGraphics.CGColor;
using NativeControl = AppKit.NSControl;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public interface ITabStop
	{
		NativeView TabStop { get; }
	}

	public abstract class ViewRenderer : ViewRenderer<View, NativeView>
	{
	}

	public abstract class ViewRenderer<TView, TNativeView> : VisualElementRenderer<TView>, IVisualNativeElementRenderer, ITabStop where TView : View where TNativeView : NativeView
	{
		string _defaultAccessibilityLabel;
		string _defaultAccessibilityHint;
		bool? _defaultIsAccessibilityElement;

		NativeColor _defaultColor;

		event EventHandler<PropertyChangedEventArgs> _elementPropertyChanged;
		event EventHandler _controlChanging;
		event EventHandler _controlChanged;

		private protected bool IsElementOrControlEmpty => Element == null || Control == null;

		protected virtual TNativeView CreateNativeControl()
		{
			return default(TNativeView);
		}

		public TNativeView Control { get; private set; }
		NativeView IVisualNativeElementRenderer.Control => Control;


		event EventHandler<PropertyChangedEventArgs> IVisualNativeElementRenderer.ElementPropertyChanged
		{
			add { _elementPropertyChanged += value; }
			remove { _elementPropertyChanged -= value; }
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


		NativeView ITabStop.TabStop => Control;
#if __MOBILE__
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (Control != null && Element != null)
				Control.Frame = new RectangleF(0, 0, (nfloat)Element.Width, (nfloat)Element.Height);
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			if (Control == null)
				return (base.SizeThatFits(size));

			return Control.SizeThatFits(size);
		}
#else
		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return (Control ?? NativeView).GetSizeRequest(widthConstraint, heightConstraint);
		}

		public override void Layout()
		{
			if (Control != null)
				Control.Frame = new RectangleF(0, 0, (nfloat)Element.Width, (nfloat)Element.Height);
			base.Layout();
		}
#endif

		/// <summary>
		/// Determines whether the native control is disposed of when this renderer is disposed
		/// Can be overridden in deriving classes 
		/// </summary>
		protected virtual bool ManageNativeControlLifetime => true;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_elementPropertyChanged = null;
				_controlChanging = null;
				_controlChanged = null;

				if (Control != null && ManageNativeControlLifetime)
				{
					Control.RemoveFromSuperview();
					Control.Dispose();
					Control = null;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
				e.OldElement.FocusChangeRequested -= ViewOnFocusChangeRequested;

			if (e.NewElement != null)
			{
				if (Control != null && e.OldElement != null && e.OldElement.BackgroundColor != e.NewElement.BackgroundColor || e.NewElement.BackgroundColor != Color.Default)
					SetBackgroundColor(e.NewElement.BackgroundColor);

				if(Control != null && e.OldElement != null && e.OldElement.Background != e.NewElement.Background)
					SetBackground(e.NewElement.Background);

				e.NewElement.FocusChangeRequested += ViewOnFocusChangeRequested;
			}

			UpdateIsEnabled();
			UpdateFlowDirection();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Control != null)
			{
				if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
					UpdateIsEnabled();
				else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
					SetBackgroundColor(Element.BackgroundColor);
				else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
					SetBackground(Element.Background);
				else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
					UpdateFlowDirection();
			}

			base.OnElementPropertyChanged(sender, e);
			_elementPropertyChanged?.Invoke(this, e);
		}

		protected override void OnRegisterEffect(PlatformEffect effect)
		{
			base.OnRegisterEffect(effect);
			effect.SetControl(Control);
		}

		protected override void SetAccessibilityHint()
		{
			_defaultAccessibilityHint = Control.SetAccessibilityHint(Element, _defaultAccessibilityHint);
		}

		protected override void SetAccessibilityLabel()
		{
			_defaultAccessibilityLabel = Control.SetAccessibilityLabel(Element, _defaultAccessibilityLabel);
		}

		protected override void SetIsAccessibilityElement()
		{
			_defaultIsAccessibilityElement = Control.SetIsAccessibilityElement(Element, _defaultIsAccessibilityElement);
		}

		protected override void SetAutomationId(string id)
		{
			if (Control == null)
				base.SetAutomationId(id);
			else
			{
				AccessibilityIdentifier = id + "_Container";
				Control.AccessibilityIdentifier = id;
			}
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (IsElementOrControlEmpty)
				return;
#if __MOBILE__
			if (color == Color.Default)
				Control.BackgroundColor = _defaultColor;
			else
				Control.BackgroundColor = color.ToUIColor();
#else
			Control.Layer.BackgroundColor = color == Color.Default ? _defaultColor : color.ToCGColor();
#endif
		}

		protected override void SetBackground(Brush brush)
		{
			if (IsElementOrControlEmpty)
				return;

			Control.UpdateBackground(brush);
		}

		protected void SetNativeControl(TNativeView uiview)
		{
			_controlChanging?.Invoke(this, EventArgs.Empty);
#if __MOBILE__
			_defaultColor = uiview.BackgroundColor;
#else
			uiview.WantsLayer = true;
			_defaultColor = uiview.Layer.BackgroundColor;
#endif
			Control = uiview;

			UpdateBackgroundColor();

			UpdateBackground();

			UpdateIsEnabled();

			UpdateFlowDirection();

			AddSubview(uiview);

			_controlChanged?.Invoke(this, EventArgs.Empty);
		}

#if __MOBILE__
		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
			base.TraitCollectionDidChange(previousTraitCollection);
			// Make sure the control adheres to changes in UI theme
			if (Forms.IsiOS13OrNewer && previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
				Control?.SetNeedsDisplay();
		}

		internal override void SendVisualElementInitialized(VisualElement element, NativeView nativeView)
		{
			base.SendVisualElementInitialized(element, Control);
		}
#endif

		void UpdateBackgroundColor()
		{
			if (IsElementOrControlEmpty)
				return;

			if (Element.BackgroundColor != Color.Default)
				SetBackgroundColor(Element.BackgroundColor);
		}

		void UpdateBackground()
		{
			if (IsElementOrControlEmpty)
				return;

			Brush brush = Element.Background;

			if (!Brush.IsNullOrEmpty(brush))
				SetBackground(brush);
		}

		void UpdateIsEnabled()
		{
			if (IsElementOrControlEmpty)
				return;

			var uiControl = Control as NativeControl;
			if (uiControl == null)
				return;
			uiControl.Enabled = Element.IsEnabled;
		}

		void UpdateFlowDirection()
		{
			if (IsElementOrControlEmpty)
				return;

			Control.UpdateFlowDirection(Element);
		}

		void ViewOnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs focusRequestArgs)
		{
			if (Control == null)
				return;
				
#if __MOBILE__
			focusRequestArgs.Result = focusRequestArgs.Focus ? Control.BecomeFirstResponder() : Control.ResignFirstResponder();
#else
			focusRequestArgs.Result = focusRequestArgs.Focus ? Control.Window.MakeFirstResponder(Control) : Control.Window.MakeFirstResponder(null);
#endif
		}
	}
}
