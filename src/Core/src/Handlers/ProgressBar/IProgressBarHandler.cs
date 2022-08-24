#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIProgressView;
#elif MONOANDROID
using PlatformView = Android.Widget.ProgressBar;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ProgressBar;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.GraphicsView.ProgressBar;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
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