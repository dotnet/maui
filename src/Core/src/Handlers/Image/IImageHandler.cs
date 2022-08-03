#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIImageView;
#elif MONOANDROID
using PlatformView = Android.Widget.ImageView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Image;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.Image;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IImageHandler : IViewHandler
	{
		new IImage VirtualView { get; }
		ImageSourcePartLoader SourceLoader { get; }
		new PlatformView PlatformView { get; }
	}
}