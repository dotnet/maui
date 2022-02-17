#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIImageView;
#elif MONOANDROID
using PlatformView = Android.Widget.ImageView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Image;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IImageHandler : IViewHandler
	{
		IImage TypedVirtualView { get; }
		ImageSourcePartLoader SourceLoader { get; }
		PlatformView TypedPlatformView { get; }
	}
}