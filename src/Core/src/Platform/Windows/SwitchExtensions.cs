using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using Microsoft.UI.Xaml.Media;

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

			var trackColor = view.TrackColor?.ToPlatform() ?? new SolidColorBrush(Color.FromArgb(6, 0, 0, 0));

			toggleSwitch.TryUpdateResource(
				    trackColor,
					"ToggleSwitchFillOn",
					"ToggleSwitchFillOnPointerOver",
					"ToggleSwitchFillOnPressed",
					"ToggleSwitchFillOnDisabled",
					"ToggleSwitchFillOff",
					"ToggleSwitchFillOffPointerOver",
					"ToggleSwitchFillOffPressed",
					"ToggleSwitchFillOffDisabled");
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