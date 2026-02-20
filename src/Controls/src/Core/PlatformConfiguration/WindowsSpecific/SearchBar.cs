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

		/// <summary>Sets whether spell checking is enabled for the search bar on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value"><see langword="true"/> to enable spell checking.</param>
		public static void SetIsSpellCheckEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSpellCheckEnabledProperty, value);
		}

		/// <summary>Gets whether spell checking is enabled for the search bar on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns><see langword="true"/> if spell checking is enabled.</returns>
		public static bool GetIsSpellCheckEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSpellCheckEnabledProperty);
		}

		/// <summary>Gets whether spell checking is enabled for the search bar on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if spell checking is enabled.</returns>
		public static bool GetIsSpellCheckEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetIsSpellCheckEnabled(config.Element);
		}

		/// <summary>Sets whether spell checking is enabled for the search bar on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable spell checking.</param>
		/// <returns>The updated platform configuration.</returns>
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
