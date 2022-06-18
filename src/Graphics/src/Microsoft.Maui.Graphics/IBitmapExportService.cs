namespace Microsoft.Maui.Graphics
{
	public interface IBitmapExportService
	{
		BitmapExportContext CreateContext(int width, int height, float displayScale = 1);
	}
}
