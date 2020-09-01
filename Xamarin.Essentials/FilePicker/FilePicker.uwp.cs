using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            SetFileTypes(options, picker);

            var resultList = new List<StorageFile>();

            if (allowMultiple)
            {
                var fileList = await picker.PickMultipleFilesAsync();
                if (fileList != null)
                    resultList.AddRange(fileList);
            }
            else
            {
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                    resultList.Add(file);
            }

            foreach (var file in resultList)
                StorageApplicationPermissions.FutureAccessList.Add(file);

            return resultList.Select(storageFile => new FileResult(storageFile));
        }

        static void SetFileTypes(PickOptions options, FileOpenPicker picker)
        {
            var hasAtLeastOneType = false;

            if (options?.FileTypes?.Value != null)
            {
                foreach (var type in options.FileTypes.Value)
                {
                    if (type.StartsWith(".") || type.StartsWith("*."))
                    {
                        picker.FileTypeFilter.Add(type.TrimStart('*'));
                        hasAtLeastOneType = true;
                    }
                }
            }

            if (!hasAtLeastOneType)
                picker.FileTypeFilter.Add("*");
        }
    }

    public partial class FilePickerFileType
    {
        public static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.UWP, new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp" } }
            });

        public static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.UWP, new[] { "*.png" } }
            });

        public static FilePickerFileType PlatformVideoFileType() =>
           new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
           {
                { DevicePlatform.UWP, new[] { "*.mp4", "*.mov", "*.avi", "*.wmv", "*.m4v", "*.mpg", "*.mpeg", "*.mp2", "*.mkv", "*.flv", "*.gifv", "*.qt" } }
           });
    }
}
