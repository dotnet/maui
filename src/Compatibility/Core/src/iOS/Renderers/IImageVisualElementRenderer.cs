#if __MOBILE__
using NativeImage = UIKit.UIImage;
using NativeImageView = UIKit.UIImageView;
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using NativeImageView = AppKit.NSImageView;
using NativeImage = AppKit.NSImage;
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public interface IImageVisualElementRenderer : IVisualNativeElementRenderer
	{
		void SetImage(NativeImage image);
		bool IsDisposed { get; }
		NativeImageView GetImage();
	}
}