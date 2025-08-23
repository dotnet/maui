using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsToggled(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			toggleSwitch?.IsOn = view?.IsOn ?? false;
		}

		static readonly string[] toggleSwitchOnKeys =
		{
			"ToggleSwitchFillOn",
			"ToggleSwitchFillOnPointerOver",
			"ToggleSwitchFillOnPressed",
			"ToggleSwitchFillOnDisabled"
		};

		static readonly string[] toggleSwitchOffKeys =
		{
			"ToggleSwitchFillOff",
			"ToggleSwitchFillOffPointerOver",
			"ToggleSwitchFillOffPressed",
			"ToggleSwitchFillOffDisabled"
		};

		public static void UpdateTrackColor(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			if (toggleSwitch is null)
			{
				return;
			}

			var trackColor = view.TrackColor?.ToPlatform();

			if (trackColor is not null)
			{
				if (view.IsOn)
				{
					toggleSwitch.Resources.SetValueForAllKey(toggleSwitchOnKeys, trackColor);
				}
				else
				{
					toggleSwitch.Resources.SetValueForAllKey(toggleSwitchOffKeys, trackColor);
				}
			}
			else
			{
				if (view.IsOn)
				{
					toggleSwitch.Resources.RemoveKeys(toggleSwitchOnKeys);
				}
				else
				{
					toggleSwitch.Resources.RemoveKeys(toggleSwitchOffKeys);
				}
			}

			toggleSwitch.RefreshThemeResources();
		}

		public static void UpdateThumbColor(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			if (toggleSwitch is null)
			{
				return;
			}

			if (view.ThumbColor is null)
			{
				return;
			}

			if (view.ThumbColor is not null)
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