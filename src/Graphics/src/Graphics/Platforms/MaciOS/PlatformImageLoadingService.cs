using System.IO;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformImageLoadingService : IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return PlatformImage.FromStream(stream, formatHint);
		}

		public IImage FromStream(Stream stream, ImageLoadOptions options)
		{
			return PlatformImage.FromStream(stream, options);
		}
	}
}
