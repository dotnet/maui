#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Microsoft.Maui.Controls.SearchBar;

	/// <summary>Provides iOS-specific configuration for SearchBar visual style.</summary>
	public static class SearchBar
	{
		/// <summary>Bindable property for attached property <c>SearchBarStyle</c>.</summary>
		public static readonly BindableProperty SearchBarStyleProperty = BindableProperty.Create("SearchBarStyle", typeof(UISearchBarStyle), typeof(SearchBar), UISearchBarStyle.Default);

		/// <summary>Gets the iOS UISearchBarStyle for the SearchBar.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns>The search bar style.</returns>
		public static UISearchBarStyle GetSearchBarStyle(BindableObject element)
		{
			return (UISearchBarStyle)element.GetValue(SearchBarStyleProperty);
		}

		/// <summary>Sets the iOS UISearchBarStyle for the SearchBar.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="style">The search bar style to apply.</param>
		public static void SetSearchBarStyle(BindableObject element, UISearchBarStyle style)
		{
			element.SetValue(SearchBarStyleProperty, style);
		}

		/// <summary>Gets the iOS UISearchBarStyle for the SearchBar.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The search bar style.</returns>
		public static UISearchBarStyle GetSearchBarStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetSearchBarStyle(config.Element);
		}

		/// <summary>Sets the iOS UISearchBarStyle for the SearchBar.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="style">The search bar style to apply.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetSearchBarStyle(
			this IPlatformElementConfiguration<iOS, FormsElement> config, UISearchBarStyle style)
		{
			SetSearchBarStyle(config.Element, style);
			return config;
		}
	}
}
