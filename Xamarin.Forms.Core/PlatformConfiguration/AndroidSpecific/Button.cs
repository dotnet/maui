namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.Button;

	public static class Button
	{
		#region UseDefaultPadding
		public static readonly BindableProperty UseDefaultPaddingProperty = BindableProperty.Create("UseDefaultPadding", typeof(bool), typeof(Button), false);

		public static bool GetUseDefaultPadding(BindableObject element)
		{
			return (bool)element.GetValue(UseDefaultPaddingProperty);
		}

		public static void SetUseDefaultPadding(BindableObject element, bool value)
		{
			element.SetValue(UseDefaultPaddingProperty, value);
		}

		public static bool UseDefaultPadding(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetUseDefaultPadding(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetUseDefaultPadding(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetUseDefaultPadding(config.Element, value);
			return config;
		}
		#endregion

		#region UseDefaultShadow
		public static readonly BindableProperty UseDefaultShadowProperty = BindableProperty.Create("UseDefaultShadow", typeof(bool), typeof(Button), false);

		public static bool GetUseDefaultShadow(BindableObject element)
		{
			return (bool)element.GetValue(UseDefaultShadowProperty);
		}

		public static void SetUseDefaultShadow(BindableObject element, bool value)
		{
			element.SetValue(UseDefaultShadowProperty, value);
		}

		public static bool UseDefaultShadow(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetUseDefaultShadow(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetUseDefaultShadow(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetUseDefaultShadow(config.Element, value);
			return config;
		}
		#endregion
	}
}