using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using AndroidUri = Android.Net.Uri;
using Uri = System.Uri;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri)
        {
            var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri.OriginalString));

            if (Platform.AppContext == null)
                return Task.FromResult(false);

            var manager = Platform.AppContext.PackageManager;
            var supportedResolvedInfos = manager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return Task.FromResult(supportedResolvedInfos.Any());
        }

        static Task PlatformOpenAsync(Uri uri)
        {
            var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri.OriginalString));
            intent.SetFlags(ActivityFlags.ClearTop);
            intent.SetFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(intent);
            return Task.CompletedTask;
        }

        static Task PlatformOpenAsync(OpenFileRequest request)
        {
            var contentUri = Platform.GetShareableFileUri(request.File);

            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(contentUri, request.File.ContentType);
            intent.SetFlags(ActivityFlags.GrantReadUriPermission);

            var chooserIntent = Intent.CreateChooser(intent, request.Title ?? string.Empty);
            chooserIntent.SetFlags(ActivityFlags.ClearTop);
            chooserIntent.SetFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(chooserIntent);

            return Task.CompletedTask;
        }

        static async Task<bool> PlatformTryOpenAsync(Uri uri)
        {
            var canOpen = await PlatformCanOpenAsync(uri).ConfigureAwait(false);

            if (canOpen)
                await PlatformOpenAsync(uri).ConfigureAwait(false);

            return canOpen;
        }
    }
}
