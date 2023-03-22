#nullable disable
namespace Microsoft.Maui.Controls.Platform
{
	public static class FontExtensions
	{
		public static string ToNativeFontFamily(this string self, IFontManager fontManager)
		{
			return fontManager.GetFontFamily(self);
		}
	}
}
