namespace Xamarin.Forms.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Forms.NavigationPage;

	public static class NavigationPage
	{
		public static readonly BindableProperty BackButtonIconProperty =
			BindableProperty.Create("BackButtonIcon", typeof(string),
				typeof(NavigationPage), default(string));

		public static string GetBackButtonIcon(BindableObject element)
		{
			return (string)element.GetValue(BackButtonIconProperty);
		}

		public static void SetBackButtonIcon(BindableObject element, string backButtonIcon)
		{
			element.SetValue(BackButtonIconProperty, backButtonIcon);
		}

		public static string GetBackButtonIcon(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetBackButtonIcon(config.Element);
		}

		public static IPlatformElementConfiguration<GTK, FormsElement> SetBackButtonIcon(
			this IPlatformElementConfiguration<GTK, FormsElement> config, string value)
		{
			SetBackButtonIcon(config.Element, value);

			return config;
		}
	}
}