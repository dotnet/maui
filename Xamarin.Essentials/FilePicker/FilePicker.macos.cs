using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AppKit;
using MobileCoreServices;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<IEnumerable<FilePickerResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            var openPanel = new NSOpenPanel
            {
                CanChooseFiles = true,
                AllowsMultipleSelection = allowMultiple,
                CanChooseDirectories = false
            };

            if (options.PickerTitle != null)
                openPanel.Title = options.PickerTitle;

            SetFileTypes(options, openPanel);

            var resultList = new List<FilePickerResult>();
            var panelResult = openPanel.RunModal();
            if (panelResult == (nint)(long)NSModalResponse.OK)
            {
                foreach (var url in openPanel.Urls)
                    resultList.Add(new FilePickerResult(url.Path));
            }

            return Task.FromResult<IEnumerable<FilePickerResult>>(resultList);
        }

        static void SetFileTypes(PickOptions options, NSOpenPanel panel)
        {
            var allowedFileTypes = new List<string>();

            if (options?.FileTypes?.Value != null)
            {
                foreach (var type in options.FileTypes.Value)
                {
                    allowedFileTypes.Add(type.TrimStart('*', '.'));
                }
            }

            panel.AllowedFileTypes = allowedFileTypes.ToArray();
        }
    }

    public partial class FilePickerFileType
    {
        public static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.macOS, new string[] { UTType.PNG, UTType.JPEG, "jpeg" } }
            });

        public static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.macOS, new string[] { UTType.PNG } }
            });
    }

    public partial class FilePickerResult
    {
        internal FilePickerResult(string filePath)
            : base(filePath)
        {
            FileName = Path.GetFileName(filePath);
        }

        Task<Stream> PlatformOpenReadStreamAsync()
            => Task.FromResult<Stream>(File.OpenRead(FullPath));
    }
}
