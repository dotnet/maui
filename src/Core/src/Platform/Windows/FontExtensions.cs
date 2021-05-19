using Microsoft.UI.Text;
using Windows.UI.Text;
using FontWeights = Microsoft.UI.Text.FontWeights;
using FWeight = Windows.UI.Text.FontWeight;
namespace Microsoft.Maui
{
	public static class FontExtensions
	{
		public static FontStyle ToFontStyle(this Font fontAttributes) =>
				fontAttributes.FontSlant switch
				{
					FontSlant.Italic => FontStyle.Italic,
					FontSlant.Oblique => FontStyle.Oblique,
					_ => FontStyle.Normal,
				};

		public static FWeight ToFontWeight(this Font font) => font.Weight switch
		{
			FontWeight.Black => FontWeights.Black,
			FontWeight.Bold => FontWeights.Bold,
			FontWeight.Heavy => FontWeights.ExtraBold,
			FontWeight.Light => FontWeights.Light,
			FontWeight.Medium => FontWeights.Medium,
			FontWeight.Regular => FontWeights.Normal,
			FontWeight.Semibold => FontWeights.SemiBold,
			FontWeight.Thin => FontWeights.Thin,
			FontWeight.Ultralight => FontWeights.ExtraLight,
			FontWeight f => new FWeight((ushort)f)
		};
	}
}