using System;
using System.ComponentModel;
using System.IO;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformImageLoadingService : IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return PlatformImage.FromStream(stream, formatHint);
		}
	}

	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SkiaImageLoadingService : PlatformImageLoadingService { }
}
