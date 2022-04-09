using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler
	{
		new UIView? PlatformView { get; }
		new UIView? ContainerView { get; }
		UIViewController? ViewController { get; }
	}
}