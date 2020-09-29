namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsImageButton = Forms.ImageButton;

	public static class ImageButton
	{
		#region Shadow
		public static readonly BindableProperty IsShadowEnabledProperty = BindableProperty.Create("IsShadowEnabled", typeof(bool), typeof(Forms.ImageButton), false);

		public static bool GetIsShadowEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsShadowEnabledProperty);
		}

		public static void SetIsShadowEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsShadowEnabledProperty, value);
		}

		public static bool GetIsShadowEnabled(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetIsShadowEnabled(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsImageButton> SetIsShadowEnabled(this IPlatformElementConfiguration<Android, FormsImageButton> config, bool value)
		{
			SetIsShadowEnabled(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create("ShadowColor", typeof(Color), typeof(ImageButton), Color.Default);

		public static Color GetShadowColor(BindableObject element)
		{
			return (Color)element.GetValue(ShadowColorProperty);
		}

		public static void SetShadowColor(BindableObject element, Color value)
		{
			element.SetValue(ShadowColorProperty, value);
		}

		public static Color GetShadowColor(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetShadowColor(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsImageButton> SetShadowColor(this IPlatformElementConfiguration<Android, FormsImageButton> config, Color value)
		{
			SetShadowColor(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ShadowRadiusProperty = BindableProperty.Create("ShadowRadius", typeof(double), typeof(ImageButton), 10.0);

		public static double GetShadowRadius(BindableObject element)
		{
			return (double)element.GetValue(ShadowRadiusProperty);
		}

		public static void SetShadowRadius(BindableObject element, double value)
		{
			element.SetValue(ShadowRadiusProperty, value);
		}

		public static double GetShadowRadius(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetShadowRadius(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsImageButton> SetShadowRadius(this IPlatformElementConfiguration<Android, FormsImageButton> config, double value)
		{
			SetShadowRadius(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ShadowOffsetProperty = BindableProperty.Create("ShadowOffset", typeof(Size), typeof(VisualElement), Size.Zero);

		public static Size GetShadowOffset(BindableObject element)
		{
			return (Size)element.GetValue(ShadowOffsetProperty);
		}

		public static void SetShadowOffset(BindableObject element, Size value)
		{
			element.SetValue(ShadowOffsetProperty, value);
		}

		public static Size GetShadowOffset(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetShadowOffset(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsImageButton> SetShadowOffset(this IPlatformElementConfiguration<Android, FormsImageButton> config, Size value)
		{
			SetShadowOffset(config.Element, value);
			return config;
		}
		#endregion
	}
}