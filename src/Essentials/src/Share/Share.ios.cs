using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using LinkPresentation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ShareImplementation : IShare
	{
		async Task PlatformRequestAsync(ShareTextRequest request)
		{
			var src = new TaskCompletionSource<bool>();
			var items = new List<NSObject>();
			if (!string.IsNullOrWhiteSpace(request.Text))
			{
				items.Add(GetShareItem(new NSString(request.Text), request.Title, request.PreviewImage?.FullPath));
			}

			if (!string.IsNullOrWhiteSpace(request.Uri))
			{
				items.Add(GetShareItem(NSUrl.FromString(request.Uri), request.Title, request.PreviewImage?.FullPath));
			}

			var activityController = new UIActivityViewController(items.ToArray(), null)
			{
				CompletionWithItemsHandler = (a, b, c, d) =>
				{
					src.TrySetResult(true);
				},
			};

			var vc = WindowStateManager.Default.GetCurrentUIViewController(true);

			if (activityController.PopoverPresentationController != null)
			{
				activityController.PopoverPresentationController.SourceView = vc.View;

				if (request.PresentationSourceBounds != Rect.Zero || OperatingSystem.IsIOSVersionAtLeast(13, 0))
					activityController.PopoverPresentationController.SourceRect = request.PresentationSourceBounds.AsCGRect();
			}

			await vc.PresentViewControllerAsync(activityController, true);
			await src.Task;
		}

		Task PlatformRequestAsync(ShareFileRequest request) =>
			PlatformRequestAsync((ShareMultipleFilesRequest)request);

		async Task PlatformRequestAsync(ShareMultipleFilesRequest request)
		{
			var src = new TaskCompletionSource<bool>();
			var items = new List<NSObject>();

			foreach (var file in request.Files)
			{
				var fileUrl = NSUrl.FromFilename(file.FullPath);
				items.Add(GetShareItem(fileUrl, request.Title));
			}

			var activityController = new UIActivityViewController(items.ToArray(), null)
			{
				CompletionWithItemsHandler = (a, b, c, d) =>
				{
					src.TrySetResult(true);
				}
			};

			var vc = WindowStateManager.Default.GetCurrentUIViewController();

			if (activityController.PopoverPresentationController != null)
			{
				activityController.PopoverPresentationController.SourceView = vc.View;

				if (request.PresentationSourceBounds != Rect.Zero || OperatingSystem.IsIOSVersionAtLeast(13, 0))
					activityController.PopoverPresentationController.SourceRect = request.PresentationSourceBounds.AsCGRect();
			}

			await vc.PresentViewControllerAsync(activityController, true);
			await src.Task;
		}

		NSObject GetShareItem(NSString obj, string title, string previewImagePath = null)
			=> new ShareActivityItemSource(obj, string.IsNullOrWhiteSpace(title) ? obj : title, previewImagePath);

		NSObject GetShareItem(NSObject obj, string title, string previewImagePath = null)
			=> string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(previewImagePath)
				? obj
				: new ShareActivityItemSource(obj, title, previewImagePath);
	}

	class ShareActivityItemSource : UIActivityItemSource
	{
		readonly NSObject item;
		readonly string title;
		readonly string previewImagePath;

		internal ShareActivityItemSource(NSObject item, string title, string previewImagePath = null)
		{
			this.item = item;
			this.title = title;
			this.previewImagePath = previewImagePath;
		}

		public override NSObject GetItemForActivity(UIActivityViewController activityViewController, NSString activityType) => item;

		public override NSObject GetPlaceholderData(UIActivityViewController activityViewController) => item;

		public override LPLinkMetadata GetLinkMetadata(UIActivityViewController activityViewController)
		{
			var meta = new LPLinkMetadata();
			if (!string.IsNullOrWhiteSpace(title))
				meta.Title = title;
			if (item is NSUrl url)
				meta.Url = url;

			if (!string.IsNullOrWhiteSpace(previewImagePath))
			{
				var image = UIImage.FromFile(previewImagePath);
				if (image != null)
				{
					meta.ImageProvider = new NSItemProvider(image);
					meta.IconProvider = new NSItemProvider(image);
				}
			}

			return meta;
		}
	}
}
