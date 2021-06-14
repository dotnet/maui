using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class Image : IImage
	{
		IImageSource IImageSourcePart.Source => Source;

		public void Refresh()
		{
			InvalidateMeasureInternal(InvalidationTrigger.RendererReady);
			var l = this.FindParentOfType<Layout>();
			l.ForceLayout();
			//(this as IView).InvalidateMeasure();
		}

		void IImageSourcePart.UpdateIsLoading(bool isLoading) =>
			IsLoading = isLoading;
	}

	public partial class ImageSource : IImageSource
	{
	}

	public partial class FileImageSource : IFileImageSource
	{
	}

	public partial class StreamImageSource : IStreamImageSource
	{
	}

	public partial class UriImageSource : IUriImageSource, IStreamImageSource
	{
	}

	public partial class FontImageSource : IFontImageSource
	{
		Font IFontImageSource.Font => Font.OfSize(FontFamily, Size);
	}
}