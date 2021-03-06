using Microsoft.Maui;

#if __IOS__ || IOS || MACCATALYST
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
			if (TypedNativeView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var sizeThatFits = TypedNativeView.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));

			var size = new Size(
				sizeThatFits.Width == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Width,
				sizeThatFits.Height == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Height);

			if (double.IsInfinity(size.Width) || double.IsInfinity(size.Height))
			{
				TypedNativeView.SizeToFit();

				size = new Size(TypedNativeView.Frame.Width, TypedNativeView.Frame.Height);
			}

			return size;
		}

		void SetupContainer()
		{

		}

		void RemoveContainer()
		{


		}
	}
}