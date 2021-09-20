#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIButton;
#elif MONOANDROID
using NativeView = Google.Android.Material.Button.MaterialButton;
#elif WINDOWS
using NativeView = Microsoft.Maui.MauiButton;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IButtonHandler : IViewHandler
	{
		IButton TypedVirtualView { get; }
		NativeView TypedNativeView { get; }
		ImageSourcePartLoader ImageSourceLoader { get; }


#if __ANDROID__
		// These are a little odd here but Android doesn't let you retrieve these from a control
		ButtonHandler.ButtonClickListener ClickListener { get; }
		ButtonHandler.ButtonTouchListener TouchListener { get; }
#endif

	}
}