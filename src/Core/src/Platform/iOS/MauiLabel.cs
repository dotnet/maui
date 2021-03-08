using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform.iOS
{
	public class MauiLabel : UILabel
	{
		public UIEdgeInsets TextInsets { get; set; }

		public MauiLabel(CGRect frame) : base(frame)
		{
		}

		public MauiLabel()
		{
		}

		public override void DrawText(CGRect rect) => base.DrawText(TextInsets.InsetRect(rect));

		public override CGSize SizeThatFits(CGSize size) => AddInsets(base.SizeThatFits(size));

		CGSize AddInsets(CGSize size) => new CGSize(
			width: size.Width + TextInsets.Left + TextInsets.Right,
			height: size.Height + TextInsets.Top + TextInsets.Bottom);
	}
}
