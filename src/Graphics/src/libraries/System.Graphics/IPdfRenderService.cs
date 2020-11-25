using System.IO;

namespace System.Graphics
{
    public interface IPdfRenderService
    {
        IPdfPage CreatePage(Stream stream, int pageNumber = -1);
    }
}