using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static async Task<PickResult> PlatformPickFileAsync(PickOptions options)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            SetFileTypes(options, picker);

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return null;
            }

            StorageApplicationPermissions.FutureAccessList.Add(file);
            return new PickResult(file.Path, file.Name, file);
        }

        static void SetFileTypes(PickOptions options, Windows.Storage.Pickers.FileOpenPicker picker)
        {
            var hasAtLeastOneType = false;

            if (options?.FileTypes?.Length > 0)
            {
                foreach (var type in options.FileTypes)
                {
                    if (type.StartsWith("."))
                    {
                        picker.FileTypeFilter.Add(type);
                        hasAtLeastOneType = true;
                    }
                }
            }

            if (!hasAtLeastOneType)
            {
                picker.FileTypeFilter.Add("*");
            }
        }
    }

    public partial class PickOptions
    {
        static PickOptions PlatformGetImagesPickOptions()
        {
            return new PickOptions
            {
                FileTypes = new string[] { ".jpg", ".png" }
            };
        }
    }

    public partial class PickResult
    {
        StorageFile storageFile;

        internal PickResult(string path, string filename, StorageFile storageFile)
            : this(path, filename)
        {
            this.storageFile = storageFile;
        }

        Stream PlatformGetStream()
        {
            return storageFile.OpenStreamForReadAsync().Result;
        }
    }
}
