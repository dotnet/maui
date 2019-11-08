using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static async Task<FilePickerResult> PlatformPickFileAsync(PickOptions options)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            SetFileTypes(options, picker);

            var file = await picker.PickSingleFileAsync();
            if (file == null)
                return null;

            StorageApplicationPermissions.FutureAccessList.Add(file);

            return new FilePickerResult(file);
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
                { DevicePlatform.UWP, new[] { "*.png", "*.jpg", "*.jpeg" } }
            });

        public static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.UWP, new[] { "*.png" } }
            });
    }

    public partial class PickerResultBase
    {
        readonly IStorageFile storageFile;

        internal PickerResultBase(IStorageFile storageFile)
        {
            this.storageFile = storageFile;
            FileName = storageFile.Name;
        }

        Task<Stream> PlatformOpenReadStreamAsync()
        {
            // we can make this assumption because
            // the only way to construct this object
            // is with an IStorageFile
            return storageFile.OpenStreamForReadAsync();
        }
    }

    public partial class FilePickerResult
    {
        internal FilePickerResult(IStorageFile storageFile)
            : base(storageFile)
        {
        }
    }
}
