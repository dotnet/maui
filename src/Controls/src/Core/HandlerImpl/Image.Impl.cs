namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Image.xml" path="Type[@FullName='Microsoft.Maui.Controls.Image']/Docs" />
	public partial class Image : IImage
	{
		IImageSource IImageSourcePart.Source => Source;

		void IImageSourcePart.UpdateIsLoading(bool isLoading) =>
			IsLoading = isLoading;
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/ImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.ImageSource']/Docs" />
	public partial class ImageSource : IImageSource
	{
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/FileImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.FileImageSource']/Docs" />
	public partial class FileImageSource : IFileImageSource
	{
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/StreamImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.StreamImageSource']/Docs" />
	public partial class StreamImageSource : IStreamImageSource
	{
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/UriImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.UriImageSource']/Docs" />
	public partial class UriImageSource : IUriImageSource, IStreamImageSource
	{
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/FontImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.FontImageSource']/Docs" />
	public partial class FontImageSource : IFontImageSource
	{
		Font IFontImageSource.Font => Font.OfSize(FontFamily, Size, enableScaling: FontAutoScalingEnabled);
	}
}