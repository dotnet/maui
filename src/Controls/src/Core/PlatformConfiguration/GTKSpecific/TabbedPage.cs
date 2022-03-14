namespace Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific.TabbedPage']/Docs" />
	public static class TabbedPage
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='TabPositionProperty']/Docs" />
		public static readonly BindableProperty TabPositionProperty =
			BindableProperty.Create("TabPosition", typeof(TabPosition),
				typeof(TabbedPage), TabPosition.Default);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='GetTabPosition'][1]/Docs" />
		public static TabPosition GetTabPosition(BindableObject element)
		{
			return (TabPosition)element.GetValue(TabPositionProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='SetTabPosition'][1]/Docs" />
		public static void SetTabPosition(BindableObject element, TabPosition tabPosition)
		{
			element.SetValue(TabPositionProperty, tabPosition);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='GetTabPosition'][2]/Docs" />
		public static TabPosition GetTabPosition(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetTabPosition(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='SetTabPosition'][2]/Docs" />
		public static IPlatformElementConfiguration<GTK, FormsElement> SetTabPosition(
			this IPlatformElementConfiguration<GTK, FormsElement> config, TabPosition value)
		{
			SetTabPosition(config.Element, value);

			return config;
		}
	}
}
