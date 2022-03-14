namespace Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific.NavigationPage']/Docs" />
	public static class NavigationPage
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='BackButtonIconProperty']/Docs" />
		public static readonly BindableProperty BackButtonIconProperty =
			BindableProperty.Create("BackButtonIcon", typeof(string),
				typeof(NavigationPage), default(string));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='GetBackButtonIcon'][1]/Docs" />
		public static string GetBackButtonIcon(BindableObject element)
		{
			return (string)element.GetValue(BackButtonIconProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='SetBackButtonIcon'][1]/Docs" />
		public static void SetBackButtonIcon(BindableObject element, string backButtonIcon)
		{
			element.SetValue(BackButtonIconProperty, backButtonIcon);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='GetBackButtonIcon'][2]/Docs" />
		public static string GetBackButtonIcon(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetBackButtonIcon(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='SetBackButtonIcon'][2]/Docs" />
		public static IPlatformElementConfiguration<GTK, FormsElement> SetBackButtonIcon(
			this IPlatformElementConfiguration<GTK, FormsElement> config, string value)
		{
			SetBackButtonIcon(config.Element, value);

			return config;
		}
	}
}
