using System;
using System.Globalization;

namespace Xamarin.Forms
{
	public interface IExtendedTypeConverter
	{
		[Obsolete("IExtendedTypeConverter.ConvertFrom is obsolete as of version 2.2.0. Please use ConvertFromInvariantString (string, IServiceProvider) instead.")]
		object ConvertFrom(CultureInfo culture, object value, IServiceProvider serviceProvider);

		object ConvertFromInvariantString(string value, IServiceProvider serviceProvider);
	}
}