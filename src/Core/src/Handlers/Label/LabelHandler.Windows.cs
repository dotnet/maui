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

		public static partial void MapHeight(ILabelHandler handler, ILabel view) =>
			// VerticalAlignment only works when the container's Height is set and the child's Height is Auto. The child's Height
			// is set to Auto when the container is introduced
			handler.ToPlatform().UpdateHeight(view);

		public static partial void MapBackground(ILabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.ToPlatform().UpdateBackground(label);
		}

		public static void MapOpacity(ILabelHandler handler, ILabel label) =>
			ViewHandler.MapOpacity(handler, label);

		public static partial void MapText(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateText(label);

		public static partial void MapTextColor(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateTextColor(label);

		public static partial void MapCharacterSpacing(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateCharacterSpacing(label);

		public static partial void MapFont(ILabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(label, fontManager);
		}

		public static partial void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);

		public static partial void MapVerticalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.PlatformView?.UpdateVerticalTextAlignment(label);
		}

		public static partial void MapTextDecorations(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateTextDecorations(label);

		public static partial void MapPadding(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdatePadding(label);

		public static partial void MapLineHeight(ILabelHandler handler, ILabel label) =>
			handler.PlatformView?.UpdateLineHeight(label);
	}
}
