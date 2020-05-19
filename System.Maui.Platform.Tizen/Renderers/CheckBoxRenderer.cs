using System;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace System.Maui.Platform.Tizen
{
	public class CheckBoxRenderer : ViewRenderer<CheckBox, Check>
	{
		readonly string[] _onColorParts;
		readonly string[] _onColorEdjeParts;

		public CheckBoxRenderer()
		{
			_onColorParts = Device.Idiom == TargetIdiom.Watch ? new string[] {"outer_bg_on", "outer_bg_on_pressed", "check_on", "check_on_pressed"} : new string[] {"bg_on", "stroke"};
			_onColorEdjeParts = new string[_onColorParts.Length];
			RegisterPropertyHandler(CheckBox.IsCheckedProperty, UpdateIsChecked);
			RegisterPropertyHandler(CheckBox.ColorProperty, UpdateOnColor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Check(System.Maui.Maui.NativeParent));
				Control.StateChanged += OnStateChanged;
				for (int i=0; i<_onColorParts.Length; i++)
				{
					_onColorEdjeParts[i] = Control.ClassName.ToLower().Replace("elm_", "") + "/" + _onColorParts[i];
				}
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
				foreach(string s in _onColorEdjeParts)
				{
					Control.EdjeObject.DeleteColorClass(s);
				}
			}
			else
			{
				EColor color = Element.Color.ToNative();
				foreach (string s in _onColorParts)
				{
					Control.SetPartColor(s, color);
				}
			}
		}
	}
}
