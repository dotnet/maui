using System;
using ElmSharp;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;
using EColor = ElmSharp.Color;
using Specific = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement;
using SpecificSwitch = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Switch;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
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
			((IVisualElementController)Element).PlatformSizeChanged();
			UpdateBackgroundColor(false);
			UpdateOnColor(false);
			UpdateColor();
		}

		protected virtual void UpdateColor()
		{
			var color = SpecificSwitch.GetColor(Element);
			if (color != null)
			{
				Control.Color = color.ToPlatformEFL();
			}
		}

		protected void UpdateOnColor(bool initialize)
		{
			if (initialize && Element.OnColor.IsDefault())
				return;

			if (Element.OnColor.IsDefault())
			{
				Control.DeleteOnColors();
			}
			else
			{
				Control.SetOnColors(Element.OnColor.ToPlatformEFL());
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
