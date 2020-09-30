namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.ScrollView;

	public static class ScrollView
	{
		public static readonly BindableProperty ShouldDelayContentTouchesProperty = BindableProperty.Create(nameof(ShouldDelayContentTouches), typeof(bool), typeof(ScrollView), true);

		public static bool GetShouldDelayContentTouches(BindableObject element)
		{
			return (bool)element.GetValue(ShouldDelayContentTouchesProperty);
		}

		public static void SetShouldDelayContentTouches(BindableObject element, bool value)
		{
			element.SetValue(ShouldDelayContentTouchesProperty, value);
		}

		public static bool ShouldDelayContentTouches(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShouldDelayContentTouches(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetShouldDelayContentTouches(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetShouldDelayContentTouches(config.Element, value);
			return config;
		}
	}
}