#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, TextBlock>
	{
		protected override TextBlock CreatePlatformView() => new TextBlock();

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			(VirtualView != null && VirtualView.VerticalTextAlignment != TextAlignment.Start) ||
			base.NeedsContainer;

		protected override void SetupContainer()
		{
			base.SetupContainer();

			// VerticalAlignment only works when the child's Height is Auto
			PlatformView.Height = double.NaN;

			MapHeight(this, VirtualView);
		}

		protected override void RemoveContainer()
		{
			base.RemoveContainer();

			MapHeight(this, VirtualView);
		}

		public static void MapHeight(ILabelHandler handler, ILabel view) =>
			// VerticalAlignment only works when the container's Height is set and the child's Height is Auto. The child's Height
			// is set to Auto when the container is introduced
			handler.ToPlatform().UpdateHeight(view);

		public static void MapBackground(ILabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.ToPlatform().UpdateBackground(label);
		}

		public static void MapOpacity(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView.UpdateOpacity(label);
			handler.ToPlatform().UpdateOpacity(label);
		}

		public static void MapText(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateText(label);

		public static void MapTextColor(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateTextColor(label);

		public static void MapCharacterSpacing(ILabelHandler handler, ILabel label)
		{
			if (handler.IsConnectingHandler() && label.CharacterSpacing == 0)
			{
				return;
			}

			handler.PlatformView?.UpdateCharacterSpacing(label);
		}

		public static void MapFont(ILabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(label, fontManager);
		}

		public static void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);

		public static void MapVerticalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.PlatformView?.UpdateVerticalTextAlignment(label);
		}

		public static void MapTextDecorations(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateTextDecorations(label);

		public static void MapPadding(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdatePadding(label);

		public static void MapLineHeight(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateLineHeight(label);
	}
}
