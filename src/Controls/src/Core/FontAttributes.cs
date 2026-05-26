#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>Enumerates values that describe font styles.</summary>
	[Flags]
	public enum FontAttributes
	{
		/// <summary>The font is unmodified.</summary>
		None = 0,
		/// <summary>The font is bold.</summary>
		Bold = 1 << 0,
		/// <summary>The font is italic.</summary>
		Italic = 1 << 1
	}

	/// <summary>Converts a string into a <see cref="Microsoft.Maui.Controls.FontAttributes"/> object.</summary>
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
			if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
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
