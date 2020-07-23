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
        static async Task<IEnumerable<FilePickerResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();
            await Permissions.EnsureGrantedAsync<Permissions.StorageRead>();

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
                if (result == AppControlReplyResult.Succeeded)
                {
                    if (reply.ExtraData.Count() > 0)
                    {
                        var selectedFiles = reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected).ToList();
                        fileResults.AddRange(selectedFiles.Select(f => new FilePickerResult(f)));
                    }
                }

                tcs.TrySetResult(fileResults);
            });

            return await tcs.Task;
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
        internal FilePickerResult(string fullPath)
            : base(fullPath)
        {
        }

        async Task<Stream> PlatformOpenReadAsync()
        {
            await Permissions.RequestAsync<Permissions.StorageRead>();

            var stream = File.Open(FullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(stream).Result;
        }
    }
}
