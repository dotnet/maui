using System.Globalization;
using System.Linq;
using Foundation;

namespace Microsoft.Maui.Platform
{
	public static class Culture
	{
		static NSLocale? s_locale;
		static CultureInfo? s_currentCulture;

		public static CultureInfo CurrentCulture
		{
			get
			{
				if (s_locale == null || s_currentCulture == null || s_locale != NSLocale.CurrentLocale)
				{
					s_locale = NSLocale.CurrentLocale;
					string? countryCode = s_locale.CountryCode;
					var cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures)
						.Where(c => c.Name.EndsWith("-" + countryCode)).FirstOrDefault();

					if (cultureInfo == null)
						cultureInfo = CultureInfo.InvariantCulture;

					s_currentCulture = cultureInfo;
				}

				return s_currentCulture;
			}
		}
	}
}