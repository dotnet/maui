namespace Xamarin.Forms.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Forms.TabbedPage;

	public static class TabbedPage
	{
		public static readonly BindableProperty TabPositionProperty =
			BindableProperty.Create("TabPosition", typeof(TabPosition),
				typeof(TabbedPage), TabPosition.Default);

		public static TabPosition GetTabPosition(BindableObject element)
		{
			return (TabPosition)element.GetValue(TabPositionProperty);
		}

		public static void SetTabPosition(BindableObject element, TabPosition tabPosition)
		{
			element.SetValue(TabPositionProperty, tabPosition);
		}

		public static TabPosition GetTabPosition(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetTabPosition(config.Element);
		}

		public static IPlatformElementConfiguration<GTK, FormsElement> SetTabPosition(
			this IPlatformElementConfiguration<GTK, FormsElement> config, TabPosition value)
		{
			SetTabPosition(config.Element, value);

			return config;
		}
	}
}