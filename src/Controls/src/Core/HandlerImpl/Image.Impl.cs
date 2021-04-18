namespace Microsoft.Maui.Controls
{
	public partial class Image : IImage
	{
		IImageSource IImageSourcePart.Source => Source;

		void IImageSourcePart.UpdateIsLoading(bool isLoading) =>
			IsLoading = isLoading;
	}

	public partial class ImageSource : Maui.IImageSource
	{
	}

	public partial class FileImageSource : Maui.IFileImageSource
	{
	}

	public partial class StreamImageSource : Maui.IStreamImageSource
	{
	}

	public partial class UriImageSource : Maui.IUriImageSource
	{
	}

	public partial class FontImageSource : Maui.IFontImageSource
	{
		Font IFontImageSource.Font => Font.OfSize(FontFamily, Size);
	}
}