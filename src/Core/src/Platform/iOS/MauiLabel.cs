using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform.iOS
{
	public class MauiLabel : UILabel
	{
		public UIEdgeInsets TextInsets { get; set; }

		public CALayer? BackgroundLayer { get; private set; }

		public MauiLabel(CGRect frame) : base(frame)
		{
		}

		public MauiLabel()
		{
		}

		public void SetBackground(IBrush? brush)
		{
			if (brush == null)
				BackgroundLayer = null;
			else
				BackgroundLayer = this.CreateBackgroundLayer(brush);
		}

		public override void DrawText(CGRect rect)
		{
			if (BackgroundLayer != null)
			{
				var context = UIGraphics.GetCurrentContext();
				context.SaveState();

				BackgroundLayer?.RenderInContext(context);

				context.RestoreState();
			}

			base.DrawText(TextInsets.InsetRect(rect));
		}

		public override CGSize SizeThatFits(CGSize size) => AddInsets(base.SizeThatFits(size));

		CGSize AddInsets(CGSize size) => new CGSize(
			width: size.Width + TextInsets.Left + TextInsets.Right,
			height: size.Height + TextInsets.Top + TextInsets.Bottom);
	}
}