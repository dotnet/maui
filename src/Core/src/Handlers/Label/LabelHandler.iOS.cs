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


		public override void PlatformArrange(Rect rect)
		{
			base.PlatformArrange(rect);

			if (VirtualView == null)
				return;

			SizeF fitSize;
			double fitHeight;
			var bounds = PlatformView.Bounds;
			var offsetFromParent = rect.Y;

			// VerticalTextAlignment currently doesn't work
			// if the label is inside a container
			// Because the wrapper view resets the frame on the
			// wrapped view
			if (NeedsContainer)
				offsetFromParent = 0;

			switch (VirtualView.VerticalTextAlignment)
			{
				case Maui.TextAlignment.Start:
					fitSize = PlatformView.SizeThatFits(rect.Size.ToCGSize());
					fitHeight = Math.Min(bounds.Height, fitSize.Height);
					var startFrame = new RectangleF(rect.X, offsetFromParent, rect.Width, fitHeight);

					if (startFrame != RectangleF.Empty)
						PlatformView.Frame = startFrame;

					break;

				case Maui.TextAlignment.Center:
					break;

				case Maui.TextAlignment.End:
					fitSize = PlatformView.SizeThatFits(rect.Size.ToCGSize());
					fitHeight = Math.Min(bounds.Height, fitSize.Height);

					var yOffset = offsetFromParent + rect.Height - fitHeight;
					var endFrame = new RectangleF(rect.X, yOffset, rect.Width, fitHeight);

					if (endFrame != RectangleF.Empty)
						PlatformView.Frame = endFrame;

					break;
			}
		}

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
			handler.PlatformView?.UpdateLineHeight(label);
			handler.PlatformView?.UpdateTextDecorations(label);
			handler.PlatformView?.UpdateCharacterSpacing(label);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);
		}
	}
}