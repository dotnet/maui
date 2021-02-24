using Microsoft.Maui;

#if __IOS__
using NativeColor = UIKit.UIColor;
#else
using NativeColor = AppKit.NSColor;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class AbstractViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		public void SetFrame(Rectangle rect)
		{
			if (View != null)
				View.Frame = rect.ToCGRect();
		}

		public virtual Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var sizeThatFits = TypedNativeView?.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));

			if (sizeThatFits.HasValue)
			{
				return new Size(
					sizeThatFits.Value.Width == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Value.Width,
					sizeThatFits.Value.Height == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Value.Height);
			}

			return new Size(widthConstraint, heightConstraint);
		}

		void SetupContainer()
		{

		}

		void RemoveContainer()
		{


		}
	}
}