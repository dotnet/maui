#nullable enable
#if __IOS__ || MACCATALYST || WINDOWS
using NativeView = Microsoft.Maui.Platform.MauiCheckBox;
#elif __ANDROID__
using NativeView = AndroidX.AppCompat.Widget.AppCompatCheckBox;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface ICheckBoxHandler : IViewHandler
	{
		new ICheckBox VirtualView { get; }
		new NativeView NativeView { get; }
	}
}