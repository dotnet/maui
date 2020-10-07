using System;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class Share
    {
        static Task PlatformRequestAsync(ShareTextRequest request)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = new AppControl
            {
                Operation = AppControlOperations.ShareText,
            };

            if (!string.IsNullOrEmpty(request.Text))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/text", request.Text);
            if (!string.IsNullOrEmpty(request.Uri))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/url", request.Uri);
            if (!string.IsNullOrEmpty(request.Subject))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/subject", request.Subject);
            if (!string.IsNullOrEmpty(request.Title))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/title", request.Title);

            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }

        static Task PlatformRequestAsync(ShareFileRequest request)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = new AppControl
            {
                Operation = AppControlOperations.ShareText,
            };

            if (!string.IsNullOrEmpty(request.File.FullPath))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/path", request.File.FullPath);
            if (!string.IsNullOrEmpty(request.Title))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/title", request.Title);

            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }
    }
}
