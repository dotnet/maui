namespace Microsoft.Maui.Graphics
{
	internal interface IPdfExportService
	{
		PdfExportContext CreateContext(float width = -1, float height = -1);
	}
}
