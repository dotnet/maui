#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Google.Android.Material.Button.MaterialButton;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Button;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
#if WINDOWS
	public partial interface IButtonHandler2 : IViewHandler
#else
	public partial interface IButtonHandler2 : IButtonHandler
#endif
	{

#if WINDOWS
		new PlatformView PlatformView { get; }
		new IButton VirtualView { get; }
		ImageSourcePartLoader ImageSourceLoader { get; }
#endif
	}
}
