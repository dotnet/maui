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
            var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri.AbsoluteUri));

            if (Platform.AppContext == null)
                return Task.FromResult(false);

            var manager = Platform.AppContext.PackageManager;
            var supportedResolvedInfos = manager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return Task.FromResult(supportedResolvedInfos.Any());
        }

        static Task PlatformOpenAsync(Uri uri)
        {
            var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri.AbsoluteUri));
            intent.SetFlags(ActivityFlags.ClearTop);
            intent.SetFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(intent);
            return Task.CompletedTask;
        }
    }
}
