using System.Collections.Generic;
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
				UpdateOnPointerOverResources(toggleSwitch, trackColor);
			}

			toggleSwitch.RefreshThemeResources();
		}

		const string OnPointerOverResourceKey = "ToggleSwitchFillOnPointerOver";

		static readonly ConditionalWeakTable<ToggleSwitch, SwitchHoverResourceState> s_switchHoverResourceStates = new();

		sealed class SwitchHoverResourceState
		{
			public SwitchHoverResourceState(ResourceDictionary owner) => Owner = owner;

			// Retain resource wrappers until cleanup, without keeping the switch alive.
			public ResourceDictionary Owner { get; }
			public ResourceDictionary Resources { get; } = new();
			public Dictionary<string, HoverResourceState> ThemeStates { get; } = new();
			public bool HasOriginalDirectValue { get; set; }
			public object? OriginalDirectValue { get; set; }
		}

		static void UpdateOnPointerOverResources(ToggleSwitch toggleSwitch, Brush? trackColor)
		{
			var resources = toggleSwitch.Resources;
			if (s_switchHoverResourceStates.TryGetValue(toggleSwitch, out var state) && !ReferenceEquals(state.Owner, resources))
			{
				s_switchHoverResourceStates.Remove(toggleSwitch);
				state = null;
			}

			if (state is null)
			{
				if (trackColor is null)
					return;

				state = new SwitchHoverResourceState(resources);
				s_switchHoverResourceStates.Add(toggleSwitch, state);
			}

			bool hasDirectValue = TryGetLocalHoverResource(resources, out var directValue);
			if (trackColor is null)
			{
				if (state.HasOriginalDirectValue && !hasDirectValue)
					resources[OnPointerOverResourceKey] = state.OriginalDirectValue;

				UpdateOnPointerOverResource(state, "Default", null);
				UpdateOnPointerOverResource(state, "HighContrast", null);
				if (IsEmpty(state.Resources))
					resources.MergedDictionaries.Remove(state.Resources);

				s_switchHoverResourceStates.Remove(toggleSwitch);
				return;
			}

			// Direct entries precede merged resources; leave application theme dictionaries untouched.
			if (hasDirectValue)
			{
				state.HasOriginalDirectValue = true;
				state.OriginalDirectValue = directValue;
				resources.Remove(OnPointerOverResourceKey);
			}

			UpdateOnPointerOverResource(state, "Default",
				trackColor is SolidColorBrush brush ? new SolidColorBrush(brush.Color) { Opacity = 0.9 } : null);
			UpdateOnPointerOverResource(state, "HighContrast", trackColor);
			var mergedDictionaries = resources.MergedDictionaries;
			if (mergedDictionaries.Count == 0 || !ReferenceEquals(mergedDictionaries[mergedDictionaries.Count - 1], state.Resources))
			{
				mergedDictionaries.Remove(state.Resources);
				mergedDictionaries.Add(state.Resources);
			}
		}

		sealed class HoverResourceState
		{
			public HoverResourceState(ResourceDictionary resources) => Resources = resources;

			public ResourceDictionary Resources { get; }
			public bool CreatedDictionary { get; set; }
			public bool HasOriginalValue { get; set; }
			public object? OriginalValue { get; set; }
			public Brush? AppliedBrush { get; set; }
		}

		static void UpdateOnPointerOverResource(SwitchHoverResourceState switchState, string theme, Brush? brush)
		{
			var resources = switchState.Resources;
			var themeResources = resources.ThemeDictionaries.TryGetValue(theme, out var dictionary)
				? (ResourceDictionary)dictionary
				: null;
			bool createdDictionary = themeResources is null;

			if (themeResources is null)
			{
				if (brush is null)
				{
					switchState.ThemeStates.Remove(theme);
					return;
				}

				themeResources = new ResourceDictionary();
				resources.ThemeDictionaries[theme] = themeResources;
			}

			if (switchState.ThemeStates.TryGetValue(theme, out var state) && !ReferenceEquals(state.Resources, themeResources))
			{
				switchState.ThemeStates.Remove(theme);
				state = null;
			}
			if (brush is null && state is null)
				return;

			bool hasCurrentValue = TryGetLocalHoverResource(themeResources, out var currentValue);

			if (brush is null)
			{
				if (state is not null && hasCurrentValue && ReferenceEquals(currentValue, state.AppliedBrush))
				{
					if (state.HasOriginalValue)
						themeResources[OnPointerOverResourceKey] = state.OriginalValue;
					else
						themeResources.Remove(OnPointerOverResourceKey);

					if (state.CreatedDictionary && IsEmpty(themeResources))
					{
						resources.ThemeDictionaries.Remove(theme);
					}
				}

				switchState.ThemeStates.Remove(theme);
				return;
			}

			if (state is null)
			{
				state = new HoverResourceState(themeResources)
				{
					CreatedDictionary = createdDictionary,
					HasOriginalValue = hasCurrentValue,
					OriginalValue = currentValue
				};
				switchState.ThemeStates[theme] = state;
			}
			else if (!hasCurrentValue || !ReferenceEquals(currentValue, state.AppliedBrush))
			{
				state.HasOriginalValue = hasCurrentValue;
				state.OriginalValue = currentValue;
			}

			state.AppliedBrush = brush;
			themeResources[OnPointerOverResourceKey] = brush;
		}

		static bool TryGetLocalHoverResource(ResourceDictionary resources, out object? value)
		{
			// ResourceDictionary lookup can return fallback resources; only capture local entries.
			foreach (var entry in resources)
			{
				if (OnPointerOverResourceKey.Equals(entry.Key))
				{
					value = entry.Value;
					return true;
				}
			}

			value = null;
			return false;
		}

		static bool IsEmpty(ResourceDictionary resources) =>
			resources.Count == 0 &&
			resources.MergedDictionaries.Count == 0 &&
			resources.ThemeDictionaries.Count == 0 &&
			resources.Source is null;

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