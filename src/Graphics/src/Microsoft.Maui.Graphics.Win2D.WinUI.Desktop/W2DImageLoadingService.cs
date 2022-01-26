using System.IO;

namespace Microsoft.Maui.Graphics.Win2D
{
	public class SkiaImageLoadingService : IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return W2DImage.FromStream(stream, formatHint);
		}
	}
}
