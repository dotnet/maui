using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Microsoft.Caboodle
{
    public static partial class DataTransfer
    {
        public static Task RequestAsync(ShareTextRequest request)
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

        internal class ShareActivityItemSource : UIActivityItemSource
        {
            NSObject item;
            string subject;

            public ShareActivityItemSource(NSObject item, string subject)
            {
                this.item = item;
                this.subject = subject;
            }

            public override NSObject GetItemForActivity(UIActivityViewController activityViewController, NSString activityType) => item;

            public override NSObject GetPlaceholderData(UIActivityViewController activityViewController) => item;

            public override string GetSubjectForActivity(UIActivityViewController activityViewController, NSString activityType) => subject;
        }
    }
}
