using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, FrameworkElement>
	{
		protected TextBlock? TextBlock { get; set; }

		protected override FrameworkElement CreateNativeView()
		{
			TextBlock = new TextBlock();
			return new Border { Child = TextBlock };
		}

		public static void MapText(LabelHandler handler, ILabel label) =>
			handler.TextBlock?.UpdateText(label);

		public static void MapTextColor(LabelHandler handler, ILabel label) =>
			handler.TextBlock?.UpdateTextColor(label);

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label) { }

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TextBlock?.UpdateFont(label, fontManager);
		}
    
		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label) { }

		public static void MapLineBreakMode(LabelHandler handler, ILabel label) { }

		public static void MapTextDecorations(LabelHandler handler, ILabel label) { }

		public static void MapMaxLines(LabelHandler handler, ILabel label) { }

		public static void MapPadding(LabelHandler handler, ILabel label) =>
			handler.TextBlock?.UpdatePadding(label);

		public static void MapLineHeight(LabelHandler handler, ILabel label) { }
	}
}