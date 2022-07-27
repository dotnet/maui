using System.IO;

namespace Microsoft.Maui.Graphics.Skia
{
	public class SkiaImageLoadingService : IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return SkiaImage.FromStream(stream, formatHint);
		}
	}
}
