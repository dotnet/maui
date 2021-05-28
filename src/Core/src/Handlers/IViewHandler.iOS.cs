using UIKit;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler
	{
		new UIView? NativeView { get; }
		new UIView? ContainerView { get; }
		UIViewController? ViewController { get; }
	}
}