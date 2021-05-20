using UIKit;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IFrameworkElementHandler
	{
		new UIView? NativeView { get; }
		UIViewController? ViewController { get; }
	}
}