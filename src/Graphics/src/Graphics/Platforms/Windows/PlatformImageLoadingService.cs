using System;
using System.ComponentModel;
using System.IO;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
#if MAUI_GRAPHICS_WIN2D
	[System.Obsolete("Use Microsoft.Maui.Graphics.Platform.PlatformImageLoadingService instead.")]
	public class W2DImageLoadingService
#else
	public class PlatformImageLoadingService
#endif
		: IImageLoadingService
	{
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return PlatformImage.FromStream(stream, formatHint);
		}
	}

#if MAUI_GRAPHICS_WIN2D
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SkiaImageLoadingService : W2DImageLoadingService { }
#endif
}
