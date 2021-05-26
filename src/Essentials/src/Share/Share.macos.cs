using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Microsoft.Maui.Graphics.Native;

namespace Microsoft.Maui.Essentials
{
	public static partial class Share
	{
		static Task PlatformRequestAsync(ShareTextRequest request)
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

		static Task PlatformRequestAsync(ShareMultipleFilesRequest request)
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
