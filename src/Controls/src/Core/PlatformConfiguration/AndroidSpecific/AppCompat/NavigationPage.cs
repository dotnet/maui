#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <summary>Appcompat platform specific navigation page.</summary>
	public static class NavigationPage
	{
		/// <summary>Bindable property for attached property <c>BarHeight</c>.</summary>
		public static readonly BindableProperty BarHeightProperty = BindableProperty.Create("BarHeight", typeof(int), typeof(NavigationPage), default(int));

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/NavigationPage.xml" path="//Member[@MemberName='GetBarHeight'][1]/Docs/*" />
		public static int GetBarHeight(BindableObject element)
		{
			return (int)element.GetValue(BarHeightProperty);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/NavigationPage.xml" path="//Member[@MemberName='SetBarHeight'][1]/Docs/*" />
		public static void SetBarHeight(BindableObject element, int value)
		{
			element.SetValue(BarHeightProperty, value);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/NavigationPage.xml" path="//Member[@MemberName='GetBarHeight'][2]/Docs/*" />
		public static int GetBarHeight(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetBarHeight(config.Element);
		}

		/// <include file="../../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat/NavigationPage.xml" path="//Member[@MemberName='SetBarHeight'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetBarHeight(this IPlatformElementConfiguration<Android, FormsElement> config, int value)
		{
			SetBarHeight(config.Element, value);
			return config;
		}
	}
}
