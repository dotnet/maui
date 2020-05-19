#if __MOBILE__
using UIKit;
namespace System.Maui.Platform.iOS
#else
using UIView = AppKit.NSView;

namespace System.Maui.Platform.MacOS
#endif
{
	public abstract class PlatformEffect : PlatformEffect<UIView, UIView>
	{
	}
}