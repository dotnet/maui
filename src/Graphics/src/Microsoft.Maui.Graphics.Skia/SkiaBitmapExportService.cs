namespace Microsoft.Maui.Graphics.Skia
{
	public class PlatformBitmapExportService : IBitmapExportService
	{
		public BitmapExportContext CreateContext(int width, int height, float displayScale = 1)
		{
			return new SkiaBitmapExportContext(width, height, displayScale, 72, false);
		}
	}
}
