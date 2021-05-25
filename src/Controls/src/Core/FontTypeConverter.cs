using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Controls
{
	[Xaml.TypeConversion(typeof(Font))]
	public sealed class FontTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			// string should be formatted as "[name],[attributes],[size]" there may be multiple attributes, e.g. "Georgia, Bold, Italic, 42"
			if (value != null)
			{
				// trim because mono implements Enum.Parse incorrectly and fails to trim correctly.
				List<string> parts = value.Split(',').Select(s => s.Trim()).ToList();

				string name = null;
				var bold = false;
				var italic = false;
				double size = -1;
				NamedSize namedSize = 0;

				// check if last is a size
				string last = parts.Last();

				if (double.TryParse(last, NumberStyles.Number, CultureInfo.InvariantCulture, out var trySize))
				{
					size = trySize;
					parts.RemoveAt(parts.Count - 1);
				}
				else if (Enum.TryParse(last, out NamedSize tryNamedSize))
				{
					namedSize = tryNamedSize;
					parts.RemoveAt(parts.Count - 1);
				}

				// check if first is a name
				foreach (string part in parts)
				{
					if (Enum.TryParse(part, out FontAttributes tryAttibute))
					{
						// they did not provide a font name
						if (tryAttibute == FontAttributes.Bold)
							bold = true;
						else if (tryAttibute == FontAttributes.Italic)
							italic = true;
					}
					else
					{
						// they may have provided a font name
						if (name != null)
							throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Font)));

						name = part;
					}
				}

				FontAttributes attributes = 0;
				if (bold)
					attributes |= FontAttributes.Bold;
				if (italic)
					attributes |= FontAttributes.Italic;
				if (size == -1 && namedSize == 0)
					namedSize = NamedSize.Medium;

				if (name != null)
				{
					if (size == -1)
						size = Device.GetNamedSize(namedSize, null, false);
					return Font.OfSize(name, size).WithAttributes(attributes);
				}
				if (size == -1)
					return Font.SystemFontOfSize(Device.GetNamedSize(namedSize, null, false)).WithAttributes(attributes);
				return Font.SystemFontOfSize(size).WithAttributes(attributes);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Font)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is Font font))
				throw new NotSupportedException();
			var parts = new List<string>();
			if (!string.IsNullOrEmpty(font.FontFamily))
				parts.Add(font.FontFamily);
			if (font.Weight == FontWeight.Bold)
				parts.Add("Bold");
			if (font.FontSlant != FontSlant.Default)
				parts.Add("Italic");
			parts.Add($"{font.FontSize}");
			return string.Join(", ", parts);
		}
	}
}