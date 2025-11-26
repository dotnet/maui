#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <summary>Controls the tab position on tabbed pages on the GTK platform.</summary>
	public static class TabbedPage
	{
		/// <summary>Bindable property for <see cref="TabPosition"/>.</summary>
		public static readonly BindableProperty TabPositionProperty =
			BindableProperty.Create("TabPosition", typeof(TabPosition),
				typeof(TabbedPage), TabPosition.Default);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='GetTabPosition'][1]/Docs/*" />
		public static TabPosition GetTabPosition(BindableObject element)
		{
			return (TabPosition)element.GetValue(TabPositionProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='SetTabPosition'][1]/Docs/*" />
		public static void SetTabPosition(BindableObject element, TabPosition tabPosition)
		{
			element.SetValue(TabPositionProperty, tabPosition);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='GetTabPosition'][2]/Docs/*" />
		public static TabPosition GetTabPosition(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetTabPosition(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/TabbedPage.xml" path="//Member[@MemberName='SetTabPosition'][2]/Docs/*" />
		public static IPlatformElementConfiguration<GTK, FormsElement> SetTabPosition(
			this IPlatformElementConfiguration<GTK, FormsElement> config, TabPosition value)
		{
			SetTabPosition(config.Element, value);

			return config;
		}
	}
}
