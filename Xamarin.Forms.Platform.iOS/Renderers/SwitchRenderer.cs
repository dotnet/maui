using System;
using System.ComponentModel;
using System.Drawing;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class SwitchRenderer : ViewRenderer<Switch, UISwitch>
	{
		UIColor _defaultOnColor;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Control.ValueChanged -= OnControlValueChanged;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			if (e.OldElement != null)
				e.OldElement.Toggled -= OnElementToggled;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new UISwitch(RectangleF.Empty));
					Control.ValueChanged += OnControlValueChanged;
				}

				_defaultOnColor = Control.OnTintColor;
				Control.On = Element.IsToggled;
				e.NewElement.Toggled += OnElementToggled;
				UpdateOnColor();
			}

			base.OnElementChanged(e);
		}

		void UpdateOnColor()
		{
			if (Element != null)
			{
				if (Element.OnColor == Color.Default)
					Control.OnTintColor = _defaultOnColor;
				else
					Control.OnTintColor = Element.OnColor.ToUIColor();
			}
		}

		void OnControlValueChanged(object sender, EventArgs e)
		{
			((IElementController)Element).SetValueFromRenderer(Switch.IsToggledProperty, Control.On);
		}

		void OnElementToggled(object sender, EventArgs e)
		{
			Control.SetState(Element.IsToggled, true);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Switch.OnColorProperty.PropertyName)
				UpdateOnColor();
		}
	}
}