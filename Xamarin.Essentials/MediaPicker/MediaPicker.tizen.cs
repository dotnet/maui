using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static async Task<MediaFile> PlatformShowPhotoPickerAsync(MediaPickerOptions options)
            => new MediaFile(await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            }));
    }
}
