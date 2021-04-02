using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platform.iOS;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, MauiLabel>
	{
		protected override MauiLabel CreateNativeView() => new MauiLabel();

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateText(label);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateTextColor(label);
		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			MapFormatting(handler, label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdateMaxLines(label);
		}

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.View?.UpdatePadding(label);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			MapFormatting(handler, label);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.View?.UpdateFont(label, fontManager);
		}

		public static void MapLineHeight(LabelHandler handler, ILabel label)
		{
			MapFormatting(handler, label);
		}

		public static void MapFormatting(LabelHandler handler, ILabel label)
		{
			// Update all of the attributed text formatting properties
			handler.View?.UpdateLineHeight(label);
			handler.View?.UpdateTextDecorations(label);
			handler.View?.UpdateCharacterSpacing(label);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.View?.UpdateHorizontalTextAlignment(label);
		}
	}
}