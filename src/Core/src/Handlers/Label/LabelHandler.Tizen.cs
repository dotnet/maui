using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, Label>
	{
		protected override Label CreatePlatformView() => new();

		public static partial void MapBackground(ILabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(label);
		}

		public static partial void MapText(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateText(label);
		}

		public static partial void MapTextColor(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextColor(label);
		}

		public static partial void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);
		}

		public static partial void MapVerticalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(label);
		}

		public static partial void MapTextDecorations(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextDecorations(label);
		}

		public static partial void MapFont(ILabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(label, fontManager);
		}

		public static partial void MapShadow(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateShadow(label);
		}

		public static partial void MapCharacterSpacing(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView.UpdateCharacterSpacing(label);
		}

		public static partial void MapLineHeight(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView.UpdateLineHeight(label);
		}

		[MissingMapper]
		public static partial void MapPadding(ILabelHandler handler, ILabel label) { }
	}
}