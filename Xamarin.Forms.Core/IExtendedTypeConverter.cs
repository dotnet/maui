using System;
using System.Globalization;

namespace Xamarin.Forms
{
	public interface IExtendedTypeConverter
	{
		[Obsolete("use ConvertFromInvariantString (string, IServiceProvider)")]
		object ConvertFrom(CultureInfo culture, object value, IServiceProvider serviceProvider);

		object ConvertFromInvariantString(string value, IServiceProvider serviceProvider);
	}
}