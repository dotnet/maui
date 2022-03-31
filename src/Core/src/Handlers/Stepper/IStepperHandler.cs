#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIStepper;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiStepper;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.MauiStepper;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IStepperHandler : IViewHandler
	{
		new IStepper VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}