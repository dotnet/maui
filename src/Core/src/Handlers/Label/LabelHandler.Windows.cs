#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, TextBlock>
	{
		protected override TextBlock CreatePlatformView() => new TextBlock();

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(LabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.ToPlatform().UpdateBackground(label);
		}

		public static void MapOpacity(LabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.PlatformView.UpdateOpacity(label);
			handler.ToPlatform().UpdateOpacity(label);
		}

		public static void MapText(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateText(label);

		public static void MapTextColor(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateTextColor(label);

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateCharacterSpacing(label);

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(label, fontManager);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);

		public static void MapVerticalTextAlignment(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(label);

		public static void MapLineBreakMode(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateLineBreakMode(label);

		public static void MapTextDecorations(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateTextDecorations(label);

		public static void MapMaxLines(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateMaxLines(label);

		public static void MapPadding(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdatePadding(label);

		public static void MapLineHeight(LabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateLineHeight(label);
	}
}
