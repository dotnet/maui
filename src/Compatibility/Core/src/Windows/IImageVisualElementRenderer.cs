
namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public interface IImageVisualElementRenderer : IVisualNativeElementRenderer
	{
		bool IsDisposed { get; }
		void SetImage(Microsoft.UI.Xaml.Media.ImageSource image);
		Microsoft.UI.Xaml.Controls.Image GetImage();
	}
}
