#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TabbedPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.TabbedPage']/Docs/*" />
	public static class TabbedPage
	{
		/// <summary>Bindable property for <see cref="TranslucencyMode"/>.</summary>
		public static readonly BindableProperty TranslucencyModeProperty =
			BindableProperty.Create("TranslucencyMode",
				typeof(TranslucencyMode), typeof(TabbedPage), TranslucencyMode.Default);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TabbedPage.xml" path="//Member[@MemberName='GetTranslucencyMode'][1]/Docs/*" />
		public static TranslucencyMode GetTranslucencyMode(BindableObject element)
			=> (TranslucencyMode)element.GetValue(TranslucencyModeProperty);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TabbedPage.xml" path="//Member[@MemberName='SetTranslucencyMode'][1]/Docs/*" />
		public static void SetTranslucencyMode(BindableObject element, TranslucencyMode value)
			=> element.SetValue(TranslucencyModeProperty, value);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TabbedPage.xml" path="//Member[@MemberName='GetTranslucencyMode'][2]/Docs/*" />
		public static TranslucencyMode GetTranslucencyMode(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
			=> GetTranslucencyMode(config.Element);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TabbedPage.xml" path="//Member[@MemberName='SetTranslucencyMode'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetTranslucencyMode(
			this IPlatformElementConfiguration<iOS, FormsElement> config, TranslucencyMode value)
		{
			SetTranslucencyMode(config.Element, value);
			return config;
		}

//todo add these methods to public API 
#pragma warning disable RS0016
		public static readonly BindableProperty AutoResizeIconsProperty =
			BindableProperty.Create("AutoResizeIcons",
				typeof(bool), typeof(TabbedPage), false);

		public static bool GetAutoResizeIcons(BindableObject element)
			=> (bool)element.GetValue(AutoResizeIconsProperty);

		public static void SetAutoResizeIcons(BindableObject element, bool value)
			=> element.SetValue(AutoResizeIconsProperty, value);

		public static bool GetAutoResizeIcons(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
			=> GetAutoResizeIcons(config.Element);

		public static IPlatformElementConfiguration<iOS, FormsElement> SetAutoResizeIcons(
			this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetAutoResizeIcons(config.Element, value);
			return config;
		}
	}
}
