using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Converters
{
	/// <summary>
	/// Type converter for SafeAreaEdges struct, supporting XAML parsing of 
	/// SafeArea.Ignore attached property values.
	/// </summary>
	public class SafeAreaEdgesTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
		{
			// IMPORTANT! Update SafeAreaEdgesTypeDesignConverter.IsValid if making changes here
			var strValue = value?.ToString();

			if (strValue != null)
			{
				strValue = strValue.Trim();

				// Split by comma - if no comma, we get array with single element
				var parts = strValue.Split(',');
				var regions = new SafeAreaRegions[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();

					// Performance optimization: use string comparison instead of Enum.TryParse
					// since SafeAreaRegions has specific values
					if (string.Equals(part, "All", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = SafeAreaRegions.All;
					}
					else if (string.Equals(part, "None", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = SafeAreaRegions.None;
					}
					else if (string.Equals(part, "Container", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = SafeAreaRegions.Container;
					}
					else if (string.Equals(part, "SoftInput", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = SafeAreaRegions.SoftInput;
					}
					else if (string.Equals(part, "Default", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = SafeAreaRegions.Default;
					}
					else
					{
						throw new FormatException($"Cannot convert \"{part}\" into {typeof(SafeAreaRegions)}");
					}
				}

				// Convert array to SafeAreaEdges using the same logic as before
				return regions.Length switch
				{
					1 => new SafeAreaEdges(regions[0]),
					2 => new SafeAreaEdges(regions[0], regions[1]), // horizontal, vertical
					4 => new SafeAreaEdges(regions[0], regions[1], regions[2], regions[3]), // left, top, right, bottom
					_ => throw new FormatException($"SafeAreaEdges must have 1, 2, or 4 values, but got {regions.Length}")
				};
			}

			throw new FormatException($"Cannot convert \"{strValue}\" into {typeof(SafeAreaEdges)}");
		}

		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not SafeAreaEdges edges)
				throw new NotSupportedException();

			return edges.ToString();
		}
	}
}