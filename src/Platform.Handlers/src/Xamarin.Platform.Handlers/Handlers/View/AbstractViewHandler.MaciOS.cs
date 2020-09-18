using Xamarin.Forms;

#if __IOS__
using NativeColor = UIKit.UIColor;
#else
using NativeColor = AppKit.NSColor;
#endif

namespace Xamarin.Platform.Handlers
{
	public partial class AbstractViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		public void SetFrame(Rectangle rect) => View.Frame = rect.ToCGRect();

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var s = TypedNativeView.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));
			var request = new Size(s.Width == float.PositiveInfinity ? double.PositiveInfinity : s.Width,
				s.Height == float.PositiveInfinity ? double.PositiveInfinity : s.Height);
			return new SizeRequest(request);
		}

		void SetupContainer()
		{

		}

		void RemoveContainer()
		{


		}
	}
}