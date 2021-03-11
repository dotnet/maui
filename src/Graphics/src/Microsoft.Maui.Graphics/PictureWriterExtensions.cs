using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
    public static class PictureWriterExtensions
    {
        public static byte[] SaveAsBytes(this IPictureWriter target, IPicture picture)
        {
            if (target == null || picture == null)
                return null;

            using (var stream = new MemoryStream())
            {
                target.Save(picture, stream);
                return stream.ToArray();
            }
        }

        public static async Task<byte[]> SaveAsBytesAsync(this IPictureWriter target, IPicture picture)
        {
            if (target == null || picture == null)
                return null;

            using (var stream = new MemoryStream())
            {
                await target.SaveAsync(picture, stream);
                return stream.ToArray();
            }
        }

        public static string SaveAsBase64(this IPictureWriter target, IPicture picture)
        {
            if (target == null)
                return null;

            var bytes = target.SaveAsBytes(picture);
            return Convert.ToBase64String(bytes);
        }

        public static Stream SaveAsStream(this IPictureWriter target, IPicture picture)
        {
            if (target == null)
                return null;

            var bytes = target.SaveAsBytes(picture);
            return new MemoryStream(bytes);
        }
    }
}