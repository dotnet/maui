using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tizen;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<IEnumerable<FilePickerResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();
            Permissions.EnsureDeclared<Permissions.StorageRead>();

            var tcs = new TaskCompletionSource<IEnumerable<FilePickerResult>>();

            var appControl = new AppControl();
            appControl.Operation = AppControlOperations.Pick;
            appControl.ExtraData.Add(AppControlData.SectionMode, allowMultiple ? "multiple" : "single");
            appControl.LaunchMode = AppControlLaunchMode.Single;

            var fileType = options?.FileTypes?.Value?.FirstOrDefault();
            appControl.Mime = fileType ?? "*/*";

            var fileResults = new List<FilePickerResult>();

            AppControl.SendLaunchRequest(appControl, (request, reply, result) =>
            {
                var resultFiles = new List<FilePickerResult>();

                if (result == AppControlReplyResult.Succeeded)
                {
                    if (reply.ExtraData.Count() > 0)
                    {
                        var info = reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected).ToList();
                        resultFiles.Add(new FilePickerResult(info));
                    }
                }

                tcs.TrySetResult(resultFiles);
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
            await Permissions.RequestAsync<Permissions.StorageRead>();

            var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(stream).Result;
        }
    }
}
