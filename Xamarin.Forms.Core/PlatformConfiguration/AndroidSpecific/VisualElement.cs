namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.VisualElement;

	public static class VisualElement
	{
		#region Elevation

		public static readonly BindableProperty ElevationProperty =
			BindableProperty.Create("Elevation", typeof(float?),
				typeof(FormsElement));

		public static float? GetElevation(FormsElement element)
		{
			return (float?)element.GetValue(ElevationProperty);
		}

		public static void SetElevation(FormsElement element, float? value)
		{
			element.SetValue(ElevationProperty, value);
		}

		public static float? GetElevation(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetElevation(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetElevation(this IPlatformElementConfiguration<Android, FormsElement> config, float? value)
		{
			SetElevation(config.Element, value);
			return config;
		}

		#endregion

		#region IsLegacyColorModeEnabled

		public static readonly BindableProperty IsLegacyColorModeEnabledProperty =
			BindableProperty.CreateAttached("IsLegacyColorModeEnabled", typeof(bool),
				typeof(FormsElement), true);

		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion
	}
}