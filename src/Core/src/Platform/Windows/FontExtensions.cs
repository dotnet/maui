using Microsoft.UI.Text;
using Windows.UI.Text;

namespace Microsoft.Maui
{
	public static class FontExtensions
	{
		public static FontStyle ToFontStyle(this FontAttributes fontAttributes) =>
			fontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;

		public static FontWeight ToFontWeight(this FontAttributes fontAttributes) =>
			fontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
	}
}