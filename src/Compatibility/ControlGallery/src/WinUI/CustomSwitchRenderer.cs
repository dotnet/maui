using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using WResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;

[assembly: ExportRenderer(typeof(CustomSwitch), typeof(CustomSwitchRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	// Used in Issue7253.cs
	public class CustomSwitchRenderer : SwitchRenderer
	{
		protected CustomSwitch CustomSwitch => Element as CustomSwitch;

		protected CustomSwitchStyle CustomStyle { get; set; }

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
				return;

			Control.OnContent = null;
			Control.OffContent = null;
			CustomStyle = new CustomSwitchStyle();
			Control.Resources = CustomStyle;

			OnElementPropertyChanged(this, new PropertyChangedEventArgs(nameof(CustomSwitch.CustomColor)));
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(CustomSwitch.CustomColor):
					CustomStyle.ToggleSwitchStrokeOn(CustomSwitch.CustomColor.ToUwpColor());
					CustomStyle.ToggleSwitchStrokeOff(CustomSwitch.CustomColor.ToUwpColor());
					CustomStyle.ToggleSwitchKnobFillOn(CustomSwitch.CustomColor.ToUwpColor());
					CustomStyle.ToggleSwitchKnobFillOff(CustomSwitch.CustomColor.ToUwpColor());
					CustomStyle.ToggleSwitchStrokeOnPointerOver(CustomSwitch.CustomColor.ToUwpColor());
					CustomStyle.ToggleSwitchStrokeOffPointerOver(CustomSwitch.CustomColor.ToUwpColor());
					CustomStyle.ToggleSwitchKnobFillOffPointerOver(CustomSwitch.CustomColor.ToUwpColor());
					CustomStyle.ToggleSwitchKnobFillOnPointerOver(CustomSwitch.CustomColor.ToUwpColor());
					break;
			}

			base.OnElementPropertyChanged(sender, e);
		}

		protected class CustomSwitchStyle : WResourceDictionary
		{
			public void ToggleSwitchStrokeOn(global::Windows.UI.Color c) => this["ToggleSwitchStrokeOn"] = c;
			public void ToggleSwitchStrokeOff(Windows.UI.Color c) => this["ToggleSwitchStrokeOff"] = c;
			public void ToggleSwitchKnobFillOn(Windows.UI.Color c) => this["ToggleSwitchKnobFillOn"] = c;
			public void ToggleSwitchKnobFillOff(Windows.UI.Color c) => this["ToggleSwitchKnobFillOff"] = c;
			public void ToggleSwitchStrokeOnPointerOver(Windows.UI.Color c) => this["ToggleSwitchStrokeOnPointerOver"] = c;
			public void ToggleSwitchStrokeOffPointerOver(Windows.UI.Color c) => this["ToggleSwitchStrokeOffPointerOver"] = c;
			public void ToggleSwitchKnobFillOffPointerOver(Windows.UI.Color c) => this["ToggleSwitchKnobFillOffPointerOver"] = c;
			public void ToggleSwitchKnobFillOnPointerOver(Windows.UI.Color c) => this["ToggleSwitchKnobFillOnPointerOver"] = c;

		}
	}

	public static class ColorHelper
	{
		public static Windows.UI.Color ToUwpColor(this Color xColor) =>
			Windows.UI.Color.FromArgb((byte)(xColor.Alpha * 255), (byte)(xColor.Red * 255), (byte)(xColor.Green * 255), (byte)(xColor.Blue * 255));
	}
}