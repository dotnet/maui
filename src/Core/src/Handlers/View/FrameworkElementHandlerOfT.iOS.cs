using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class FrameworkElementHandler<TVirtualView, TNativeView> : INativeViewHandler
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

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}