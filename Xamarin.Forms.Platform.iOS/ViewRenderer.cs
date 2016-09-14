using System;
using System.ComponentModel;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.iOS
{
	public abstract class ViewRenderer : ViewRenderer<View, UIView>
	{
	}

	public abstract class ViewRenderer<TView, TNativeView> : VisualElementRenderer<TView> where TView : View where TNativeView : UIView
	{
		UIColor _defaultColor;

		public TNativeView Control { get; private set; }

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (Control != null)
				Control.Frame = new RectangleF(0, 0, (nfloat)Element.Width, (nfloat)Element.Height);
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			if (Control == null)
				return (base.SizeThatFits(size));

			return Control.SizeThatFits(size);
		}

		/// <summary>
		/// Determines whether the native control is disposed of when this renderer is disposed
		/// Can be overridden in deriving classes 
		/// </summary>
		protected virtual bool ManageNativeControlLifetime => true;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && Control != null && ManageNativeControlLifetime)
			{
				Control.Dispose();
				Control = null;
			}
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

				e.NewElement.FocusChangeRequested += ViewOnFocusChangeRequested;
			}

			UpdateIsEnabled();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Control != null)
			{
				if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
					UpdateIsEnabled();
				else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
					SetBackgroundColor(Element.BackgroundColor);
			}

			base.OnElementPropertyChanged(sender, e);
		}

		protected override void OnRegisterEffect(PlatformEffect effect)
		{
			base.OnRegisterEffect(effect);
			effect.Control = Control;
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
			if (Control == null)
				return;

			if (color == Color.Default)
				Control.BackgroundColor = _defaultColor;
			else
				Control.BackgroundColor = color.ToUIColor();
		}

		protected void SetNativeControl(TNativeView uiview)
		{
			_defaultColor = uiview.BackgroundColor;
			Control = uiview;

			if (Element.BackgroundColor != Color.Default)
				SetBackgroundColor(Element.BackgroundColor);

			UpdateIsEnabled();

			AddSubview(uiview);
		}

		internal override void SendVisualElementInitialized(VisualElement element, UIView nativeView)
		{
			base.SendVisualElementInitialized(element, Control);
		}

		void UpdateIsEnabled()
		{
			if (Element == null || Control == null)
				return;

			var uiControl = Control as UIControl;
			if (uiControl == null)
				return;
			uiControl.Enabled = Element.IsEnabled;
		}

		void ViewOnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs focusRequestArgs)
		{
			if (Control == null)
				return;

			focusRequestArgs.Result = focusRequestArgs.Focus ? Control.BecomeFirstResponder() : Control.ResignFirstResponder();
		}
	}
}