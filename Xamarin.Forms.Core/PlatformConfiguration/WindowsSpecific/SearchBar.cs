using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Forms.SearchBar;

	public static class SearchBar
	{
		public static readonly BindableProperty IsSpellCheckEnabledProperty =
			BindableProperty.Create("IsSpellCheckEnabled ", typeof(bool), typeof(SearchBar), false);

		public static void SetIsSpellCheckEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsSpellCheckEnabledProperty, value);
		}

		public static bool GetIsSpellCheckEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsSpellCheckEnabledProperty);
		}

		public static bool GetIsSpellCheckEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetIsSpellCheckEnabled(config.Element);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetIsSpellCheckEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			SetIsSpellCheckEnabled(config.Element, value);
			return config;
		}

		public static bool IsSpellCheckEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetIsSpellCheckEnabled(config.Element);
		}

		public static void EnableSpellCheck(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetIsSpellCheckEnabled(config.Element, true);
		}

		public static void DisableSpellCheck(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetIsSpellCheckEnabled(config.Element, false);
		}
	}
}