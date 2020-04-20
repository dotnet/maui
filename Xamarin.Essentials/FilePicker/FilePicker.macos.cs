using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AppKit;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<FilePickerResult> PlatformPickFileAsync(PickOptions options)
        {
            var openPanel = new NSOpenPanel
            {
                CanChooseFiles = true,
                AllowsMultipleSelection = false,
                CanChooseDirectories = false
            };

            SetFileTypes(options, openPanel);

            FilePickerResult result = null;
            var resultCount = openPanel.RunModal();
            if (resultCount == 1)
            {
                result = new FilePickerResult(openPanel.Urls[0].Path);
            }

            return Task.FromResult(result);
        }

        static Task<IEnumerable<FilePickerResult>> PlatformPickMultipleFilesAsync(PickOptions options)
        {
            var openPanel = new NSOpenPanel
            {
                CanChooseFiles = true,
                AllowsMultipleSelection = true,
                CanChooseDirectories = false
            };

            SetFileTypes(options, openPanel);

            var resultList = new List<FilePickerResult>();
            var result = openPanel.RunModal();
            if (result != 0)
            {
                foreach (var url in openPanel.Urls)
                {
                    resultList.Add(new FilePickerResult(url.Path));
                }
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
                    allowedFileTypes.Add(type);
                }
            }

            if (allowedFileTypes.Count == 0)
                allowedFileTypes.Add("*.*");

            panel.AllowedFileTypes = allowedFileTypes.ToArray();
        }
    }

    public partial class FilePickerFileType
    {
        public static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.macOS, new[] { "*.png", "*.jpg", "*.jpeg" } }
            });

        public static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.macOS, new[] { "*.png" } }
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
