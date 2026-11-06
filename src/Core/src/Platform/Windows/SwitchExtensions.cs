using System.Runtime.CompilerServices;
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
				var hoverBrush = trackColor is SolidColorBrush brush ? new SolidColorBrush(brush.Color) { Opacity = 0.9 } : null;
				UpdateOnPointerOverResource(toggleSwitch.Resources, "Default", hoverBrush);
				if (toggleSwitch.Resources.ThemeDictionaries.ContainsKey("Light"))
					UpdateOnPointerOverResource(toggleSwitch.Resources, "Light", hoverBrush);
				if (toggleSwitch.Resources.ThemeDictionaries.ContainsKey("Dark"))
					UpdateOnPointerOverResource(toggleSwitch.Resources, "Dark", hoverBrush);
				UpdateOnPointerOverResource(toggleSwitch.Resources, "HighContrast", trackColor);
			}

			toggleSwitch.RefreshThemeResources();
		}

		static readonly ConditionalWeakTable<ResourceDictionary, HoverResourceState> s_hoverResourceStates = new();

		sealed class HoverResourceState
		{
			public bool CreatedDictionary { get; set; }
			public bool HasOriginalValue { get; set; }
			public object? OriginalValue { get; set; }
			public Brush? AppliedBrush { get; set; }
		}

		static void UpdateOnPointerOverResource(ResourceDictionary resources, string theme, Brush? brush)
		{
			const string key = "ToggleSwitchFillOnPointerOver";
			var themeResources = resources.ThemeDictionaries.TryGetValue(theme, out var dictionary)
				? (ResourceDictionary)dictionary
				: null;
			bool createdDictionary = themeResources is null;

			if (themeResources is null)
			{
				if (brush is null)
					return;

				themeResources = new ResourceDictionary();
				resources.ThemeDictionaries[theme] = themeResources;
			}

			s_hoverResourceStates.TryGetValue(themeResources, out var state);
			if (brush is null && state is null)
				return;

			bool hasCurrentValue = false;
			object? currentValue = null;
			// ResourceDictionary lookup can return fallback resources; only capture local entries.
			foreach (var entry in themeResources)
			{
				if (key.Equals(entry.Key))
				{
					hasCurrentValue = true;
					currentValue = entry.Value;
					break;
				}
			}

			if (brush is null)
			{
				if (state is not null && hasCurrentValue && ReferenceEquals(currentValue, state.AppliedBrush))
				{
					if (state.HasOriginalValue)
						themeResources[key] = state.OriginalValue;
					else
						themeResources.Remove(key);

					if (state.CreatedDictionary &&
						themeResources.Count == 0 &&
						themeResources.MergedDictionaries.Count == 0 &&
						themeResources.ThemeDictionaries.Count == 0 &&
						themeResources.Source is null)
					{
						resources.ThemeDictionaries.Remove(theme);
					}
				}

				s_hoverResourceStates.Remove(themeResources);
				return;
			}

			if (state is null)
			{
				state = new HoverResourceState
				{
					CreatedDictionary = createdDictionary,
					HasOriginalValue = hasCurrentValue,
					OriginalValue = currentValue
				};
				s_hoverResourceStates.Add(themeResources, state);
			}
			else if (!hasCurrentValue || !ReferenceEquals(currentValue, state.AppliedBrush))
			{
				state.HasOriginalValue = hasCurrentValue;
				state.OriginalValue = currentValue;
			}

			state.AppliedBrush = brush;
			themeResources[key] = brush;
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