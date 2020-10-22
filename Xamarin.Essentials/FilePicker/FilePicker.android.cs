using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Provider;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            // we only need the permission when accessing the file, but it's more natural
            // to ask the user first, then show the picker.
            await Permissions.RequestAsync<Permissions.StorageRead>();

            // Essentials supports >= API 19 where this action is available
            var action = Intent.ActionOpenDocument;

            var intent = new Intent(action);
            intent.SetType("*/*");
            intent.AddFlags(ActivityFlags.GrantPersistableUriPermission);
            intent.PutExtra(Intent.ExtraAllowMultiple, allowMultiple);

            var allowedTypes = options?.FileTypes?.Value?.ToArray();
            if (allowedTypes?.Length > 0)
                intent.PutExtra(Intent.ExtraMimeTypes, allowedTypes);

            var pickerIntent = Intent.CreateChooser(intent, options?.PickerTitle ?? "Select file");

            try
            {
                var result = await IntermediateActivity.StartAsync(pickerIntent, Platform.requestCodeFilePicker);
                var resultList = new List<FileResult>();

                var clipData = new List<global::Android.Net.Uri>();

                if (result.ClipData == null)
                {
                    clipData.Add(result.Data);
                }
                else
                {
                    for (var i = 0; i < result.ClipData.ItemCount; i++)
                        clipData.Add(result.ClipData.GetItemAt(i).Uri);
                }

                foreach (var contentUri in clipData)
                {
                    Platform.AppContext.ContentResolver.TakePersistableUriPermission(
                        contentUri,
                        ActivityFlags.GrantReadUriPermission);

                    resultList.Add(new FileResult(contentUri));
                }

                return resultList;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }

    public partial class FilePickerFileType
    {
        static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "image/png", "image/jpeg" } }
            });

        static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "image/png" } }
            });

        static FilePickerFileType PlatformJpegFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "image/jpeg" } }
            });

        static FilePickerFileType PlatformVideoFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "video/*" } }
            });

        static FilePickerFileType PlatformPdfFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/pdf" } }
            });
    }
}
