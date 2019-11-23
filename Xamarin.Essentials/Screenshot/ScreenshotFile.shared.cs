using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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
        public Task<Stream> PlatformOpenReadStreamAsync()
        {
            // This is a truely async file stream. The buffer size parameter is the same as the default.
            return Task.FromResult<Stream>(
                new FileStream(FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true));
        }
#else
        public Task<Stream> PlatformOpenReadStreamAsync() => throw ExceptionUtils.NotSupportedOrImplementedException;
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
