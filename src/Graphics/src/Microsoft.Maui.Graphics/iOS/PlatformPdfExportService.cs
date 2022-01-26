namespace Microsoft.Maui.Graphics.Platform
{
	internal class PlatformPdfExportService : IPdfExportService
	{
		public PdfExportContext CreateContext(float width = -1, float height = -1)
			=> new PlatformPdfExportContext(width, height);
	}
}
