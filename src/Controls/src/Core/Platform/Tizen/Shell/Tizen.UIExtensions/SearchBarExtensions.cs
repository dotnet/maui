using Microsoft.Maui.Controls;
using TFontAttributes = Tizen.UIExtensions.Common.FontAttributes;

namespace Tizen.UIExtensions.Shell
{
	public static class SearchBarExtensions
	{

		public static TFontAttributes ToNative(this FontAttributes fontAttribute)
		{
			TFontAttributes attributes = TFontAttributes.None;
			if (fontAttribute == FontAttributes.Italic)
				attributes = attributes | TFontAttributes.Italic;

			if (fontAttribute == FontAttributes.Bold)
				attributes = attributes | TFontAttributes.Bold;

			return attributes;
		}
	}
}
