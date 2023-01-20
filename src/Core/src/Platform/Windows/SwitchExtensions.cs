using Microsoft.UI.Xaml.Controls;
using WResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

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

		public static void UpdateTrackColor(this ToggleSwitch toggleSwitch, ISwitch view)
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
					view.TrackColor.ToPlatform(),
					"ToggleSwitchFillOn",
					"ToggleSwitchFillOnPointerOver",
					"ToggleSwitchFillOnPressed",
					"ToggleSwitchFillOnDisabled");
			}
		}

		public static void UpdateThumbColor(this ToggleSwitch toggleSwitch, ISwitch view)
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
					view.ThumbColor.ToPlatform(),
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