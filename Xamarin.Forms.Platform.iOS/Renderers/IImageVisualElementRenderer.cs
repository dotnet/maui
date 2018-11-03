using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IImageVisualElementRenderer : IVisualNativeElementRenderer
	{
		void SetImage(UIImage image);
		bool IsDisposed { get; }
		UIImageView GetImage();
	}
}