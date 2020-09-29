using System;

namespace Xamarin.Forms
{
	[Flags]
	[TypeConverter(typeof(FontAttributesConverter))]
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
	}
}