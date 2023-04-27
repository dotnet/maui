using System;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
#if MAUI_GRAPHICS_WIN2D
	[System.Obsolete("Use Microsoft.Maui.Graphics.Platform.PlatformBitmapExportService instead.")]
	public class W2DBitmapExportService
#else
	public class PlatformBitmapExportService
#endif
		: IBitmapExportService
	{
		public BitmapExportContext CreateContext(int width, int height, float displayScale = 1)
		{
			throw new NotImplementedException();
		}
	}
}
