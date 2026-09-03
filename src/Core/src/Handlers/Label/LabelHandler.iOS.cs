using System;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Primitives.Dimension;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, MauiLabel>
	{
		protected override MauiLabel CreatePlatformView() => new MauiLabel();

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (VirtualView is not null && IsExplicitSet(VirtualView.Width))
			{
				widthConstraint = Math.Min(widthConstraint, VirtualView.Width);
				PlatformView.PreferredMaxLayoutWidth = (nfloat)VirtualView.Width;
			}
			else
			{
				PlatformView.PreferredMaxLayoutWidth = 0;
			}

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(ILabelHandler handler, ILabel label)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			if (label.Background.IsNullOrEmpty())
			{
				var containerView = handler.ContainerView as UIView;
				if (handler.PlatformView is not null)
				{
					// UpdateBackground returns early for non-LayoutView/ContentView types (e.g. UILabel),
					// leaving any previously applied solid BackgroundColor in place. Explicitly clear it
					// and remove any residual gradient layer so the label returns to transparent default.
					handler.PlatformView.RemoveBackgroundLayer();
					handler.PlatformView.BackgroundColor = UIColor.Clear;
				}
				// Container only survives here for non-null-but-empty paints or when Clip/Shadow/Mask keep base.NeedsContainer true.
				if (containerView is not null)
				{
					containerView.RemoveBackgroundLayer();
					containerView.BackgroundColor = UIColor.Clear;
				}

				return;
			}

			// Gradient sublayers cover UILabel text, so route them to WrapperView; solid colors stay on PlatformView for correct Clip masking.
			if (label.Background is GradientPaint)
			{
				// A previous solid paint leaves an opaque BackgroundColor on the UILabel that would
				// sit on top of the WrapperView's gradient and hide it. Clear it before applying the gradient.
				if (handler.PlatformView is not null)
					handler.PlatformView.BackgroundColor = UIColor.Clear;

				handler.ToPlatform()?.UpdateBackground(label);
			}
			else
			{
				// Symmetric to the gradient branch: a previous gradient CALayer lives on the WrapperView
				// and would show through the new (semi-transparent) solid paint. Remove it before applying.
				(handler.ContainerView as UIView)?.RemoveBackgroundLayer();
				handler.PlatformView?.UpdateBackground(label);
			}
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