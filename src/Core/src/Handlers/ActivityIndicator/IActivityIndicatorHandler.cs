#if __IOS__ || MACCATALYST
using NativeView = Microsoft.Maui.Platform.MauiActivityIndicator;
#elif MONOANDROID
using NativeView = Android.Widget.ProgressBar;
#elif WINDOWS
using NativeView = Microsoft.Maui.Platform.MauiActivityIndicator;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IActivityIndicatorHandler : IViewHandler
	{
		new IActivityIndicator VirtualView { get; }
		new NativeView NativeView { get; }
	}
}