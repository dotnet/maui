#nullable disable

using System;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Platform
{
	public class MauiLabel : UILabel
	{
		ILabel _label;

		public UIEdgeInsets TextInsets { get; set; }

		public MauiLabel(RectangleF frame) : base(frame)
		{
		}

		public MauiLabel()
		{
		}

		public override void DrawText(RectangleF rect) =>
			base.DrawText(TextInsets.InsetRect(rect));

		public override void InvalidateIntrinsicContentSize()
		{
			base.InvalidateIntrinsicContentSize();

			if (Frame.Width == 0 && Frame.Height == 0)
			{
				// The Label hasn't actually been laid out on screen yet; no reason to request a layout
				return;
			}

			if (!Frame.Size.IsCloseTo(AddInsets(IntrinsicContentSize), (nfloat)0.001))
			{
				// The text or its attributes have changed enough that the size no longer matches the set Frame. It's possible
				// that the Label needs to be laid out again at a different size, so we request that the parent do so. 
				Superview?.SetNeedsLayout();
			}
		}

		public override SizeF SizeThatFits(SizeF size) => AddInsets(base.SizeThatFits(size));

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (_label == null)
				return;

			SizeF fitSize;
			nfloat labelHeight;

			switch (_label.VerticalTextAlignment)
			{
				case Maui.TextAlignment.Start:
					fitSize = SizeThatFits(_label.Frame.Size.ToCGSize());
					labelHeight = (nfloat)Math.Min(Bounds.Height, fitSize.Height);
					Frame = new RectangleF(0, 0, (nfloat)_label.Width, labelHeight);
					break;
				case Maui.TextAlignment.Center:
					Frame = new RectangleF(0, 0, (nfloat)_label.Width, (nfloat)_label.Height);
					break;
				case Maui.TextAlignment.End:
					fitSize = SizeThatFits(_label.Frame.Size.ToCGSize());
					labelHeight = (nfloat)Math.Min(Bounds.Height, fitSize.Height);
					nfloat yOffset = (nfloat)(_label.Height - labelHeight);
					Frame = new RectangleF(0, yOffset, (nfloat)_label.Width, labelHeight);
					break;
			}
		}

		public void UpdateVerticalAlignment(ILabel label)
		{
			_label = label;

			LayoutSubviews();
		}

		SizeF AddInsets(SizeF size) => new SizeF(
			width: size.Width + TextInsets.Left + TextInsets.Right,
			height: size.Height + TextInsets.Top + TextInsets.Bottom);
	}
}