using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	static class DecorableTextElement
	{
		public static readonly BindableProperty TextDecorationsProperty = BindableProperty.Create(nameof(IDecorableTextElement.TextDecorations), typeof(TextDecorations), typeof(IDecorableTextElement), TextDecorations.None);
	}

	/// <include file="../../docs/Microsoft.Maui.Controls/TextDecorationConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.TextDecorationConverter']/Docs/*" />
	public class TextDecorationConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			TextDecorations result = TextDecorations.None;
			if (strValue == null)
				throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(TextDecorations)));

			var valueArr = strValue.Split(',');

			if (valueArr.Length <= 1)
				valueArr = strValue.Split(' ');

			foreach (var item in valueArr)
			{
				if (Enum.TryParse(item.Trim(), true, out TextDecorations textDecorations))
					result |= textDecorations;
				else if (item.Equals("line-through", StringComparison.OrdinalIgnoreCase))
					result |= TextDecorations.Strikethrough;
				else
					throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", item, typeof(TextDecorations)));
			}

			return result;
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not TextDecorations td)
				throw new NotSupportedException();
			if (td == TextDecorations.None)
				return nameof(TextDecorations.None);
			if (td == TextDecorations.Underline)
				return nameof(TextDecorations.Underline);
			if (td == TextDecorations.Strikethrough)
				return nameof(TextDecorations.Strikethrough);
			if (td == (TextDecorations.Underline & TextDecorations.Strikethrough))
				return $"{nameof(TextDecorations.Underline)}, {nameof(TextDecorations.Strikethrough)}";
			throw new NotSupportedException();
		}
	}
}

