namespace Xamarin.Forms.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Forms.TabbedPage;

	public static class TabbedPage
	{
		#region TabsStyle
		public static readonly BindableProperty TabsStyleProperty = BindableProperty.Create("TabsStyle", typeof(TabsStyle), typeof(TabbedPage), TabsStyle.Default);

		public static TabsStyle GetTabsStyle(BindableObject element)
		{
			return (TabsStyle)element.GetValue(TabsStyleProperty);
		}

		public static void SetTabsStyle(BindableObject element, TabsStyle value)
		{
			element.SetValue(TabsStyleProperty, value);
		}

		public static TabsStyle GetTabsStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetTabsStyle(config.Element);
		}

		public static IPlatformElementConfiguration<macOS, FormsElement> SetShowTabs(this IPlatformElementConfiguration<macOS, FormsElement> config, TabsStyle value)
		{
			SetTabsStyle(config.Element, value);
			return config;
		}

		public static IPlatformElementConfiguration<macOS, FormsElement> ShowTabsOnNavigation(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.OnNavigation);
			return config;
		}

		public static IPlatformElementConfiguration<macOS, FormsElement> ShowTabs(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.Default);
			return config;
		}

		public static IPlatformElementConfiguration<macOS, FormsElement> HideTabs(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.Hidden);
			return config;
		}
		#endregion
	}
}