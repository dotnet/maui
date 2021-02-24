using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;

namespace Microsoft.Maui.Essentials
{
	public static partial class Share
	{
		static Task PlatformRequestAsync(ShareTextRequest request)
		{
			var items = new List<string>();
			if (!string.IsNullOrWhiteSpace(request.Text))
			{
				items.Add(request.Text);
			}

			if (!string.IsNullOrWhiteSpace(request.Uri))
			{
				items.Add(request.Uri);
			}

			var intent = new Intent(Intent.ActionSend);
			intent.SetType(FileSystem.MimeTypes.TextPlain);
			intent.PutExtra(Intent.ExtraText, string.Join(System.Environment.NewLine, items));

			if (!string.IsNullOrWhiteSpace(request.Subject))
			{
				intent.PutExtra(Intent.ExtraSubject, request.Subject);
			}

			var chooserIntent = Intent.CreateChooser(intent, request.Title ?? string.Empty);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			chooserIntent.SetFlags(flags);
			Platform.AppContext.StartActivity(chooserIntent);

			return Task.CompletedTask;
		}

		static Task PlatformRequestAsync(ShareMultipleFilesRequest request)
		{
			// load the data we need
			var contentUris = new List<IParcelable>(request.Files.Count);
			var contentType = default(string);
			foreach (var file in request.Files)
			{
				contentUris.Add(Platform.GetShareableFileUri(file));

				if (contentType == null)
					contentType = file.ContentType;
				else if (contentType != file.ContentType)
					contentType = FileSystem.MimeTypes.All;
			}

			var intentType = contentUris.Count > 1
				? Intent.ActionSendMultiple
				: Intent.ActionSend;
			var intent = new Intent(intentType);

			intent.SetType(contentType);
			intent.SetFlags(ActivityFlags.GrantReadUriPermission);

			if (contentUris.Count > 1)
				intent.PutParcelableArrayListExtra(Intent.ExtraStream, contentUris);
			else if (contentUris.Count == 1)
				intent.PutExtra(Intent.ExtraStream, contentUris[0]);

			if (!string.IsNullOrEmpty(request.Title))
				intent.PutExtra(Intent.ExtraTitle, request.Title);

			var chooserIntent = Intent.CreateChooser(intent, request.Title ?? string.Empty);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			chooserIntent.SetFlags(flags);
			Platform.AppContext.StartActivity(chooserIntent);

			return Task.CompletedTask;
		}
	}
}
