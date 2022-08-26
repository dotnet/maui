using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, Label>
	{
		protected override Label CreatePlatformView() => new();

		public static void MapBackground(ILabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(label);
		}

		public static void MapText(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateText(label);
		}

		public static void MapTextColor(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextColor(label);
		}

		public static void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapVerticalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(label);
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

		public static void MapShadow(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateShadow(label);
		}

		public static void MapCharacterSpacing(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView.UpdateCharacterSpacing(label);
		}

		public static void MapLineHeight(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView.UpdateLineHeight(label);
		}

		[MissingMapper]
		public static void MapPadding(ILabelHandler handler, ILabel label) { }
	}
}