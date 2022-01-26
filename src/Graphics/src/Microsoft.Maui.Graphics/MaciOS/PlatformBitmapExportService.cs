namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformBitmapExportService : IBitmapExportService
	{
		public BitmapExportContext CreateContext(int width, int height, float displayScale = 1)
		{
			return new PlatformBitmapExportContext(width, height, displayScale);
		}
	}
}
