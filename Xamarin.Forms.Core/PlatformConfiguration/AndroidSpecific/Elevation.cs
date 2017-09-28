namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	public static class Elevation
	{
		public static readonly BindableProperty ElevationProperty =
			BindableProperty.Create("Elevation", typeof(float?),
				typeof(Elevation));

		public static float? GetElevation(VisualElement element) 
		{
			return (float?)element.GetValue(ElevationProperty);
		}

		public static void SetElevation(VisualElement element, float? value)
		{
			element.SetValue(ElevationProperty, value);
		}

		public static float? GetElevation(this IPlatformElementConfiguration<Android, VisualElement> config)
		{
			return GetElevation(config.Element);
		}

		public static IPlatformElementConfiguration<Android, VisualElement> SetElevation(this IPlatformElementConfiguration<Android, VisualElement> config, float? value) 
		{
			SetElevation(config.Element, value);
			return config;
		}
	}
}