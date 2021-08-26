using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	[Flags]
	public enum FontAttributes
	{
		None = 0,
		Bold = 1 << 0,
		Italic = 1 << 1
	}

	public sealed class FontAttributesConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (string.IsNullOrEmpty(strValue))
				return FontAttributes.None;

			FontAttributes attributes = FontAttributes.None;
			strValue = strValue.Trim();
			if (strValue.Contains(","))
			{ //Xaml
				foreach (var part in strValue.Split(','))
					attributes |= ParseSingleAttribute(part, strValue);

			}
			else
			{ //CSS or single value
				foreach (var part in strValue.Split(' '))
					attributes |= ParseSingleAttribute(part, strValue);
			}
			return attributes;
		}

		static FontAttributes ParseSingleAttribute(string part, string originalvalue)
		{
			part = part.Trim();
			if (Enum.TryParse(part, true, out FontAttributes attr))
				return attr;
			if (part.Equals("normal", StringComparison.OrdinalIgnoreCase))
				return FontAttributes.None;
			if (part.Equals("oblique", StringComparison.OrdinalIgnoreCase))
				return FontAttributes.Italic;

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", originalvalue, typeof(FontAttributes)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not FontAttributes attr)
				throw new NotSupportedException();
			if (attr == FontAttributes.None)
				return "";
			var parts = new List<string>();
			if ((attr & FontAttributes.Bold) == FontAttributes.Bold)
				parts.Add(nameof(FontAttributes.Bold));
			if ((attr & FontAttributes.Italic) == FontAttributes.Italic)
				parts.Add(nameof(FontAttributes.Italic));
			return string.Join("' ", parts);
		}
	}
}