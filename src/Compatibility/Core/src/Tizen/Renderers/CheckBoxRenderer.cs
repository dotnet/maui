using System;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class CheckBoxRenderer : ViewRenderer<CheckBox, Check>
	{
		public CheckBoxRenderer()
		{
			RegisterPropertyHandler(CheckBox.IsCheckedProperty, UpdateIsChecked);
			RegisterPropertyHandler(CheckBox.ColorProperty, UpdateOnColor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Check(Forms.NativeParent));
				Control.StateChanged += OnStateChanged;
			}
			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.StateChanged -= OnStateChanged;
				}
			}
			base.Dispose(disposing);
		}

		void OnStateChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(CheckBox.IsCheckedProperty, Control.IsChecked);
		}

		void UpdateIsChecked()
		{
			Control.IsChecked = Element.IsChecked;
		}

		void UpdateOnColor(bool initialize)
		{
			if (initialize && Element.Color.IsDefault)
				return;

			if (Element.Color.IsDefault)
			{
				Control.DeleteOnColors();
			}
			else
			{
				Control.SetOnColors(Element.Color.ToNative());
			}
		}
	}
}
