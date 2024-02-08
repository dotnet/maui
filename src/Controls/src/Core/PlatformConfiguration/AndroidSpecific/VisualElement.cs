#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.VisualElement;

	/// <summary>
	/// Controls the legacy color mode and elevation for visual elements on the Android platform.
	/// </summary>
	public static class VisualElement
	{
		#region Elevation

		/// <summary>Bindable property for attached property <c>Elevation</c>.</summary>
		public static readonly BindableProperty ElevationProperty =
			BindableProperty.Create("Elevation", typeof(float?),
				typeof(FormsElement));

		/// <summary>
		/// Gets the elevation for the element.
		/// </summary>
		/// <param name="element">The visual element on the Android platform whose elevation to get.</param>
		/// <returns>The elevation for the element.</returns>
		public static float? GetElevation(FormsElement element)
		{
			return (float?)element.GetValue(ElevationProperty);
		}

		/// <summary>
		/// Sets the elevation for the visual element.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="value">The new elevation value.</param>
		public static void SetElevation(FormsElement element, float? value)
		{
			element.SetValue(ElevationProperty, value);
		}

		/// <summary>
		/// Gets the elevation for the element.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element on the Android platform whose elevation to get.</param>
		/// <returns>The elevation for the element.</returns>
		public static float? GetElevation(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetElevation(config.Element);
		}

		/// <summary>
		/// Sets the elevation for the visual element.
		/// </summary>
		/// <param name="config">The visual element on the Android platform whose elevation to set.</param>
		/// <param name="value">The new elevation value.</param>
		/// <returns></returns>
		public static IPlatformElementConfiguration<Android, FormsElement> SetElevation(this IPlatformElementConfiguration<Android, FormsElement> config, float? value)
		{
			SetElevation(config.Element, value);
			return config;
		}

		#endregion

		#region IsLegacyColorModeEnabled

		/// <summary>Bindable property for attached property <c>IsLegacyColorModeEnabled</c>.</summary>
		public static readonly BindableProperty IsLegacyColorModeEnabledProperty =
			BindableProperty.CreateAttached("IsLegacyColorModeEnabled", typeof(bool),
				typeof(FormsElement), true);

		/// <summary>
		/// Gets whether or not the legacy color mode for this element is enabled.
		/// </summary>
		/// <param name="element">The element on the Android platform whose legacy color mode status to get.</param>
		/// <returns><see langword="true"/> if legacy color mode is enabled for this element, otherwise <see langword="false"/>.</returns>
		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <summary>
		/// Sets whether or not the legacy color mode for this element is enabled.
		/// </summary>
		/// <param name="element">The platform configuration for the visual element on the Android platform whose legacy color mode status to set.</param>
		/// <param name="value"><see langword="true" /> to enable legacy color mode. Otherwise, <see langword="false" />.</param>
		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		/// <summary>
		/// Gets whether or not the legacy color mode for this element is enabled.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element on the Android platform whose legacy color mode status to get.</param>
		/// <returns><see langword="true"/> if legacy color mode is enabled for this element, otherwise <see langword="false"/>.</returns>
		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <summary>
		/// Sets whether or not the legacy color mode for this element is enabled.
		/// </summary>
		/// <param name="config">The platform configuration for the visual element on the Android platform whose legacy color mode status to set.</param>
		/// <param name="value"><see langword="true" /> to enable legacy color mode. Otherwise, <see langword="false" />.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion
	}
}
