namespace Microsoft.Maui.Graphics.Platform
{
	internal class PlatformPdfExportService : IPdfExportService
	{
		public PlatformPdfExportService()
		{
		}

		public PdfExportContext CreateContext(float width = -1, float height = -1)
		{
			return new PlatformPdfExportContext(width, height);
		}
	}
}
