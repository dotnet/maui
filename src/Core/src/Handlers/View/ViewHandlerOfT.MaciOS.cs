using Microsoft.Maui;

#if __IOS__ || IOS || MACCATALYST
using NativeColor = UIKit.UIColor;
#else
using NativeColor = AppKit.NSColor;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		public override void SetFrame(Rectangle rect)
		{
			if (View != null)
				View.Frame = rect.ToCGRect();
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (View == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var sizeThatFits = View.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));

			var size = new Size(
				sizeThatFits.Width == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Width,
				sizeThatFits.Height == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Height);

			if (double.IsInfinity(size.Width) || double.IsInfinity(size.Height))
			{
				View.SizeToFit();

				size = new Size(View.Frame.Width, View.Frame.Height);
			}

			return size;
		}

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{


		}
	}
}