namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific.TabbedPage']/Docs" />
	public static class TabbedPage
	{
		#region TabsStyle
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="//Member[@MemberName='TabsStyleProperty']/Docs" />
		public static readonly BindableProperty TabsStyleProperty = BindableProperty.Create("TabsStyle", typeof(TabsStyle), typeof(TabbedPage), TabsStyle.Default);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="//Member[@MemberName='GetTabsStyle'][1]/Docs" />
		public static TabsStyle GetTabsStyle(BindableObject element)
		{
			return (TabsStyle)element.GetValue(TabsStyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="//Member[@MemberName='SetTabsStyle']/Docs" />
		public static void SetTabsStyle(BindableObject element, TabsStyle value)
		{
			element.SetValue(TabsStyleProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="//Member[@MemberName='GetTabsStyle'][2]/Docs" />
		public static TabsStyle GetTabsStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetTabsStyle(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="//Member[@MemberName='SetShowTabs']/Docs" />
		public static IPlatformElementConfiguration<macOS, FormsElement> SetShowTabs(this IPlatformElementConfiguration<macOS, FormsElement> config, TabsStyle value)
		{
			SetTabsStyle(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="//Member[@MemberName='ShowTabsOnNavigation']/Docs" />
		public static IPlatformElementConfiguration<macOS, FormsElement> ShowTabsOnNavigation(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.OnNavigation);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="//Member[@MemberName='ShowTabs']/Docs" />
		public static IPlatformElementConfiguration<macOS, FormsElement> ShowTabs(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.Default);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/TabbedPage.xml" path="//Member[@MemberName='HideTabs']/Docs" />
		public static IPlatformElementConfiguration<macOS, FormsElement> HideTabs(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.Hidden);
			return config;
		}
		#endregion
	}
}
