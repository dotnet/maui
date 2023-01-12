using System;
using System.ComponentModel;
using System.IO;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	public class W2DImageLoadingService : IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return W2DImage.FromStream(stream, formatHint);
		}
	}

	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SkiaImageLoadingService : W2DImageLoadingService { }
}
