using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	[Flags]
	public enum FontAttributes
	{
		None = 0,
		Bold = 1 << 0,
		Italic = 1 << 1
	}

	[Xaml.TypeConversion(typeof(FontAttributes))]
	public sealed class FontAttributesConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return FontAttributes.None;

			FontAttributes attributes = FontAttributes.None;
			value = value.Trim();
			if (value.Contains(","))
			{ //Xaml
				foreach (var part in value.Split(','))
					attributes |= ParseSingleAttribute(part, value);

			}
			else
			{ //CSS or single value
				foreach (var part in value.Split(' '))
					attributes |= ParseSingleAttribute(part, value);
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

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is FontAttributes attr))
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