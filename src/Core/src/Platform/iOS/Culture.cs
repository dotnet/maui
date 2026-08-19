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
						// iOS uses identifiers such as en_US, en_GB, ta_IN, etc.
						var cultureName = identifier.Replace('_', '-');
						s_currentCulture = CultureInfo.GetCultureInfo(cultureName);
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
