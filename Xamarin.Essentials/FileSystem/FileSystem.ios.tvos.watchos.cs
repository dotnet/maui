using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    class UIImageFileResult : FileResult
    {
        readonly UIImage uiImage;
        NSData data;

        internal UIImageFileResult(UIImage image)
            : base()
        {
            uiImage = image;

            FullPath = Guid.NewGuid().ToString() + ".png";
            FileName = FullPath;
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            data ??= uiImage.AsPNG();

            return Task.FromResult(data.AsStream());
        }
    }
}
