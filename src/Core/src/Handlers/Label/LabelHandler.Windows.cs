#nullable enable
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

		[MissingMapper]
		public static void MapCharacterSpacing(LabelHandler handler, ILabel label) { }

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.TextBlock?.UpdateFont(label, fontManager);
		}

		[MissingMapper]
		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label) { }

		[MissingMapper]
		public static void MapLineBreakMode(LabelHandler handler, ILabel label) { }

		[MissingMapper]
		public static void MapTextDecorations(LabelHandler handler, ILabel label) { }

		public static void MapMaxLines(LabelHandler handler, ILabel label) =>
			handler.TextBlock?.UpdateMaxLines(label);

		public static void MapPadding(LabelHandler handler, ILabel label) =>
			handler.TextBlock?.UpdatePadding(label);

		[MissingMapper]
		public static void MapLineHeight(LabelHandler handler, ILabel label) { }
	}
}
