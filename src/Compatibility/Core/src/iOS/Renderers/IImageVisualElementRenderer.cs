#if __MOBILE__
using PlatformImage = UIKit.UIImage;
using PlatformImageView = UIKit.UIImageView;
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using PlatformImage = AppKit.NSImage;
using PlatformImageView = AppKit.NSImageView;
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public interface IImageVisualElementRenderer : IVisualNativeElementRenderer
	{
		void SetImage(PlatformImage image);
		bool IsDisposed { get; }
		PlatformImageView GetImage();
	}
}