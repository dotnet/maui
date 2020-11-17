using System.IO;

namespace System.Graphics
{
    public static class BitmapExportContextExtensions
    {
        public static void WriteToFile(this BitmapExportContext exportContext, string filename)
        {
            using (var outputStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                exportContext.WriteToStream(outputStream);
            }
        }
    }
}