using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<FilePickerResult> PlatformPickFileAsync(PickOptions options)
        {
            Permissions.EnsureDeclared(PermissionType.LaunchApp);
            Permissions.EnsureDeclared(PermissionType.ReadExternalStorage);

            var tcs = new TaskCompletionSource<FilePickerResult>();

            var appControl = new AppControl();
            appControl.Operation = AppControlOperations.Pick;
            appControl.ExtraData.Add(AppControlData.SectionMode, "single");
            appControl.LaunchMode = AppControlLaunchMode.Single;

            var fileType = options?.FileTypes?.Value?.FirstOrDefault();
            if (fileType == null)
            {
                appControl.Mime = "*/*";
            }
            else
            {
                appControl.Mime = fileType;
            }

            AppControl.SendLaunchRequest(appControl, (request, reply, result) =>
            {
                var pr = new FilePickerResult(new List<string>());
                if (result == AppControlReplyResult.Succeeded)
                {
                    if (reply.ExtraData.Count() > 0)
                    {
                        pr = new FilePickerResult(reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected).ToList());
                    }
                }
                tcs.TrySetResult(pr);
            });
            return tcs.Task;
        }
    }

    public partial class FilePickerFileType
    {
        public static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Tizen, new[] { "image/*" } },
            });

        public static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Tizen, new[] { "image/png" } }
            });
    }

    public partial class FilePickerResult
    {
        readonly string fullPath;

        internal FilePickerResult(IList<string> list)
            : base()
        {
            if (list == null || list.Count <= 0)
                throw new ArgumentNullException(nameof(list));

            if (list != null)
            {
                foreach (var path in list)
                {
                    fullPath = path;
                    FileName = string.Empty;
                    if (path.Count() > 0)
                    {
                        FileName = Path.GetFileName(path);
                    }
                }
            }
        }

        async Task<Stream> PlatformOpenReadStreamAsync()
        {
            await Permissions.RequestAsync(PermissionType.ReadExternalStorage);

            var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(stream).Result;
        }
    }
}
