using System;
using System.Drawing;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class SwitchRenderer : ViewRenderer<Switch, UISwitch>
	{
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

				Control.On = Element.IsToggled;
				e.NewElement.Toggled += OnElementToggled;
			}

			base.OnElementChanged(e);
		}

		void OnControlValueChanged(object sender, EventArgs e)
		{
			((IElementController)Element).SetValueFromRenderer(Switch.IsToggledProperty, Control.On);
		}

		void OnElementToggled(object sender, EventArgs e)
		{
			Control.SetState(Element.IsToggled, true);
		}
	}
}