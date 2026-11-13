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
				UpdateOnPointerOverResources(toggleSwitch.Resources, trackColor);
			}

			toggleSwitch.RefreshThemeResources();
		}

		const string OnPointerOverResourceKey = "ToggleSwitchFillOnPointerOver";

		static readonly ConditionalWeakTable<ResourceDictionary, HoverResourceState> s_hoverResourceStates = new();

		// Native resources outlive their projected wrappers, so their state must be
		// owned by the merged dictionary rather than a weak wrapper key.
		sealed class SwitchHoverResourceState : ResourceDictionary
		{
			public bool HasOriginalDirectValue { get; set; }
			public object? OriginalDirectValue { get; set; }
			public HoverResourceState? DefaultThemeState;
			public HoverResourceState? HighContrastThemeState;
		}

		static void UpdateOnPointerOverResources(ResourceDictionary resources, Brush? trackColor)
		{
			SwitchHoverResourceState? state = null;
			var mergedDictionaries = resources.MergedDictionaries;
			for (int i = 0; i < mergedDictionaries.Count; i++)
			{
				if (mergedDictionaries[i] is SwitchHoverResourceState existing)
				{
					state = existing;
					break;
				}
			}

			if (state is null)
			{
				if (trackColor is null)
					return;

				state = new SwitchHoverResourceState();
				mergedDictionaries.Add(state);
			}

			bool hasDirectValue = TryGetLocalHoverResource(resources, out var directValue);
			if (trackColor is null)
			{
				if (state.HasOriginalDirectValue && !hasDirectValue)
					resources[OnPointerOverResourceKey] = state.OriginalDirectValue;

				UpdateOnPointerOverResource(state, "Default", null, ref state.DefaultThemeState);
				UpdateOnPointerOverResource(state, "HighContrast", null, ref state.HighContrastThemeState);
				if (IsEmpty(state))
					mergedDictionaries.Remove(state);

				state.HasOriginalDirectValue = false;
				state.OriginalDirectValue = null;
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
				trackColor is SolidColorBrush brush ? new SolidColorBrush(brush.Color) { Opacity = 0.9 } : null, ref state.DefaultThemeState);
			UpdateOnPointerOverResource(state, "HighContrast", trackColor, ref state.HighContrastThemeState);
			if (mergedDictionaries.Count == 0 || !ReferenceEquals(mergedDictionaries[mergedDictionaries.Count - 1], state))
			{
				mergedDictionaries.Remove(state);
				mergedDictionaries.Add(state);
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

		static void UpdateOnPointerOverResource(ResourceDictionary resources, string theme, Brush? brush, ref HoverResourceState? activeState)
		{
			var themeResources = resources.ThemeDictionaries.TryGetValue(theme, out var dictionary)
				? (ResourceDictionary)dictionary
				: null;
			bool createdDictionary = themeResources is null;

			if (themeResources is null)
			{
				if (brush is null)
				{
					activeState = null;
					return;
				}

				themeResources = new ResourceDictionary();
				resources.ThemeDictionaries[theme] = themeResources;
			}

			var state = activeState;
			if (state is null || !ReferenceEquals(state.Resources, themeResources))
				s_hoverResourceStates.TryGetValue(themeResources, out state);
			if (brush is null && state is null)
			{
				activeState = null;
				return;
			}

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

				s_hoverResourceStates.Remove(themeResources);
				activeState = null;
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
				s_hoverResourceStates.Add(themeResources, state);
			}
			else if (!hasCurrentValue || !ReferenceEquals(currentValue, state.AppliedBrush))
			{
				state.HasOriginalValue = hasCurrentValue;
				state.OriginalValue = currentValue;
			}

			state.AppliedBrush = brush;
			activeState = state;
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