#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIProgressView;
#elif MONOANDROID
using PlatformView = Android.Widget.ProgressBar;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ProgressBar;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IProgressBarHandler : IViewHandler
	{
		new IProgress VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}