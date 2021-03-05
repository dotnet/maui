using Microsoft.Maui.Controls.Internals;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public static partial class FontExtensions
	{
		public static UIFont ToUIFont(this Font self)
			=> Forms.FontManager.GetFont(self);

		internal static UIFont ToUIFont(this IFontElement self)
			=> Forms.FontManager.GetFont(Font.OfSize(self.FontFamily, self.FontSize).WithAttributes(self.FontAttributes));

		internal static bool IsDefault(this IFontElement self)
			=> self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) && self.FontAttributes == FontAttributes.None;
	}
}