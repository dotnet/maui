#nullable disable

using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiLabel : UILabel
	{
		public ILabel VirtualView { get; set; }
		public UIEdgeInsets TextInsets { get; set; }

		public MauiLabel(CGRect frame) : base(frame)
		{
		}

		public MauiLabel()
		{
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (VirtualView == null)
				return;

			CGSize fitSize;
			nfloat labelHeight;

			switch (VirtualView.VerticalTextAlignment)
			{
				case Maui.TextAlignment.Start:
					fitSize = SizeThatFits(VirtualView.Frame.Size.ToCGSize());
					labelHeight = (nfloat)Math.Min(Bounds.Height, fitSize.Height);
					Frame = new CGRect(0, 0, (nfloat)VirtualView.Width, labelHeight);
					break;
				case Maui.TextAlignment.Center:
					Frame = new CGRect(0, 0, (nfloat)VirtualView.Width, (nfloat)VirtualView.Height);
					break;
				case Maui.TextAlignment.End:
					fitSize = SizeThatFits(VirtualView.Frame.Size.ToCGSize());
					labelHeight = (nfloat)Math.Min(Bounds.Height, fitSize.Height);
					nfloat yOffset = (nfloat)(VirtualView.Height - labelHeight);
					Frame = new CGRect(0, yOffset, (nfloat)VirtualView.Width, labelHeight);
					break;
			}

			// TODO: Recalculate Span positions.
		}

		public override void DrawText(CGRect rect) =>
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

		public override CGSize SizeThatFits(CGSize size) 
			=> AddInsets(base.SizeThatFits(size));

		CGSize AddInsets(CGSize size) => new CGSize(
			width: size.Width + TextInsets.Left + TextInsets.Right,
			height: size.Height + TextInsets.Top + TextInsets.Bottom);
	}
}