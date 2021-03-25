using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platform.iOS;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : AbstractViewHandler<ILabel, MauiLabel>
	{
		protected override MauiLabel CreateNativeView() => new MauiLabel();

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateText(label);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateTextColor(label);
		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			MapFormatting(handler, label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateMaxLines(label);
		}

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdatePadding(label);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			MapFormatting(handler, label);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(label, fontManager);
		}

		public static void MapLineHeight(LabelHandler handler, ILabel label)
		{
			MapFormatting(handler, label);
		}

		public static void MapFormatting(LabelHandler handler, ILabel label)
		{
			// Update all of the attributed text formatting properties
			handler.TypedNativeView?.UpdateLineHeight(label);
			handler.TypedNativeView?.UpdateTextDecorations(label);
			handler.TypedNativeView?.UpdateCharacterSpacing(label);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(label);
		}
	}
}