using System;
using System.Diagnostics;
using System.IO;

namespace Xamarin.Essentials
{
    public class ScreenshotFile : ReadOnlyFile, IDisposable
    {
        public bool Disposed { get; private set; }

        internal ScreenshotFile(string filePath)
            : base(filePath, "image/png")
        {
        }

#if !NETSTANDARD1_0
        public Stream AsStream() => System.IO.File.Open(FullPath, FileMode.Open, FileAccess.Read);
#endif

        ~ScreenshotFile()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (Disposed)
                return;
#if !NETSTANDARD1_0
            if (isDisposing)
            {
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to delete temporary Screenshot file: {ex.Message}");
                }
            }
#endif
            Disposed = true;
        }
    }
}
