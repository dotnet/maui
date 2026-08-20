using System;
using System.Globalization;
using Foundation;

namespace Microsoft.Maui.Platform
{
	public static class Culture
	{
		static CultureInfo? s_currentCulture;
		static string? s_localeIdentifier;

		public static CultureInfo CurrentCulture
		{
			get
			{
				var locale = NSLocale.CurrentLocale;
				var identifier = locale.LocaleIdentifier;

				if (s_currentCulture is null || s_localeIdentifier != identifier)
				{
					s_localeIdentifier = identifier;

					try
					{
						// On iOS 26, the locale identifier can contain a Regional Format override
						// (e.g. "en_US@rg=inzzzz"), which .NET cannot parse directly. Build the
						// culture name from LanguageCode/ScriptCode/RegionCode instead, since
						// RegionCode (unlike the identifier) resolves the "rg" override.
						if (OperatingSystem.IsIOSVersionAtLeast(26))
						{
							var languageCode = locale.LanguageCode;
							var scriptCode = locale.ScriptCode;
							var regionCode = locale.RegionCode;

							var cultureName = string.IsNullOrEmpty(scriptCode)
								? string.IsNullOrEmpty(regionCode) ? languageCode : $"{languageCode}-{regionCode}"
								: string.IsNullOrEmpty(regionCode) ? $"{languageCode}-{scriptCode}" : $"{languageCode}-{scriptCode}-{regionCode}";

							s_currentCulture = CultureInfo.GetCultureInfo(cultureName);
						}
						else
						{
							// iOS uses identifiers such as en_US, en_GB, ta_IN, etc.
							var cultureName = identifier.Replace('_', '-');
							s_currentCulture = CultureInfo.GetCultureInfo(cultureName);
						}
					}
					catch (CultureNotFoundException)
					{
						s_currentCulture = CultureInfo.InvariantCulture;
					}
				}

				return s_currentCulture;
			}
		}
	}
}
