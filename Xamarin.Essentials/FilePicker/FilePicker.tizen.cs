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
        static async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();
            await Permissions.EnsureGrantedAsync<Permissions.StorageRead>();

            var tcs = new TaskCompletionSource<IEnumerable<FileResult>>();

            var appControl = new AppControl();
            appControl.Operation = AppControlOperations.Pick;
            appControl.ExtraData.Add(AppControlData.SectionMode, allowMultiple ? "multiple" : "single");
            appControl.LaunchMode = AppControlLaunchMode.Single;

            var fileType = options?.FileTypes?.Value?.FirstOrDefault();
            appControl.Mime = fileType ?? FileSystem.MimeTypes.All;

            var fileResults = new List<FileResult>();

            AppControl.SendLaunchRequest(appControl, (request, reply, result) =>
            {
                if (result == AppControlReplyResult.Succeeded)
                {
                    if (reply.ExtraData.Count() > 0)
                    {
                        var selectedFiles = reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected).ToList();
                        fileResults.AddRange(selectedFiles.Select(f => new FileResult(f)));
                    }
                }

                tcs.TrySetResult(fileResults);
            });

            return await tcs.Task;
        }
    }

    public partial class FilePickerFileType
    {
        static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.ImageAll } },
            });

        static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.ImagePng } }
            });

        static FilePickerFileType PlatformJpegFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.ImageJpg } }
            });

        static FilePickerFileType PlatformVideoFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.VideoAll } }
            });

        static FilePickerFileType PlatformPdfFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.Pdf } }
            });
    }
}
