
namespace System.Maui.Platform.UWP
{
	public interface IImageVisualElementRenderer : IVisualNativeElementRenderer
	{
		bool IsDisposed { get; }
		void SetImage(global::Windows.UI.Xaml.Media.ImageSource image);
		global::Windows.UI.Xaml.Controls.Image GetImage();
	}
}
