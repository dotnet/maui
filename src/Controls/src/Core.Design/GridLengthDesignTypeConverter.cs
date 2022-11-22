using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Design
{
	public class GridLengthDesignTypeConverter : TypeConverter
	{
		// This tells XAML this converter can be used to process strings
		// Without this the values won't show up as hints
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			return IsValid(value?.ToString());
		}

		internal static bool IsValid(string value)
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value))
				return false;

			if (string.Compare(value, "auto", StringComparison.OrdinalIgnoreCase) == 0)
				return true;
			if (string.Compare(value, "*", StringComparison.OrdinalIgnoreCase) == 0)
				return true;
			if (value.EndsWith("*", StringComparison.Ordinal) && double.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out var len))
				return !(len < 0 || double.IsNaN(len));
			if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out len))
				return !(len < 0 || double.IsNaN(len));

			return false;
		}
	}
}
