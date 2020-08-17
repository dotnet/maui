using System;
using ElmSharp;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.VisualElement;
using SpecificSwitch = Xamarin.Forms.PlatformConfiguration.TizenSpecific.Switch;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SwitchRenderer : ViewRenderer<Switch, Check>
	{
		public SwitchRenderer()
		{
			RegisterPropertyHandler(Switch.IsToggledProperty, HandleToggled);
			RegisterPropertyHandler(Switch.OnColorProperty, UpdateOnColor);
			RegisterPropertyHandler(SpecificSwitch.ColorProperty, UpdateColor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Check(Forms.NativeParent)
				{
					Style = SwitchStyle.Toggle
				});
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

		protected override void UpdateThemeStyle()
		{
			var style = Specific.GetStyle(Element);
			if (string.IsNullOrEmpty(style))
			{
				return;
			}
			switch (style)
			{
				case SwitchStyle.Toggle:
				case SwitchStyle.Favorite:
				case SwitchStyle.CheckBox:
				case SwitchStyle.OnOff:
				case SwitchStyle.Small:
					Control.Style = style;
					break;
				default:
					Control.Style = SwitchStyle.Toggle;
					break;
			}
			((IVisualElementController)Element).NativeSizeChanged();
			UpdateBackgroundColor(false);
			UpdateOnColor(false);
			UpdateColor();
		}

		protected virtual void UpdateColor()
		{
			var color = SpecificSwitch.GetColor(Element);
			if (color != Color.Default)
			{
				Control.Color = color.ToNative();
			}
		}

		protected void UpdateOnColor(bool initialize)
		{
			if (initialize && Element.OnColor.IsDefault)
				return;

			if (Element.OnColor.IsDefault)
			{
				Control.DeleteOnColors();
			}
			else
			{
				Control.SetOnColors(Element.OnColor.ToNative());
			}
		}

		void OnStateChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Switch.IsToggledProperty, Control.IsChecked);
		}

		void HandleToggled()
		{
			Control.IsChecked = Element.IsToggled;
		}

	}
}
