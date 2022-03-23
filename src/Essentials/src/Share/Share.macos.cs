using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ShareImplementation : IShare
	{
		Task PlatformRequestAsync(ShareTextRequest request)
		{
			var items = new List<NSObject>();
			if (!string.IsNullOrWhiteSpace(request.Title))
				items.Add(new NSString(request.Title));
			if (!string.IsNullOrWhiteSpace(request.Text))
				items.Add(new NSString(request.Text));
			if (!string.IsNullOrWhiteSpace(request.Uri))
				items.Add(NSUrl.FromString(request.Uri));

			return PlatformShowRequestAsync(request, items);
		}

		Task PlatformRequestAsync(ShareFileRequest request) =>
			PlatformRequestAsync((ShareMultipleFilesRequest)request);

		Task PlatformRequestAsync(ShareMultipleFilesRequest request)
		{
			var items = new List<NSObject>();

			if (!string.IsNullOrWhiteSpace(request.Title))
				items.Add(new NSString(request.Title));

			foreach (var file in request.Files)
				items.Add(NSUrl.FromFilename(file.FullPath));

			return PlatformShowRequestAsync(request, items);
		}

		static Task PlatformShowRequestAsync(ShareRequestBase request, List<NSObject> items)
		{
			var window = Platform.GetCurrentWindow();
			var view = window.ContentView;

			var rect = request.PresentationSourceBounds.AsCGRect();
			rect.Y = view.Bounds.Height - rect.Bottom;

			var picker = new NSSharingServicePicker(items.ToArray());
			picker.ShowRelativeToRect(rect, view, NSRectEdge.MinYEdge);

			return Task.CompletedTask;
		}
	}
}
