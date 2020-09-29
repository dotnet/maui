namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat
{
	using FormsElement = Forms.NavigationPage;

	public static class NavigationPage
	{
		public static readonly BindableProperty BarHeightProperty = BindableProperty.Create("BarHeight", typeof(int), typeof(NavigationPage), default(int));

		public static int GetBarHeight(BindableObject element)
		{
			return (int)element.GetValue(BarHeightProperty);
		}

		public static void SetBarHeight(BindableObject element, int value)
		{
			element.SetValue(BarHeightProperty, value);
		}

		public static int GetBarHeight(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetBarHeight(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetBarHeight(this IPlatformElementConfiguration<Android, FormsElement> config, int value)
		{
			SetBarHeight(config.Element, value);
			return config;
		}
	}
}