using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
	static class DecorableTextElement
	{
		public static readonly BindableProperty TextDecorationsProperty = BindableProperty.Create(nameof(IDecorableTextElement.TextDecorations), typeof(TextDecorations), typeof(IDecorableTextElement), TextDecorations.None);
	}

	[Xaml.TypeConversion(typeof(TextDecorations))]
	public class TextDecorationConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			TextDecorations result = TextDecorations.None;
			if (value == null)
				throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(TextDecorations)));

			var valueArr = value.Split(',');

			if (valueArr.Length <= 1)
				valueArr = value.Split(' ');

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
	}

}