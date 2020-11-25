namespace System.Graphics.CoreGraphics
{
    public class NativePdfExportService : IPdfExportService
    {
        public static NativePdfExportService Instance = new NativePdfExportService();

        private NativePdfExportService()
        {
        }

        public PdfExportContext CreateContext(float width = -1, float height = -1)
        {
            return new NativePdfExportContext(width, height);
        }
    }
}