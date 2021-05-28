using System;
using System.ComponentModel;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class SwitchRenderer : ViewRenderer<Switch, UISwitch>
	{
		UIColor _defaultOnColor;
		UIColor _defaultThumbColor;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public SwitchRenderer()
		{

		}

		[PortHandler]
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Control.ValueChanged -= OnControlValueChanged;

			base.Dispose(disposing);
		}

		[PortHandler]
		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			if (e.OldElement != null)
				e.OldElement.Toggled -= OnElementToggled;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new UISwitch(CGRect.Empty));
					Control.ValueChanged += OnControlValueChanged;
				}

				_defaultOnColor = UISwitch.Appearance.OnTintColor;
				_defaultThumbColor = UISwitch.Appearance.ThumbTintColor;
				Control.On = Element.IsToggled;
				e.NewElement.Toggled += OnElementToggled;
				UpdateOnColor();
				UpdateThumbColor();
			}

			base.OnElementChanged(e);
		}

		[PortHandler]
		void UpdateOnColor()
		{
			if (Element != null)
			{
				if (Element.OnColor == null)
					Control.OnTintColor = _defaultOnColor;
				else
					Control.OnTintColor = Element.OnColor.ToUIColor();
			}
		}

		[PortHandler]
		void UpdateThumbColor()
		{
			if (Element == null)
				return;

			Color thumbColor = Element.ThumbColor;
			Control.ThumbTintColor = thumbColor?.ToUIColor() ?? _defaultThumbColor;
		}

		[PortHandler]
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
			if (e.PropertyName == Switch.ThumbColorProperty.PropertyName)
				UpdateThumbColor();
		}
	}
}