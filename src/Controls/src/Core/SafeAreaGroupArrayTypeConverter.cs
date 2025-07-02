using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Type converter for SafeAreaGroup[] arrays, supporting XAML parsing of 
	/// SafeAreaGuides.IgnoreSafeArea attached property values.
	/// </summary>
	public class SafeAreaGroupArrayTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				strValue = strValue.Trim();

				// Split by comma - if no comma, we get array with single element
				var parts = strValue.Split(',');
				var result = new SafeAreaGroup[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					
					// Performance optimization: use string comparison instead of Enum.TryParse
					// since SafeAreaGroup only has two values currently
					if (string.Equals(part, "All", StringComparison.OrdinalIgnoreCase))
					{
						result[i] = SafeAreaGroup.All;
					}
					else if (string.Equals(part, "None", StringComparison.OrdinalIgnoreCase))
					{
						result[i] = SafeAreaGroup.None;
					}
					else
					{
						throw new FormatException($"Cannot convert \"{part}\" to SafeAreaGroup");
					}
				}

				return result;
			}

			throw new FormatException($"Cannot convert \"{strValue}\" into {typeof(SafeAreaGroup[])}");
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not SafeAreaGroup[] array)
				throw new NotSupportedException();

			if (array.Length == 0)
				return string.Empty;

			if (array.Length == 1)
				return array[0].ToString();

			return string.Join(", ", Array.ConvertAll(array, item => item.ToString()));
		}
	}
}