using System.Maui.Core.Controls;

#if __MOBILE__
using NativeColor = UIKit.UIColor;
#else
using NativeColor = AppKit.NSColor;
#endif

namespace System.Maui.Platform {
	public partial class AbstractViewRenderer<TVirtualView, TNativeView> : INativeViewRenderer {

		public void SetFrame (Rectangle rect) => View.Frame = rect.ToCGRect ();

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var s = TypedNativeView.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));
			var request = new Size(s.Width == float.PositiveInfinity ? double.PositiveInfinity : s.Width,
				s.Height == float.PositiveInfinity ? double.PositiveInfinity : s.Height);
			return new SizeRequest(request);
		}

		void SetupContainer()
		{
			var oldParent = TypedNativeView.Superview;
			ContainerView ??= new ContainerView ();
			if (oldParent == ContainerView)
				return;
			ContainerView.MainView = TypedNativeView;
		}
		void RemoveContainer()
		{

		}
		
	}
}
