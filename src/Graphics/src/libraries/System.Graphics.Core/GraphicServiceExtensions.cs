using System.IO;

namespace System.Graphics
{
    public static class GraphicServiceExtensions
    {
        public static IImage LoadImageFromBytes(this IGraphicsService target, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return target.LoadImageFromStream(stream);
            }
        }
    }
}