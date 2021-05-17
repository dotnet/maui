using System.Runtime.InteropServices;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class FontExtensions
	{
		public static string ToNativeFontFamily(this string self)
		{
			return CompatServiceProvider.FontManager.GetFontFamily(self);
		}
	}
}
