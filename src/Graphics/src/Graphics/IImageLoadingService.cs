using System.IO;

namespace Microsoft.Maui.Graphics
{
	public interface IImageLoadingService
	{
		IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png);
	}
}
