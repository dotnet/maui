using System;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri)
            => Task.FromResult(uri.IsWellFormedOriginalString());

        static Task PlatformOpenAsync(Uri uri)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = new AppControl
            {
                Operation = AppControlOperations.ShareText,
                Uri = uri.AbsoluteUri
            };

            if (uri.AbsoluteUri.StartsWith("geo:"))
                appControl.Operation = AppControlOperations.Pick;
            else if (uri.AbsoluteUri.StartsWith("http"))
                appControl.Operation = AppControlOperations.View;
            else if (uri.AbsoluteUri.StartsWith("mailto:"))
                appControl.Operation = AppControlOperations.Compose;
            else if (uri.AbsoluteUri.StartsWith("sms:"))
                appControl.Operation = AppControlOperations.Compose;
            else if (uri.AbsoluteUri.StartsWith("tel:"))
                appControl.Operation = AppControlOperations.Dial;

            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }

        static Task PlatformOpenAsync(OpenFileRequest request)
        {
            if (string.IsNullOrEmpty(request.File.FullPath))
                throw new ArgumentNullException(nameof(request.File.FullPath));

            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = new AppControl
            {
                Operation = AppControlOperations.View,
            };

            if (!string.IsNullOrEmpty(request.File.FullPath))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/path", request.File.FullPath);

            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }

        static async Task<bool> PlatformTryOpenAsync(Uri uri)
        {
            var canOpen = await PlatformCanOpenAsync(uri);

            if (canOpen)
                await PlatformOpenAsync(uri);

            return canOpen;
        }
    }
}
