using System;
using System.ComponentModel;
using System.IO;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform implementation of <see cref="IImageLoadingService"/> which
	/// loads images into a new <see cref="IImage"/> instance.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
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
