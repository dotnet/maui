using System.IO;
using System.Threading.Tasks;

namespace System.Graphics
{
    public static class PdfPageExtensions
    {
        public static byte[] AsBytes(this IPdfPage target)
        {
            if (target == null)
                return null;

            using (var stream = new MemoryStream())
            {
                target.Save(stream);
                return stream.ToArray();
            }
        }

        public static Stream AsStream(this IPdfPage target)
        {
            if (target == null)
                return null;

            var stream = new MemoryStream();
            target.Save(stream);
            stream.Position = 0;

            return stream;
        }

        public static async Task<byte[]> AsBytesAsync(this IPdfPage target)
        {
            if (target == null)
                return null;

            using (var stream = new MemoryStream())
            {
                await target.SaveAsync(stream);
                return stream.ToArray();
            }
        }
        
        public static string AsBase64(this IPdfPage target)
        {
            if (target == null)
                return null;

            var bytes = target.AsBytes();
            return Convert.ToBase64String(bytes);
        }
    }
}