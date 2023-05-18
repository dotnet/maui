#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/NavigationPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.NavigationPage']/Docs/*" />
	public static class NavigationPage
	{
		#region HasBreadCrumbsBar
		/// <summary>Bindable property for attached property <c>HasBreadCrumbsBar</c>.</summary>
		public static readonly BindableProperty HasBreadCrumbsBarProperty
			= BindableProperty.CreateAttached("HasBreadCrumbsBar", typeof(bool), typeof(FormsElement), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/NavigationPage.xml" path="//Member[@MemberName='GetHasBreadCrumbsBar']/Docs/*" />
		public static bool GetHasBreadCrumbsBar(BindableObject element)
		{
			return (bool)element.GetValue(HasBreadCrumbsBarProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/NavigationPage.xml" path="//Member[@MemberName='SetHasBreadCrumbsBar'][1]/Docs/*" />
		public static void SetHasBreadCrumbsBar(BindableObject element, bool value)
		{
			element.SetValue(HasBreadCrumbsBarProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/NavigationPage.xml" path="//Member[@MemberName='HasBreadCrumbsBar']/Docs/*" />
		public static bool HasBreadCrumbsBar(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetHasBreadCrumbsBar(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/NavigationPage.xml" path="//Member[@MemberName='SetHasBreadCrumbsBar'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetHasBreadCrumbsBar(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetHasBreadCrumbsBar(config.Element, value);
			return config;
		}
		#endregion
	}
}
