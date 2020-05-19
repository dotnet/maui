using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Maui
{
	public interface IExtendedTypeConverter
	{
		[Obsolete("IExtendedTypeConverter.ConvertFrom is obsolete as of version 2.2.0. Please use ConvertFromInvariantString (string, IServiceProvider) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		object ConvertFrom(CultureInfo culture, object value, IServiceProvider serviceProvider);

		object ConvertFromInvariantString(string value, IServiceProvider serviceProvider);
	}
}