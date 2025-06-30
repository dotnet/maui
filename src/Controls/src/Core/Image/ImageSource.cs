#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>Abstract class whose implementors load images from files or the Web.</summary>
	public partial class ImageSource : IImageSource
	{
	}

	/// <summary>An <see cref="Microsoft.Maui.Controls.ImageSource"/> that reads an image from a file.</summary>
	public partial class FileImageSource : IFileImageSource
	{
	}

	/// <summary><see cref="Microsoft.Maui.Controls.ImageSource"/> that loads an image from a <see cref="System.IO.Stream"/>.</summary>
	public partial class StreamImageSource : IStreamImageSource
	{
	}

	/// <summary>An ImageSource that loads an image from a URI, caching the result.</summary>
	public partial class UriImageSource : IUriImageSource, IStreamImageSource
	{
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/FontImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.FontImageSource']/Docs/*" />
	public partial class FontImageSource : IFontImageSource
	{
		Font IFontImageSource.Font => Font.OfSize(FontFamily, Size, enableScaling: FontAutoScalingEnabled);
	}
}