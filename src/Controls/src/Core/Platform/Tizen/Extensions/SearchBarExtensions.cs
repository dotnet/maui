#nullable disable
using Microsoft.Maui.Controls;
using TFontAttributes = Tizen.UIExtensions.Common.FontAttributes;

namespace Microsoft.Maui.Controls.Platform
{
	public static class SearchBarExtensions
	{

		public static TFontAttributes ToPlatform(this FontAttributes fontAttribute)
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
