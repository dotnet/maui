using AppKit;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class FlippedClipView : NSClipView
	{
		public override bool IsFlipped
		{
			get
			{
				return true;
			}
		}

		public IVisualElementRenderer ContentRenderer { get; set; }

		public override RectangleF ConstrainBoundsRect(RectangleF proposedBounds)
		{
			RectangleF ret = base.ConstrainBoundsRect(proposedBounds);

			if (ContentRenderer == null || ContentRenderer.Element == null)
				return ret;

			if (Frame.Height > ContentRenderer.Element.Height)
				ret.Y = (float)(Frame.Height - ContentRenderer.Element.Height - ContentRenderer.Element.Y - ContentRenderer.Element.Y);

			return ret;
		}
	}
}
