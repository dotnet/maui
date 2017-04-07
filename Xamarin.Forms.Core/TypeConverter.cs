using System;
using System.Globalization;

namespace Xamarin.Forms
{
	public abstract class TypeConverter
	{
		public virtual bool CanConvertFrom(Type sourceType)
		{
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));

			return sourceType == typeof(string);
		}

		[Obsolete("ConvertFrom is obsolete as of version 2.2.0. Please use ConvertFromInvariantString (string) instead.")]
		public virtual object ConvertFrom(object o)
		{
			return null;
		}

		[Obsolete("ConvertFrom is obsolete as of version 2.2.0. Please use ConvertFromInvariantString (string) instead.")]
		public virtual object ConvertFrom(CultureInfo culture, object o)
		{
			return null;
		}

		public virtual object ConvertFromInvariantString(string value)
		{
#pragma warning disable 0618 // retain until ConvertFrom removed
			return ConvertFrom(CultureInfo.InvariantCulture, value);
#pragma warning restore
		}
	}
}