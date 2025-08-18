#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.SearchBar;

	/// <summary>Provides control over the spellchecker on search bars.</summary>
	public static class SearchBar
	{
		/// <summary>Bindable property for <see cref="IsSpellCheckEnabled"/>.</summary>
		public static readonly BindableProperty IsSpellCheckEnabledProperty =
			BindableProperty.Create("IsSpellCheckEnabled ", typeof(bool), typeof(SearchBar), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/SearchBar.xml" path="//Member[@MemberName='SetIsSpellCheckEnabled'][1]/Docs/*" />
		public static void SetIsSpellCheckEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSpellCheckEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/SearchBar.xml" path="//Member[@MemberName='GetIsSpellCheckEnabled'][1]/Docs/*" />
		public static bool GetIsSpellCheckEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSpellCheckEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/SearchBar.xml" path="//Member[@MemberName='GetIsSpellCheckEnabled'][2]/Docs/*" />
		public static bool GetIsSpellCheckEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetIsSpellCheckEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/SearchBar.xml" path="//Member[@MemberName='SetIsSpellCheckEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetIsSpellCheckEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			SetIsSpellCheckEnabled(config.Element, value);
			return config;
		}

		/// <summary>Returns a Boolean value that tells whether the spellchecker is enabled.</summary>
		/// <param name="config">The platform configuration for the search bar element.</param>
		/// <returns><see langword="true"/> if the spellchecker is enabled. Otherwise, <see langword="false"/>.</returns>
		public static bool IsSpellCheckEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetIsSpellCheckEnabled(config.Element);
		}

		/// <summary>Enables the spellchecker.</summary>
		/// <param name="config">The platform configuration for the search bar element.</param>
		public static void EnableSpellCheck(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetIsSpellCheckEnabled(config.Element, true);
		}

		/// <summary>Disables the spellchecker.</summary>
		/// <param name="config">The platform configuration for the search bar element.</param>
		public static void DisableSpellCheck(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetIsSpellCheckEnabled(config.Element, false);
		}
	}
}
