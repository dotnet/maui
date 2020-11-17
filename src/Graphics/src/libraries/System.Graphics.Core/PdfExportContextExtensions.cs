using System.IO;

namespace System.Graphics
{
    public static class PdfExportContextExtensions
    {
        public static void WriteToFile(this PdfExportContext exportContext, string filename)
        {
            using (var outputStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                exportContext.WriteToStream(outputStream);
            }
        }
    }
}