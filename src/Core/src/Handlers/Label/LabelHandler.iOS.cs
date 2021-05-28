using Microsoft.Maui.Platform.iOS;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, MauiLabel>
	{
		protected override MauiLabel CreateNativeView() => new MauiLabel();

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(LabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.WrappedNativeView?.UpdateBackground(label);
		}

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateText(label);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextColor(label);
		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateCharacterSpacing(label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateMaxLines(label);
		}

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdatePadding(label);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextDecorations(label);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(label, fontManager);
		}

		public static void MapLineHeight(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateLineHeight(label);
		}

		public static void MapFormatting(LabelHandler handler, ILabel label)
		{
			// Update all of the attributed text formatting properties
			handler.NativeView?.UpdateLineHeight(label);
			handler.NativeView?.UpdateTextDecorations(label);
			handler.NativeView?.UpdateCharacterSpacing(label);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.NativeView?.UpdateHorizontalTextAlignment(label);
		}
	}
}