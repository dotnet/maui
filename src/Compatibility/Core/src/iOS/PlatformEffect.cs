#if __MOBILE__
using UIKit;
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using UIView = AppKit.NSView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public abstract class PlatformEffect : PlatformEffect<UIView, UIView>
	{
	}
}