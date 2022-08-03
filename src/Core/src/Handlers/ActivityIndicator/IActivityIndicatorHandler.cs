#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiActivityIndicator;
#elif MONOANDROID
using PlatformView = Android.Widget.ProgressBar;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ProgressRing;
#elif TIZEN
using PlatformView = ElmSharp.ProgressBar;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IActivityIndicatorHandler : IViewHandler
	{
		new IActivityIndicator VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}