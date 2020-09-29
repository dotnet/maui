namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Xamarin.Forms.DatePicker;

	public static class DatePicker
	{
		public static readonly BindableProperty UpdateModeProperty = BindableProperty.Create(
			nameof(UpdateMode),
			typeof(UpdateMode),
			typeof(DatePicker),
			default(UpdateMode));

		public static UpdateMode GetUpdateMode(BindableObject element)
		{
			return (UpdateMode)element.GetValue(UpdateModeProperty);
		}

		public static void SetUpdateMode(BindableObject element, UpdateMode value)
		{
			element.SetValue(UpdateModeProperty, value);
		}

		public static UpdateMode UpdateMode(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUpdateMode(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetUpdateMode(this IPlatformElementConfiguration<iOS, FormsElement> config, UpdateMode value)
		{
			SetUpdateMode(config.Element, value);
			return config;
		}
	}
}