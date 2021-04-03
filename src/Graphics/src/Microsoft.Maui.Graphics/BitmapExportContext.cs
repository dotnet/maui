using System;
using System.IO;

namespace Microsoft.Maui.Graphics
{
    public abstract class BitmapExportContext : IDisposable
    {
        public int Width { get; }
        public int Height { get; }
        public float Dpi { get; }

        protected BitmapExportContext(int width, int height, float dpi)
        {
            Width = width;
            Height = height;
            Dpi = dpi;
        }

        public virtual void Dispose()
        {
        }

        public abstract ICanvas Canvas { get; }

        public abstract void WriteToStream(Stream stream);

        public abstract IImage Image { get; }
    }
}
