using System;
using System.Globalization;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(GridLength))]
	public class GridLengthTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == null)
				return null;

			value = value.Trim();
			if (string.Compare(value, "auto", StringComparison.OrdinalIgnoreCase) == 0)
				return GridLength.Auto;
			if (string.Compare(value, "*", StringComparison.OrdinalIgnoreCase) == 0)
				return new GridLength(1, GridUnitType.Star);
			if (value.EndsWith("*", StringComparison.Ordinal) && double.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out var length))
				return new GridLength(length, GridUnitType.Star);
			if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out length))
				return new GridLength(length);

			throw new FormatException();
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is GridLength length))
				throw new NotSupportedException();
			if (length.IsAuto)
				return "auto";
			if (length.IsStar)
				return $"{length.Value.ToString(CultureInfo.InvariantCulture)}*";
			return $"{length.Value.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}