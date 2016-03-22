using System.ComponentModel;
#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public abstract class PlatformEffect : PlatformEffect<UIView, UIView>
	{
	}
}