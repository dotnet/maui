using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		UIView? INativeViewHandler.NativeView => (UIView?)base.NativeView;
		UIViewController? INativeViewHandler.ViewController => null;

		public override void NativeArrange(Rectangle rect)
		{
			if (NativeView != null)
			{
				NativeView.Frame = rect.ToCGRect();
				NativeView.UpdateBackgroundLayerFrame();
			}
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (NativeView == null || VirtualView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var explicitWidth = VirtualView.Width;
			var explicitHeight = VirtualView.Height;
			var hasExplicitWidth = explicitWidth >= 0;
			var hasExplicitHeight = explicitHeight >= 0;

			var sizeThatFits = NativeView.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));

			var size = new Size(
				sizeThatFits.Width == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Width,
				sizeThatFits.Height == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Height);

			if (double.IsInfinity(size.Width) || double.IsInfinity(size.Height))
			{
				NativeView.SizeToFit();
				size = new Size(NativeView.Frame.Width, NativeView.Frame.Height);
			}

			return new Size(hasExplicitWidth ? explicitWidth : size.Width,
				hasExplicitHeight ? explicitHeight : size.Height);
		}

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}