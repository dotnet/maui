using System;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, Label>
	{
		protected override Label CreatePlatformView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			var label = new Label(NativeParent)
			{
				// Fix me : it is workaround code, LineBreakMode is not working when Label was measured but we set LineBreakMode as WordWrap at initialize time, it works
				LineBreakMode = Tizen.UIExtensions.Common.LineBreakMode.WordWrap
			};
			return label;
		}

		public static void MapBackground(LabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(label);
		}

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateText(label);
			
			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextColor(label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapVerticalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextDecorations(label);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(label, fontManager);
		}

		public static void MapFormatting(LabelHandler handler, ILabel label)
		{
			// Update all of the attributed text formatting properties
			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);
			handler.PlatformView?.UpdateTextDecorations(label);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(LabelHandler handler, ILabel label) {}

		[MissingMapper]
		public static void MapLineHeight(LabelHandler handler, ILabel label) {}

		[MissingMapper]
		public static void MapMaxLines(LabelHandler handler, ILabel label) {}

		[MissingMapper]
		public static void MapPadding(LabelHandler handler, ILabel label) {}
	}
}