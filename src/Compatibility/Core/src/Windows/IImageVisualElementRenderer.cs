
namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public interface IImageVisualElementRenderer : IVisualPlatformElementRenderer
	{
		bool IsDisposed { get; }
		void SetImage(Microsoft.UI.Xaml.Media.ImageSource image);
		Microsoft.UI.Xaml.Controls.Image GetImage();
	}
}
