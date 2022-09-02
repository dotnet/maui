using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Microsoft.Maui.Controls.SearchBar;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/SearchBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.SearchBar']/Docs" />
	public static class SearchBar
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/SearchBar.xml" path="//Member[@MemberName='SearchBarStyleProperty']/Docs" />
		public static readonly BindableProperty SearchBarStyleProperty = BindableProperty.Create("SearchBarStyle", typeof(UISearchBarStyle), typeof(SearchBar), UISearchBarStyle.Default);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/SearchBar.xml" path="//Member[@MemberName='GetSearchBarStyle'][1]/Docs" />
		public static UISearchBarStyle GetSearchBarStyle(BindableObject element)
		{
			return (UISearchBarStyle)element.GetValue(SearchBarStyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/SearchBar.xml" path="//Member[@MemberName='SetSearchBarStyle'][1]/Docs" />
		public static void SetSearchBarStyle(BindableObject element, UISearchBarStyle style)
		{
			element.SetValue(SearchBarStyleProperty, style);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/SearchBar.xml" path="//Member[@MemberName='GetSearchBarStyle'][2]/Docs" />
		public static UISearchBarStyle GetSearchBarStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetSearchBarStyle(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/SearchBar.xml" path="//Member[@MemberName='SetSearchBarStyle'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetSearchBarStyle(
			this IPlatformElementConfiguration<iOS, FormsElement> config, UISearchBarStyle style)
		{
			SetSearchBarStyle(config.Element, style);
			return config;
		}
	}
}
