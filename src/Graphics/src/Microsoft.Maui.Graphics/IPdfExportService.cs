namespace Microsoft.Maui.Graphics
{
    public interface IPdfExportService
    {
        PdfExportContext CreateContext(float width = -1, float height = -1);
    }
}