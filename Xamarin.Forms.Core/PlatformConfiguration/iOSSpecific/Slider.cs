namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.Slider;

	public static class Slider
	{
		public static readonly BindableProperty UpdateOnTapProperty = BindableProperty.Create("UpdateOnTap", typeof(bool), typeof(Slider), false);

		public static bool GetUpdateOnTap(BindableObject element)
		{
			return (bool)element.GetValue(UpdateOnTapProperty);
		}

		public static void SetUpdateOnTap(BindableObject element, bool value)
		{
			element.SetValue(UpdateOnTapProperty, value);
		}

		public static bool GetUpdateOnTap(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUpdateOnTap(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetUpdateOnTap(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetUpdateOnTap(config.Element, value);
			return config;
		}
	}
}