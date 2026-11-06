using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsToggled(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			if (toggleSwitch is not null)
			{
				toggleSwitch.IsOn = view?.IsOn ?? false;
			}
		}

		static readonly string[] toggleSwitchOnKeys =
		{
			"ToggleSwitchFillOn",
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

			if (view.IsOn)
			{
				// Match WinUI's hover opacity without reducing contrast in high-contrast themes.
				UpdateOnPointerOverResource(toggleSwitch.Resources, "Default",
					trackColor is SolidColorBrush brush ? new SolidColorBrush(brush.Color) { Opacity = 0.9 } : null);
				UpdateOnPointerOverResource(toggleSwitch.Resources, "HighContrast", trackColor);
			}

			toggleSwitch.RefreshThemeResources();
		}

		static void UpdateOnPointerOverResource(ResourceDictionary resources, string theme, Brush brush)
		{
			const string key = "ToggleSwitchFillOnPointerOver";
			var themeResources = resources.ThemeDictionaries.TryGetValue(theme, out var dictionary)
				? (ResourceDictionary)dictionary
				: null;

			if (brush is null)
			{
				themeResources?.Remove(key);
			}
			else
			{
				if (themeResources is null)
				{
					themeResources = new ResourceDictionary();
					resources.ThemeDictionaries[theme] = themeResources;
				}

				themeResources[key] = brush;
			}
		}

		static readonly string[] toggleSwitchKnobKeys =
		{
			"ToggleSwitchKnobFillOnPointerOver",
			"ToggleSwitchKnobFillOn",
			"ToggleSwitchKnobFillOnPressed",
			"ToggleSwitchKnobFillOnDisabled",
			"ToggleSwitchKnobFillOffPointerOver",
			"ToggleSwitchKnobFillOff",
			"ToggleSwitchKnobFillOffPressed",
			"ToggleSwitchKnobFillOffDisabled"
		};

		public static void UpdateThumbColor(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			if (toggleSwitch is null)
			{
				return;
			}

			if (view.ThumbColor is not null)
			{
				toggleSwitch.TryUpdateResource(view.ThumbColor.ToPlatform(), toggleSwitchKnobKeys);
			}
			else
			{
				toggleSwitch.Resources.RemoveKeys(toggleSwitchKnobKeys);
				toggleSwitch.RefreshThemeResources();
			}
		}

		internal static void UpdateMinWidth(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			double minWidth = view.MinimumWidth;
			if (double.IsNaN(minWidth))
			{
				toggleSwitch.MinWidth = 0;
			}
			else
			{
				toggleSwitch.MinWidth = minWidth;
			}
		}
	}
}