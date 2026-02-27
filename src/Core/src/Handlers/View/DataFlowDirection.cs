#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif __ANDROID__
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM)
#endif

namespace Microsoft.Maui.Handlers
{
	//	Allows mappings to make decisions based on whether the cross-platform properties are updating the platform UI or vice-versa
	public enum DataFlowDirection
	{
		ToPlatform,
		FromPlatform
	}
}
