using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;
using UIKit;

namespace Microsoft.Maui.Essentials
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

				if (request.PresentationSourceBounds != Rectangle.Zero || Platform.HasOSVersion(13, 0))
					activityController.PopoverPresentationController.SourceRect = request.PresentationSourceBounds.AsCGRect();
			}

			return vc.PresentViewControllerAsync(activityController, true);
		}

		static Task PlatformRequestAsync(ShareMultipleFilesRequest request)
		{
			var items = new List<NSObject>();

			var hasTitel = !string.IsNullOrWhiteSpace(request.Title);
			foreach (var file in request.Files)
			{
				var fileUrl = NSUrl.FromFilename(file.FullPath);
				if (hasTitel)
					items.Add(new ShareActivityItemSource(fileUrl, request.Title)); // Share with title (subject)
				else
					items.Add(fileUrl); // No title specified
			}

			var activityController = new UIActivityViewController(items.ToArray(), null);

			var vc = Platform.GetCurrentViewController();

			if (activityController.PopoverPresentationController != null)
			{
				activityController.PopoverPresentationController.SourceView = vc.View;

				if (request.PresentationSourceBounds != Rectangle.Zero)
					activityController.PopoverPresentationController.SourceRect = request.PresentationSourceBounds.AsCGRect();
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
