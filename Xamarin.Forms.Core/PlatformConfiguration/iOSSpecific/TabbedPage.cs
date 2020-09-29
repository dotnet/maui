namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.TabbedPage;

	public static class TabbedPage
	{
		public static readonly BindableProperty TranslucencyModeProperty =
			BindableProperty.Create("TranslucencyMode",
				typeof(TranslucencyMode), typeof(TabbedPage), TranslucencyMode.Default);

		public static TranslucencyMode GetTranslucencyMode(BindableObject element)
			=> (TranslucencyMode)element.GetValue(TranslucencyModeProperty);

		public static void SetTranslucencyMode(BindableObject element, TranslucencyMode value)
			=> element.SetValue(TranslucencyModeProperty, value);

		public static TranslucencyMode GetTranslucencyMode(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
			=> GetTranslucencyMode(config.Element);

		public static IPlatformElementConfiguration<iOS, FormsElement> SetTranslucencyMode(
			this IPlatformElementConfiguration<iOS, FormsElement> config, TranslucencyMode value)
		{
			SetTranslucencyMode(config.Element, value);
			return config;
		}
	}
}