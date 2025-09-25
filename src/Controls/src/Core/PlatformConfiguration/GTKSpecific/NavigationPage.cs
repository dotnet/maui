#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <summary>Provides access to the back button icon on navigation pages on the GTK platform.</summary>
	public static class NavigationPage
	{
		/// <summary>Bindable property for attached property <c>BackButtonIcon</c>.</summary>
		public static readonly BindableProperty BackButtonIconProperty =
			BindableProperty.Create("BackButtonIcon", typeof(string),
				typeof(NavigationPage), default(string));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='GetBackButtonIcon'][1]/Docs/*" />
		public static string GetBackButtonIcon(BindableObject element)
		{
			return (string)element.GetValue(BackButtonIconProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='SetBackButtonIcon'][1]/Docs/*" />
		public static void SetBackButtonIcon(BindableObject element, string backButtonIcon)
		{
			element.SetValue(BackButtonIconProperty, backButtonIcon);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='GetBackButtonIcon'][2]/Docs/*" />
		public static string GetBackButtonIcon(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetBackButtonIcon(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/NavigationPage.xml" path="//Member[@MemberName='SetBackButtonIcon'][2]/Docs/*" />
		public static IPlatformElementConfiguration<GTK, FormsElement> SetBackButtonIcon(
			this IPlatformElementConfiguration<GTK, FormsElement> config, string value)
		{
			SetBackButtonIcon(config.Element, value);

			return config;
		}
	}
}
