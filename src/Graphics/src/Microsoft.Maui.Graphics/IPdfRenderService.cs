using System.IO;

namespace Microsoft.Maui.Graphics
{
    public interface IPdfRenderService
    {
        IPdfPage CreatePage(Stream stream, int pageNumber = -1);
    }
}