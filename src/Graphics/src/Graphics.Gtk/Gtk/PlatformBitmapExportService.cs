namespace Microsoft.Maui.Graphics.Platform.Gtk
{
	public class PlatformBitmapExportService : IBitmapExportService
	{
		public BitmapExportContext CreateContext(int width, int height, float dpi)
		{
			return new PlatformBitmapExportContext(width, height, dpi);
		}
	}
}
