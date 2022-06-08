#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Google.Android.Material.Button.MaterialButton;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Button;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.Button;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IButtonHandler : IViewHandler
	{
		new IButton VirtualView { get; }
		new PlatformView PlatformView { get; }
		ImageSourcePartLoader ImageSourceLoader { get; }
	}
}
