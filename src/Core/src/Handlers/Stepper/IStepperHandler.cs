#if IOS || MACCATALYST
using PlatformView = UIKit.UIStepper;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
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