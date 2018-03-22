using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Content;

namespace Microsoft.Caboodle
{
    public static partial class DataTransfer
    {
        public static Task RequestAsync(ShareTextRequest request)
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
            chooserIntent.SetFlags(ActivityFlags.ClearTop);
            chooserIntent.SetFlags(ActivityFlags.NewTask);
            Platform.CurrentContext.StartActivity(chooserIntent);

            return Task.CompletedTask;
        }
    }
}
