using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Maui.Storage;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ShareImplementation : IShare
	{
		Task PlatformRequestAsync(ShareTextRequest request)
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
			intent.SetType(FileMimeTypes.TextPlain);
			intent.PutExtra(Intent.ExtraText, string.Join(System.Environment.NewLine, items));

			if (!string.IsNullOrWhiteSpace(request.Subject))
			{
				intent.PutExtra(Intent.ExtraSubject, request.Subject);
			}

			var chooserIntent = Intent.CreateChooser(intent, request.Title ?? string.Empty);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			chooserIntent.SetFlags(flags);
			Application.Context.StartActivity(chooserIntent);

			return Task.CompletedTask;
		}

		Task PlatformRequestAsync(ShareFileRequest request) =>
			PlatformRequestAsync((ShareMultipleFilesRequest)request);

		Task PlatformRequestAsync(ShareMultipleFilesRequest request)
		{
			var intent = CreateShareFileIntent(request);

			var chooserIntent = Intent.CreateChooser(intent, request.Title ?? string.Empty);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission;
			chooserIntent.SetFlags(flags);
			Application.Context.StartActivity(chooserIntent);

			return Task.CompletedTask;
		}

		// Extracted for testability — verifiable without launching an Activity.
		internal static Intent CreateShareFileIntent(ShareMultipleFilesRequest request)
		{
			var contentUris = new List<IParcelable>(request.Files.Count);
			var contentType = default(string);
			foreach (var file in request.Files)
			{
				contentUris.Add(FileSystemUtils.GetShareableFileUri(file));

				if (contentType == null)
					contentType = file.ContentType;
				else if (contentType != file.ContentType)
					contentType = FileMimeTypes.All;
			}

			var intentType = contentUris.Count > 1
				? Intent.ActionSendMultiple
				: Intent.ActionSend;
			var intent = new Intent(intentType);

			intent.SetType(contentType);
			intent.SetFlags(ActivityFlags.GrantReadUriPermission);

			if (contentUris.Count > 1)
			{
				intent.PutParcelableArrayListExtra(Intent.ExtraStream, contentUris);

				// Set ClipData so the system grants URI read permission to the receiving app.
				// Without this, Android 10+ logs a SecurityException when the share sheet
				// or target app tries to read the FileProvider content URI.
				if (OperatingSystem.IsAndroidVersionAtLeast(29))
				{
					var clipData = new ClipData(
						request.Title ?? string.Empty,
						new[] { contentType },
						new ClipData.Item((AndroidUri)contentUris[0]));

					for (int i = 1; i < contentUris.Count; i++)
					{
						clipData.AddItem(new ClipData.Item((AndroidUri)contentUris[i]));
					}

					intent.ClipData = clipData;
				}
			}
			else if (contentUris.Count == 1)
			{
				intent.PutExtra(Intent.ExtraStream, contentUris[0]);

				// Set ClipData so the system grants URI read permission to the receiving app.
				// Without this, Android 10+ logs a SecurityException when the share sheet
				// or target app tries to read the FileProvider content URI.
				if (OperatingSystem.IsAndroidVersionAtLeast(29))
				{
					intent.ClipData = ClipData.NewRawUri(request.Title ?? string.Empty, (AndroidUri)contentUris[0]);
				}
			}

			if (!string.IsNullOrEmpty(request.Title))
			{
				intent.PutExtra(Intent.ExtraTitle, request.Title);
			}

			return intent;
		}
	}
}
