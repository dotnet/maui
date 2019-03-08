using System;
using System.IO;

namespace Xamarin.Essentials
{
    public class MediaFile : IDisposable
    {
        public string Filepath { get; }

        public bool Disposed { get; private set; }

        internal MediaFile(string filePath)
        {
            Filepath = filePath;
        }

#if !NETSTANDARD1_0
        public Stream AsStream() => File.Open(Filepath, FileMode.Open, FileAccess.Read);
#endif

        ~MediaFile()
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
                File.Delete(Filepath);
#endif
            Disposed = true;
        }
    }
}
