using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;

namespace Xamarin.Essentials
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
            intent.SetType("text/plain");
            intent.PutExtra(Intent.ExtraText, string.Join(Environment.NewLine, items));

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

        static Task PlatformRequestAsync(ShareFileRequest request)
        {
            var contentUri = Platform.GetShareableFileUri(request.File);

            var intent = new Intent(Intent.ActionSend);
            intent.SetType(request.File.ContentType);
            intent.SetFlags(ActivityFlags.GrantReadUriPermission);
            intent.PutExtra(Intent.ExtraStream, contentUri);

            if (!string.IsNullOrEmpty(request.Title))
            {
                intent.PutExtra(Intent.ExtraTitle, request.Title);
            }

            var chooserIntent = Intent.CreateChooser(intent, request.Title ?? string.Empty);
            var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
            chooserIntent.SetFlags(flags);
            Platform.AppContext.StartActivity(chooserIntent);

            return Task.CompletedTask;
        }
    }
}
