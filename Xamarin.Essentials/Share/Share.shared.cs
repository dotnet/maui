using System;
using System.IO;
using System.Threading.Tasks;
#if !NETSTANDARD1_0
using System.Drawing;
#endif

namespace Xamarin.Essentials
{
    public static partial class Share
    {
        public static Task RequestAsync(string text) =>
            RequestAsync(new ShareTextRequest(text));

        public static Task RequestAsync(string text, string title) =>
            RequestAsync(new ShareTextRequest(text, title));

        public static Task RequestAsync(ShareTextRequest request) =>
            PlatformRequestAsync(request);

        public static Task RequestAsync(ShareFileRequest request)
        {
            return PlatformRequestAsync(request);
        }
    }

    public abstract class ShareRequestBase
    {
        public string Title { get; set; }

#if !NETSTANDARD1_0
        public Rectangle PresentationSourceBounds { get; set; } = Rectangle.Empty;
#endif
    }

    public class ShareTextRequest : ShareRequestBase
    {
        public ShareTextRequest()
        {
        }

        public ShareTextRequest(string text) => Text = text;

        public ShareTextRequest(string text, string title)
            : this(text) => Title = title;

        public string Subject { get; set; }

        public string Text { get; set; }

        public string Uri { get; set; }
    }

    public class ShareFileRequest : ShareRequestBase
    {
        public ShareFileRequest()
        {
        }

        public ShareFileRequest(string title, ShareFile file)
        {
            Title = title;
            File = file;
        }

        public ShareFileRequest(string title, FileBase file)
        {
            Title = title;
            File = new ShareFile(file);
        }

        public ShareFileRequest(ShareFile file)
        {
            File = file;
        }

        public ShareFileRequest(FileBase file)
        {
            File = new ShareFile(file);
        }

        public ShareFile File { get; set; }
    }

    public class ShareFile : FileBase
    {
        public ShareFile(string fullPath)
            : base(fullPath)
        {
        }

        public ShareFile(string fullPath, string contentType)
            : base(fullPath, contentType)
        {
        }

        public ShareFile(FileBase file)
            : base(file)
        {
        }
    }
}
