using Microsoft.UI.Xaml.Controls;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsToggled(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			if (toggleSwitch != null)
			{
				toggleSwitch.IsOn = view?.IsOn ?? false;
			}
		}

		public static void UpdateTrackColor(this ToggleSwitch toggleSwitch, ISwitch view, WResourceDictionary? originalResources = null)
		{
			if (toggleSwitch == null)
			{
				return;
			}

			if (view.TrackColor == null)
			{
				return;
			}

			if (view.TrackColor != null)
			{
				toggleSwitch.TryUpdateResource(
					view.TrackColor.ToPlatform() ?? originalResources?["ToggleSwitchFillOff"] as WSolidColorBrush ?? new WSolidColorBrush(),
					"ToggleSwitchFillOn",
					"ToggleSwitchFillOnPointerOver",
					"ToggleSwitchFillOnPressed",
					"ToggleSwitchFillOnDisabled");
			}
		}

		public static void UpdateThumbColor(this ToggleSwitch toggleSwitch, ISwitch view, WResourceDictionary? originalResources = null)
		{
			if (toggleSwitch == null)
			{
				return;
			}

			if (view.ThumbColor == null)
			{
				return;
			}

			if (view.ThumbColor != null)
			{
				toggleSwitch.TryUpdateResource(
					view.ThumbColor.ToPlatform() ?? originalResources?["ToggleSwitchKnobFillOff"] as WSolidColorBrush ?? new WSolidColorBrush(),
					"ToggleSwitchKnobFillOnPointerOver",
					"ToggleSwitchKnobFillOn",
					"ToggleSwitchKnobFillOnPressed",
					"ToggleSwitchKnobFillOnDisabled",
					"ToggleSwitchKnobFillOffPointerOver",
					"ToggleSwitchKnobFillOff",
					"ToggleSwitchKnobFillOffPressed",
					"ToggleSwitchKnobFillOffDisabled");
			}
		}
	}
}