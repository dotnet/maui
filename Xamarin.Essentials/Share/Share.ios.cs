using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Share
    {
        static Task PlatformRequestAsync(ShareTextRequest request)
        {
            var items = new List<NSObject>();
            if (!string.IsNullOrWhiteSpace(request.Text))
            {
                items.Add(new ShareActivityItemSource(new NSString(request.Text), request.Title));
            }

            if (!string.IsNullOrWhiteSpace(request.Uri))
            {
                items.Add(new ShareActivityItemSource(NSUrl.FromString(request.Uri), request.Title));
            }

            var activityController = new UIActivityViewController(items.ToArray(), null);

            var vc = Platform.GetCurrentViewController();

            if (activityController.PopoverPresentationController != null)
            {
                activityController.PopoverPresentationController.SourceView = vc.View;
            }

            return vc.PresentViewControllerAsync(activityController, true);
        }

        static Task PlatformRequestAsync(ShareFileRequest request)
        {
            var items = new List<NSObject>();

            var fileUrl = NSUrl.FromFilename(request.File.FullPath);
            if (!string.IsNullOrEmpty(request.Title))
                items.Add(new ShareActivityItemSource(fileUrl, request.Title)); // Share with title (subject)
            else
                items.Add(fileUrl); // No title specified

            var activityController = new UIActivityViewController(items.ToArray(), null);

            var vc = Platform.GetCurrentViewController();

            if (activityController.PopoverPresentationController != null)
            {
                activityController.PopoverPresentationController.SourceView = vc.View;
            }

            return vc.PresentViewControllerAsync(activityController, true);
        }
    }

    class ShareActivityItemSource : UIActivityItemSource
    {
        readonly NSObject item;
        readonly string subject;

        internal ShareActivityItemSource(NSObject item, string subject)
        {
            this.item = item;
            this.subject = subject;
        }

        public override NSObject GetItemForActivity(UIActivityViewController activityViewController, NSString activityType) => item;

        public override NSObject GetPlaceholderData(UIActivityViewController activityViewController) => item;

        public override string GetSubjectForActivity(UIActivityViewController activityViewController, NSString activityType) => subject;
    }
}
