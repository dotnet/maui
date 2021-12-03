using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FontExtensions
	{
		internal static void ApplyFont(this UI.Xaml.Documents.TextElement self, Font font) =>
			self.UpdateFont(font, CompatServiceProvider.FontManager);
	}
}