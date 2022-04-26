#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Google.Android.Material.ImageView.ShapeableImageView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Button;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiImageButton;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public interface IImageButtonHandler : IImageHandler
	{
		new IImageButton VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}