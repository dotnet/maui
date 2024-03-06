#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/FontAttributes.xml" path="Type[@FullName='Microsoft.Maui.Controls.FontAttributes']/Docs/*" />
	[Flags]
	public enum FontAttributes
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/FontAttributes.xml" path="//Member[@MemberName='None']/Docs/*" />
		None = 0,
		/// <include file="../../docs/Microsoft.Maui.Controls/FontAttributes.xml" path="//Member[@MemberName='Bold']/Docs/*" />
		Bold = 1 << 0,
		/// <include file="../../docs/Microsoft.Maui.Controls/FontAttributes.xml" path="//Member[@MemberName='Italic']/Docs/*" />
		Italic = 1 << 1
	}

	/// <include file="../../docs/Microsoft.Maui.Controls/FontAttributesConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.FontAttributesConverter']/Docs/*" />
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

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
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
After:
			{
				return FontAttributes.None;
			}

			FontAttributes attributes = FontAttributes.None;
			strValue = strValue.Trim();
			if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
			{ //Xaml
				foreach (var part in strValue.Split(','))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
			}
			else
			{ //CSS or single value
				foreach (var part in strValue.Split(' '))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
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
After:
			{
				return FontAttributes.None;
			}

			FontAttributes attributes = FontAttributes.None;
			strValue = strValue.Trim();
			if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
			{ //Xaml
				foreach (var part in strValue.Split(','))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
			}
			else
			{ //CSS or single value
				foreach (var part in strValue.Split(' '))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
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
After:
			{
				return FontAttributes.None;
			}

			FontAttributes attributes = FontAttributes.None;
			strValue = strValue.Trim();
			if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
			{ //Xaml
				foreach (var part in strValue.Split(','))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
			}
			else
			{ //CSS or single value
				foreach (var part in strValue.Split(' '))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
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
After:
			{
				return FontAttributes.None;
			}

			FontAttributes attributes = FontAttributes.None;
			strValue = strValue.Trim();
			if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
			{ //Xaml
				foreach (var part in strValue.Split(','))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
			}
			else
			{ //CSS or single value
				foreach (var part in strValue.Split(' '))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
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
After:
			{
				return FontAttributes.None;
			}

			FontAttributes attributes = FontAttributes.None;
			strValue = strValue.Trim();
			if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
			{ //Xaml
				foreach (var part in strValue.Split(','))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
			}
			else
			{ //CSS or single value
				foreach (var part in strValue.Split(' '))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
*/
			{
				return FontAttributes.None;
			}

			FontAttributes attributes = FontAttributes.None;
			strValue = strValue.Trim();
			if (strValue.IndexOf(",", StringComparison.Ordinal) != -1)
			{ //Xaml
				foreach (var part in strValue.Split(','))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
			}
			else
			{ //CSS or single value
				foreach (var part in strValue.Split(' '))
				{
					attributes |= ParseSingleAttribute(part, strValue);
				}
			}
			return attributes;
		}

		static FontAttributes ParseSingleAttribute(string part, string originalvalue)
		{
			part = part.Trim();
			if (Enum.TryParse(part, true, out FontAttributes attr))

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return attr;
			if (part.Equals("normal", StringComparison.OrdinalIgnoreCase))
				return FontAttributes.None;
After:
			{
				return attr;
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return attr;
			if (part.Equals("normal", StringComparison.OrdinalIgnoreCase))
				return FontAttributes.None;
After:
			{
				return attr;
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return attr;
			if (part.Equals("normal", StringComparison.OrdinalIgnoreCase))
				return FontAttributes.None;
After:
			{
				return attr;
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return attr;
			if (part.Equals("normal", StringComparison.OrdinalIgnoreCase))
				return FontAttributes.None;
After:
			{
				return attr;
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return attr;
			if (part.Equals("normal", StringComparison.OrdinalIgnoreCase))
				return FontAttributes.None;
After:
			{
				return attr;
*/
			{
				return attr;
			}

			if (part.Equals("normal", StringComparison.OrdinalIgnoreCase))
			{
			{
				return FontAttributes.None;
			}

			if (part.Equals("oblique", StringComparison.OrdinalIgnoreCase))
			{
				return FontAttributes.Italic;
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", originalvalue, typeof(FontAttributes)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not FontAttributes attr)
			{
				throw new NotSupportedException();
			}

			if (attr == FontAttributes.None)
			{
				return "";
			}

			var parts = new List<string>();
			if ((attr & FontAttributes.Bold) == FontAttributes.Bold)
			{
				parts.Add(nameof(FontAttributes.Bold));
			}

			if ((attr & FontAttributes.Italic) == FontAttributes.Italic)
			{
				parts.Add(nameof(FontAttributes.Italic));
			}

			return string.Join("' ", parts);
		}
	}
}
