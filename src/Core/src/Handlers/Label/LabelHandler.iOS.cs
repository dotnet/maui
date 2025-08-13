using System;
using Microsoft.Maui.Graphics;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, MauiLabel>
	{
		protected override MauiLabel CreatePlatformView() => new MauiLabel();

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(ILabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.ToPlatform().UpdateBackground(label);
		}

		public static void MapText(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextPlainText(label);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, label);
		}

		public static void MapTextColor(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextColor(label);
		}

		public static void MapCharacterSpacing(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateCharacterSpacing(label);
		}

		public static void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapVerticalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(label);
		}

		public static void MapPadding(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdatePadding(label);
		}

		public static void MapTextDecorations(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextDecorations(label);
		}

		public static void MapFont(ILabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(label, fontManager);
		}

		public static void MapLineHeight(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateLineHeight(label);
		}

		public static void MapFormatting(ILabelHandler handler, ILabel label)
		{
			// Update all of the attributed text formatting properties
			handler.UpdateValue(nameof(ILabel.LineHeight));
			handler.UpdateValue(nameof(ILabel.TextDecorations));
			handler.UpdateValue(nameof(ILabel.CharacterSpacing));

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.UpdateValue(nameof(ILabel.HorizontalTextAlignment));
		}
	}
}