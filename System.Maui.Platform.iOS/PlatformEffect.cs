#if __MOBILE__
using UIKit;
namespace Xamarin.Forms.Platform.iOS
#else
using UIView = AppKit.NSView;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public abstract class PlatformEffect : PlatformEffect<UIView, UIView>
	{
	}
}